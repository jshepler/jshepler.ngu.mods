using System;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WalderpAttacksLeft
    {
        private static int _lastGrowCount = 0;
        private static Func<int, int> fnAttacksLeft = x => 7 - ((x + 4) % 6) - 2;

        [HarmonyPostfix, HarmonyPatch(typeof(EnemyAI), "waldoAI")]
        private static void EnemyAI_waldoAI_postfix(EnemyAI __instance)
        {
            if (__instance.growCount != _lastGrowCount)
            {
                var attacksLeft = fnAttacksLeft(__instance.growCount);
                var text = attacksLeft == 0 ? "[NEXT NEXT NEXT]"
                    : attacksLeft == 5 ? ""
                    : $"[{attacksLeft} more]";

                __instance.log.AddEvent(text, 3);
                _lastGrowCount = __instance.growCount;
            }
        }
    }
}
