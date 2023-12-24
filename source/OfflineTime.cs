using HarmonyLib;
using SFB;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class OfflineTime
    {
        private static bool _skipOfflineProgress = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OpenFileDialog), "loadFileMainMenuStandalone")]
        [HarmonyPatch(typeof(OpenFileDialog), "startLoadStandalone")]
        private static void OpenFileDialog_openLoadScreen_prefix()
        {
            _skipOfflineProgress = Input.GetKey(KeyCode.LeftShift);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "OpenFilePanel", typeof(string), typeof(string), typeof(string), typeof(bool))]
        private static void StandaloneFileBrowser_OpenFilePanel_prefix(ref string title)
        {
            if (_skipOfflineProgress)
                title += " (SKIPPIPNG OFFLINE PROGRESS)";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static bool OfflineTime_addOfflineProgress_prefix(int timeElapsed, Character __instance)
        {
            if (_skipOfflineProgress)
            {
                _skipOfflineProgress = false;
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
