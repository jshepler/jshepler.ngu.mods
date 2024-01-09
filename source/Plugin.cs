using System;
using System.Collections.Generic;
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
        internal static Color ButtonColor_Green = new Color32(40, 204, 45, 255);
        internal static Color ButtonColor_Yellow = new Color32(255, 244, 79, 255);
        internal static Color ButtonColor_Red = new Color32(216, 46, 63, 255);
        internal static Color ButtonColor_LightBlue = new Color32(99, 202, 216, 255);

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        private static CharacterEventArgs _cea = null;
        internal static event EventHandler<CharacterEventArgs> OnUpdate;
        internal static event EventHandler<CharacterEventArgs> OnLateUpdate;
        internal static event EventHandler<CharacterEventArgs> OnSaveLoaded;
        internal static event EventHandler<CharacterEventArgs> OnPreSave;
        internal static event EventHandler<CharacterEventArgs> onGUI;

        internal static Character Character = null;
        internal static Action<string> ShowNotification = m => Character?.tooltip.showOverrideTooltip(m, 3f);

        private void Awake()
        {
            Log = base.Logger;
            Options.Init(base.Config);

            harmony.PatchAll();
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            if (!_cea)
                return;
                
            OnUpdate?.Invoke(null, _cea);

            // hidden "Krissmuss" screen from christmas 2020 event
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                Character.menuSwapper.swapMenu((int)Menu.Krissmuss);
        }

        private void LateUpdate()
        {
            if (_cea) OnLateUpdate?.Invoke(null, _cea);
        }

        private void OnGUI()
        {
            if (_cea) onGUI?.Invoke(null, _cea);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            Character = __instance;
            _cea = new CharacterEventArgs(__instance);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix()
        {
            OnPreSave?.Invoke(null, _cea);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_postfix(Character __instance)
        {
            OnSaveLoaded?.Invoke(null, _cea);

            // unlocks krissmuss ui theme from christmass 2019 event
            __instance.settings.prizePicked = 6;
        }

        // when starting a new game, there is no offline progress and mods that rely on this event
        // won't be called and could have bad side-effects
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            OnSaveLoaded?.Invoke(null, _cea);
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
    }

    internal class CharacterEventArgs : EventArgs
    {
        public readonly Character Character;

        public CharacterEventArgs(Character c)
        {
            Character = c;
        }

        public static implicit operator bool(CharacterEventArgs cea)
        {
            return !object.ReferenceEquals(cea, null);
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
