using System.Reflection;
using HarmonyLib;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Hardcore
    {
        internal static bool Enabled = Options.GameModes.Hardcore.Value;

        internal static bool IsHardcoreGame
        {
            get => ModSave.Data.Hardcore;
            set => ModSave.Data.Hardcore = value;
        }

        private static bool _playerDied = false;

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "Awake")]
        private static void MainMenuController_Awake_postfix()
        {
            // disable the load local file button on the start screen (main menu)
            if (Enabled)
                GameObject.Find("Canvas/Box Canvas/Main Menu Screen/Load File Button").GetComponent<Button>().interactable = false;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                // if mode disabled, make sure game isn't flagged as Hardcore
                // this prevents starting a new game with it on, turning it off for whatever reason, then turning it back on
                if (!Enabled)
                    IsHardcoreGame = false;
            };
        }

        // even though the button is disabled above, clicking it still opens the file dialog - this blocks that
        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "OpenFilePanel", typeof(string), typeof(string), typeof(string), typeof(bool))]
        private static bool StandaloneFileBrowser_OpenFilePanel_prefix(ref string title)
        {
            return !Enabled;
        }

        // disable loading cloud save if HC is enabled and the save doesn't have the HC flag set to true
        [HarmonyPostfix, HarmonyPatch(typeof(SteamManager), "OnRemoteStorageFileReadAsyncComplete")]
        private static void SteamManager_OnRemoteStorageFileReadAsyncComplete_postfix()
        {
            if (!Enabled)
                return;

            var mm = Plugin.Character.mainMenu;
            var cloudPlayerDataField = typeof(MainMenuController).GetField("cloudPlayerData", BindingFlags.Instance | BindingFlags.NonPublic);
            var cloudPlayerData = cloudPlayerDataField.GetValue(mm) as ModSave.ModPlayerData;

            // should never be null, even if loading vanilla save, but check anyway in case the cast fails for some reason
            // if null or is hardcore save, allow it
            if (cloudPlayerData == null || (cloudPlayerData.Data.ContainsKey("HardCore") && (bool)cloudPlayerData.Data["HardCore"]))
                return;

            cloudPlayerDataField.SetValue(mm, null);
            mm.setCloudSaveValidity(false);
            mm.cloudInfo.text = "<b><color=red>HARDCORE MODE</color></b>\n\nCloud save is not hardcore\n\n";
        }

        // disable the load auto save button
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "updateAutosavePod")]
        private static void MainMenuController_updateAutosavePod_postfix(MainMenuController __instance)
        {
            if (Enabled)
            {
                __instance.loadAutosaveButton.interactable = false;
                __instance.autosaveInfo.text = "<b><color=red>HARDCORE MODE</color></b>\n\nLoading autosave is disabled";
            }
        }

        // a save doesn't get the flag set unless it's started with option enabled
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            IsHardcoreGame = Enabled;
        }

        // disable the load local save button on the lower-left of the main game screen (under spend exp button)
        [HarmonyPostfix, HarmonyPatch(typeof(OpenFileDialog), "Start")]
        private static void OpenFileDialog_Start_postfix(OpenFileDialog __instance)
        {
            if (Enabled)
                __instance.standaloneLoad.interactable = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "showLoadAdvice")]
        private static bool OpenFileDialog_showLoadAdvice_prefix(OpenFileDialog __instance)
        {
            if (!Enabled)
                return true;

            __instance.tooltip.showTooltip("<b><color=red>HARDCORE MODE</color></b>\n\nCannot load saves in hardcore");
            return false;
        }

        // this make sure steam cloud is always set to nothing after player has died, no matter how long player waits
        // i.e. if player waits for next auto save, it will still write an empty save
        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "saveGamestateToSteamCloud")]
        private static bool OpenFileDialog_saveGamestateToSteamCloud_prefix()
        {
            if (!Enabled || !_playerDied)
                return true;

            Plugin.Character.steamAPI.writeToSteamCloud([0]);
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "fight")]
        private static void BossController_fight_postfix()
        {
            if (Enabled && Plugin.Character.curHP <= 0)
                died();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "playerDeath")]
        private static void AdventureController_playerDeath_postfix()
        {
            if (Enabled)
                died();
        }

        private static void died()
        {
            _playerDied = true;
            Plugin.Character.saveLoad.saveGamestateToSteamCloud();
            Popups.GameOver.Show();
        }
    }
}
