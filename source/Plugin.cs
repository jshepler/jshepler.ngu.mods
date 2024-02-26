using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

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
        internal static Color ButtonColor_Green = new Color32(40, 204, 45, 255);
        internal static Color ButtonColor_Yellow = new Color32(255, 244, 79, 255);
        internal static Color ButtonColor_Red = new Color32(216, 46, 63, 255);
        internal static Color ButtonColor_LightBlue = new Color32(99, 202, 216, 255);

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        internal static event EventHandler OnUpdate;
        internal static event EventHandler OnFixedUpdate;
        internal static event EventHandler OnLateUpdate;
        internal static event EventHandler onGUI; // have to use onGUI instead of OnGUI because OnGUI is the method unity calls

        internal static event EventHandler OnSaveLoaded;
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

            // hidden "Krissmuss" screen from christmas 2020 event
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                Character.menuSwapper.swapMenu((int)Menu.Krissmuss);
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

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_postfix(Character __instance)
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);

            // unlocks krissmuss ui theme from christmass 2019 event
            __instance.settings.prizePicked = 6;

            CheckForNewVersion();
        }

        // when starting a new game, there is no offline progress and mods that rely on this event
        // won't be called and could have bad side-effects
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            OnSaveLoaded?.Invoke(null, EventArgs.Empty);
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

        private static Coroutine _checkForNewVersion;
        private static void CheckForNewVersion()
        {
            if (Options.CheckForNewVersion.Enabled.Value == false)
                return;

            if (_checkForNewVersion != null)
                Character.StopCoroutine(_checkForNewVersion);

            _checkForNewVersion = Character.StartCoroutine(CheckForNewVersionEx());
        }

        private static WaitForSeconds _waitForOneHour = new WaitForSeconds(3600f);
        private static IEnumerator CheckForNewVersionEx()
        {
            string json;

            while (true)
            {
                using (var req = UnityWebRequest.Get("https://api.github.com/repos/jshepler/jshepler.ngu.mods/releases/latest"))
                {
                    yield return req.SendWebRequest();

                    if (req.isNetworkError)
                    {
                        LogInfo($"CheckForUpdate(): network error: {req.error}");
                        yield break;
                    }

                    if (req.isHttpError)
                    {
                        LogInfo($"CheckForUpdate(): http error: {req.responseCode}");
                        yield break;
                    }

                    json = req.downloadHandler.text;
                }

                var data = JSONNode.Parse(json);
                if (data.HasKey("tag_name"))
                {
                    var tag = data["tag_name"].Value;
                    if (!string.IsNullOrWhiteSpace(tag) && tag != PluginInfo.PLUGIN_VERSION)
                        ShowOverrideNotification($"<b><color=blue>jshepler.ngu.mods</color></b>\n\nCurrent Version: <b>{PluginInfo.PLUGIN_VERSION}</b>\nLatest Version: <b>{tag}</b>", 10f);
                }

                yield return _waitForOneHour;
            }
        }

        internal class FocusEventArgs : EventArgs
        {
            internal bool HasFocus;

            public FocusEventArgs(bool hasFocus)
            {
                HasFocus = hasFocus;
            }
        }
    }
}

/*
notes:
    the load save button from startup screen calls: MainMenuController.loadFileSave() -> MainMenuController.loadFileKartridge() -> OpenFileDialog.loadFileMainMenuStandalone()
    the load save button bottom-left game screen calls: OpenFileDialog.startLoadStandalone()
    both eventually call ImportExport.loadData(SaveData)

    reg key: HKEY_CURRENT_USER\SOFTWARE\NGU Industries\NGU Idle
 */
