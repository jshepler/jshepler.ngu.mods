using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ResetTimePlayed
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Update")]
        private static void Character_Update_postfix(Character __instance)
        {
            if (!__instance.InMenu(Menu.Stats_Misc) && Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.F1))
            {
                __instance.totalPlaytime = new PlayerTime();
                Plugin.LogInfo("total play time reset");
            }
        }
    }
}
