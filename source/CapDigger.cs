using System;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CapDigger
    {
        [HarmonyPrefix, HarmonyPatch(typeof(AllGoldDiggerController), "setLevelMaxAffordable")]
        private static bool AllGoldDiggerController_setLevelMaxAffordable_prefix(int id, AllGoldDiggerController __instance)
        {
            var character = __instance.character;

            if (id < 0 || id > character.diggers.diggers.Count)
                return false;

            var digger = character.diggers.diggers[id];
            digger.curLevel = 0;

            var netGPS = __instance.character.goldPerSecond();
            var baseDrain = __instance.baseGPSDrain[id];
            var gpsGrowth = __instance.gpsGrowthRate[id];

            while (digger.curLevel < digger.maxLevel && netGPS > baseDrain * Math.Pow(gpsGrowth, digger.curLevel))
                digger.curLevel++;

            // I don't think it would be possible for an active digger to deactivate by clicking the cap button
            // (i.e. how would the digger already be running if gps isn't enough for even level 1),
            // still check for the possibility - the activateDigger() method actually toggles
            if ((digger.curLevel > 0 && !digger.active) || (digger.curLevel == 0 && digger.active))
                __instance.activateDigger(id);

            if (digger.active)
                Plugin.ShowNotification("This digger has been set to the highest level your GPS can handle!");

            __instance.refreshMenu();
            return false;
        }
    }
}
