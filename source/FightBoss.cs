using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FightBoss
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "Update")]
        private static bool Character_Update_prefix(Character __instance)
        {
            if (!__instance.InMenu(Menu.FightBoss) || !Input.GetKeyDown(KeyCode.F))
                return true;

            __instance.bossController.beginFight();
            return false;
        }
    }
}
