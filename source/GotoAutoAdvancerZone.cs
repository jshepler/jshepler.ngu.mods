using System;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class GotoAutoAdvancerZone
    {
        [HarmonyPrefix, HarmonyPatch(typeof(ZoneForwardClick), "goToMaxZone", new Type[0])]
        private static bool ZoneForwardClick_goToMaxZone_prefix(ZoneForwardClick __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) || !Plugin.Character.arbitrary.advAdvancerBought)
                return true;

            __instance.goToMaxZone(Plugin.Character.arbitrary.advAdvancerZone);
            return false;
        }
    }
}
