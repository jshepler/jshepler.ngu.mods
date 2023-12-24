using System.Collections.Generic;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ZoneUnlockNotification
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "advanceBoss")]
        private static void BossController_advanceBoss_postfix(BossController __instance)
        {
            var bossId = Plugin.Character.effectiveBossID();

            var zones = new List<int>();
            for (var x = 0; x < zoneUnlockBossIds.Length; x++)
                if (zoneUnlockBossIds[x] == bossId)
                    zones.Add(x);

            zones.Do(z => __instance.tooltip.showTooltip($"unlocked zone: {Plugin.Character.adventureController.zoneName(z-1)}", 3));
        }

        private static int[] zoneUnlockBossIds = new[] { 0, 4, 7, 17, 37, 48, 58, 58, 66, 66, 74, 82, 82, 90, 100, 100, 108, 116, 116, 124, 132, 137, 359, 401, 426, 459, 467, 467, 475, 483, 491, 491, 501, 727, 752, 777, 810, 818, 826, 826, 834, 842, 850, 850, 871, 897, 902 };
    }
}
