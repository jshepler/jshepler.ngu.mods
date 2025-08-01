using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        internal static readonly Color ButtonColor_Green = new Color32(40, 204, 45, 255); // #28cc2d
        internal static readonly Color ButtonColor_Yellow = new Color32(255, 244, 79, 255); // #fff44f
        internal static readonly Color ButtonColor_Red = new Color32(216, 46, 63, 255); // #d82e3f
        internal static readonly Color ButtonColor_LightBlue = new Color32(99, 202, 216, 255); // #63cad8

        internal const string TEXT_GREEN = "#28cc2d";
        internal const string TEXT_YELLOW = "#fff44f";
        internal const string TEXT_RED = "#d82e3f";
        internal const string TEXT_LIGHT_BLUE = "#63cad8";

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        internal static event EventHandler OnUpdate;
        internal static event EventHandler OnFixedUpdate;
        internal static event EventHandler OnLateUpdate;
        internal static event EventHandler onGUI; // have to use onGUI instead of OnGUI because OnGUI is the method unity calls

        internal static event EventHandler OnSaveLoaded;
        internal static event EventHandler OnSaveLoaded2;
        internal static event EventHandler OnOfflineProgressionComplete;
        internal static event EventHandler OnPreSave;
        internal static event EventHandler OnGameStart;
        internal static event EventHandler<FocusEventArgs> OnGameFocus;

        internal static Character Character = null;
        internal static bool GameHasStarted = false;
        internal static bool GameHasFocus = true;

        internal static bool AltIsDown => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        internal static bool ControlIsDown => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        internal static bool ShiftIsDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        internal static bool InputFieldHasFocus => EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>()?.isFocused ?? false;

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

            OnGameStart?.Invoke(null, EventArgs.Empty);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix()
        {
            OnPreSave?.Invoke(null, EventArgs.Empty);
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        //private static void Character_addOfflineProgress_prefix()

        // this is now called from ModSave/Patches.cs::LoadModData()
        // doing the postfix on finalTriggers would execute before the modData was loaded
        internal static void ImportExport_finalTriggers_postfix()
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);
            OnSaveLoaded2?.Invoke(null, EventArgs.Empty);
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
            OnSaveLoaded2?.Invoke(null, EventArgs.Empty);
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

        private static Text _tooltipText;
        [HarmonyPostfix, HarmonyPatch(typeof(HoverTooltip), "Start")]
        private static void HoverTooltip_Start_postfix(Text ___tooltipText)
        {
            _tooltipText = ___tooltipText;
        }

        internal static void SetTooltipFont(Font font)
        {
            _tooltipText.font = font;
        }

        internal static void ResetTooltipFont()
        {
            _tooltipText.font = Fonts.LiberationSans_Regular;
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
    the load local save button on the startup screen calls:
        MainMenuController.loadFileSave()
        ->  MainMenuController.loadFileKartridge()
            ->  OpenFileDialog.loadFileMainMenuStandalone()
                ->  OpenFileDialog.loadintoGame()
                    ->  ImportExport.loadData()
                    ->  Character.addOfflineProgress()

    the load autosave button on the startup screen calls:
        MainMenuController.loadAutosave()
        -> MainMenuController.loadAutosaveSteam()
            -> OpenFileDialog.loadintoGame()
                ->  ImportExport.loadData()
                ->  Character.addOfflineProgress()
                    ->  ImportExport.loadData()
                    ->  Character.addOfflineProgress()

    the load cloud save button on the startup screen calls:
        MainMenuController.loadCloudSave()
        -> MainMenuController.loadCloudSaveSteam()
            -> openFileDialog.loadintoGame()

    the load save button bottom-left of game screen calls:
        OpenFileDialog.startLoadStandalone()
        ->  OpenFileDialog.quickLoad()
            ->  importExport.loadBase64ToData()
                ->  ImportExport.loadData()
        ->  Character.addOfflineProgress()

    both eventually call ImportExport.loadData(SaveData) which updates the game from SaveData -> PlayerData

    reg key: HKEY_CURRENT_USER\SOFTWARE\NGU Industries\NGU Idle
 */
