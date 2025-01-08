using HarmonyLib;
using SFB;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class OfflineTime
    {
        internal static bool SkipOfflineProgress = false;

        [HarmonyPrefix,
            HarmonyPatch(typeof(OpenFileDialog), "loadFileMainMenuStandalone"),
            HarmonyPatch(typeof(OpenFileDialog), "startLoadStandalone"),
            HarmonyPatch(typeof(MainMenuController), "loadAutosaveSteam"),
            HarmonyPatch(typeof(MainMenuController), "loadCloudSaveSteam")]
        private static void OpenFileDialog_openLoadScreen_prefix()
        {
            SkipOfflineProgress = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "OpenFilePanel", typeof(string), typeof(string), typeof(string), typeof(bool))]
        private static void StandaloneFileBrowser_OpenFilePanel_prefix(ref string title)
        {
            if (SkipOfflineProgress)
                title += " (SKIPPING OFFLINE PROGRESS)";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static bool OfflineTime_addOfflineProgress_prefix(int timeElapsed, Character __instance)
        {
            if (SkipOfflineProgress)
            {
                SkipOfflineProgress = false;
                return false;
            }

            if (timeElapsed > 0)
            {
                var inv = __instance.inventory;
                var ictrl = __instance.inventoryController;

                inv.mergeTime.setTime((inv.mergeTime.totalseconds + timeElapsed) % ictrl.autoMergeTime());
                inv.boostTime.setTime((inv.boostTime.totalseconds + timeElapsed) % ictrl.autoBoostTime());
            }

            return true;
        }
    }
}
