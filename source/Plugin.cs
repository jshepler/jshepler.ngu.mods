using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        //internal static Color ButtonColor_Green = new Color(0.5f, 0.827f, 0.235f);
        //internal static Color ButtonColor_Yellow = new Color(1f, 0.827f, 0.235f);
        //internal static Color ButtonColor_Red = new Color(0.925f, 0.204f, 0.204f);
        //internal static Color ButtonColor_LightBlue = new Color32(127, 208, 255, 255);

        // https://www.schemecolor.com/blue-red-yellow-green.php
        internal static Color ButtonColor_Green = new Color32(40, 204, 45, 255); // #28cc2d
        internal static Color ButtonColor_Yellow = new Color32(255, 244, 79, 255); // #fff44f
        internal static Color ButtonColor_Red = new Color32(216, 46, 63, 255); // #d82e3f
        internal static Color ButtonColor_LightBlue = new Color32(99, 202, 216, 255); // #63cad8

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        internal static event EventHandler OnUpdate;
        internal static event EventHandler OnFixedUpdate;
        internal static event EventHandler OnLateUpdate;
        internal static event EventHandler onGUI; // have to use onGUI instead of OnGUI because OnGUI is the method unity calls

        internal static event EventHandler OnSaveLoaded;
        internal static event EventHandler OnOfflineProgressionComplete;
        internal static event EventHandler OnPreSave;
        internal static event EventHandler OnGameStart;
        internal static event EventHandler<FocusEventArgs> OnGameFocus;

        internal static Character Character = null;
        internal static bool GameHasStarted = false;
        internal static bool GameHasFocus = true;

        private void Awake()
        {
            // prevents the bepinex manager object (i.e. this plugin instance) from being destroyed after Awake()
            // https://github.com/aedenthorn/PlanetCrafterMods/issues/7
            // not needed for all games, but I'm not currently aware of anything that it would hurt
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;
            
            Log = base.Logger;
            Options.Init(base.Config);

            harmony.PatchAll();
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            if (Character == null)
                return;

            OnUpdate?.Invoke(null, EventArgs.Empty);
        }

        private void FixedUpdate()
        {
            if (Character == null)
                return;

            OnFixedUpdate?.Invoke(null, EventArgs.Empty);
        }

        private void LateUpdate()
        {
            if (Character == null)
                return;

            OnLateUpdate?.Invoke(null, EventArgs.Empty);
        }

        private void OnGUI()
        {
            if (Character == null)
                return;

            onGUI?.Invoke(null, EventArgs.Empty);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            GameHasFocus = hasFocus;
            OnGameFocus?.Invoke(null, new FocusEventArgs(hasFocus));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            Character = __instance;
            GameHasStarted = true;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 2)
            {
                for (var x = 1; x < args.Length - 1; x++)
                {
                    if (args[x] == "-game")
                    {
                        AutoSaves.GameName = args[x + 1];
                        Fullscreen.SetWindowTitle($"NGU Idle - {args[x + 1]}");
                        break;
                    }
                }
            }

            var width = Options.CustomResolution.Width.Value;
            var height = Options.CustomResolution.Height.Value;
            if (width > 0 && height > 0)
                Screen.SetResolution(width, height, false);

            if (Options.OverrideCulture.Enabled.Value == true)
            {
                var localString = Options.OverrideCulture.Locale.Value;
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo(localString, false);
                }

                catch { }
            }

            OnGameStart?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix()
        {
            OnPreSave?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_prefix()
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_postfix()
        {
            OnOfflineProgressionComplete?.Invoke(null, EventArgs.Empty);
        }

        // when starting a new game, there is no offline progress and mods that rely on this event
        // won't be called and could have bad side-effects
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);
            OnOfflineProgressionComplete?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyFinalizer, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_finalizer(Exception __exception)
        {
            if(__exception != null)
                LogInfo($"Character.addOfflineProgress threw exception:\n{__exception}");
        }

        // patch TextEditor so that typing past the end of a textbox will scroll the text appropriately
        //[HarmonyTranspiler, HarmonyPatch(typeof(UnityEngine.TextEditor), "UpdateScrollOffset")]
        private static IEnumerable<CodeInstruction> UpdateScrollOffset_trans(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .End()
                .Advance(-3)
                .RemoveInstructions(3);

            return cm.InstructionEnumeration();
        }

        //[HarmonyTranspiler, HarmonyPatch(typeof(UnityEngine.TextEditor), "position", MethodType.Setter)]
        private static IEnumerable<CodeInstruction> TextEditor_set_position_trans(IEnumerable<CodeInstruction> instructions)
        {
            var scrollOffset = typeof(UnityEngine.TextEditor).GetField("scrollOffset");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, scrollOffset))
                .Advance(-1)
                .RemoveInstructions(3);

            return cm.InstructionEnumeration();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MainMenuController), "updateMiscText")]
        private static bool MainMenuController_updateMiscText_prefix(MainMenuController __instance)
        {
            var build = __instance.character.getVersionAsString();
            __instance.buildText.text = $"<b>Build {build}</b>(jshepler mods {PluginInfo.PLUGIN_VERSION})";
            __instance.buildText.resizeTextForBestFit = true;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VersionNumbering), "Start")]
        private static bool VersionNumbering_Start_prefix(VersionNumbering __instance)
        {
            var build = __instance.character.getVersionAsString();
            __instance.versionNumber.text = $"<b>Build {build}</b>\n(jshepler mods {PluginInfo.PLUGIN_VERSION})";
            __instance.versionNumber.resizeTextForBestFit = true;
            return false;
        }

        internal static void ShowNotification(string text, float seconds = 3f)
        {
            Character?.tooltip.showTooltip(text, seconds);
        }

        internal static void ShowOverrideNotification(string text, float seconds = 3f)
        {
            Character?.tooltip.showOverrideTooltip(text, seconds);
        }

        internal static void ShowTooltip(string message)
        {
            Character?.tooltip.showTooltip(message);
        }

        internal static void ShowOverrideTooltip(string message)
        {
            Character?.tooltip.showOverrideTooltip(message);
        }

        internal static void HideTooltip()
        {
            Character?.tooltip.hideTooltip();
        }

        internal static Coroutine BeginCoroutine(IEnumerator routine)
        {
            return Character?.StartCoroutine(routine);
        }

        internal static void EndCoroutine(Coroutine routine)
        {
            Character?.StopCoroutine(routine);
        }

        internal class FocusEventArgs : EventArgs
        {
            internal bool HasFocus;

            public FocusEventArgs(bool hasFocus)
            {
                HasFocus = hasFocus;
            }
        }

        //[HarmonyTranspiler, HarmonyPatch(typeof(Character), "constantLevelGain")]
        private static IEnumerable<CodeInstruction> constantLevelGain(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)50))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_I8))
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_I8, 50L));

            return cm.InstructionEnumeration();
        }
    }
}

/*
notes:
    the load save button from startup screen calls:
        MainMenuController.loadFileSave()
        ->  MainMenuController.loadFileKartridge()
            ->  OpenFileDialog.loadFileMainMenuStandalone()
                ->  OpenFileDialog.loadIntoGame()
                    ->  ImportExport.loadData()
                    ->  Character.addOfflineProgress()

    the load save button bottom-left game screen calls:
        OpenFileDialog.startLoadStandalone()
        ->  OpenFileDialog.quickLoad()
            ->  importExport.loadBase64ToData()
                ->  ImportExport.loadData()
        ->  Character.addOfflineProgress()

    both eventually call ImportExport.loadData(SaveData) which updates the game from SaveData -> PlayerData

    reg key: HKEY_CURRENT_USER\SOFTWARE\NGU Industries\NGU Idle
 */
