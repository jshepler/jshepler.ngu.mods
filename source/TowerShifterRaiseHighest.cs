using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TowerShifterRaiseHighest
    {
        private static int _floor = 0;
        private static int _killCount = 0;

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static void AdventureController_enemyDeath_prefix(AdventureController __instance)
        {
            if (!__instance.character.arbitrary.boughtLazyITOPOD
                || !__instance.character.arbitrary.lazyITOPODOn) return;
            
            if (__instance.zone != 1000)
            {
                _floor = -1;
                _killCount = 0;

                return;
            }

            if (__instance.itopodLevel != _floor)
            {
                _floor = __instance.itopodLevel;
                _killCount = 1;
            }
            else
            {
                _killCount++;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static void AdventureController_enemyDeath_postfix(AdventureController __instance)
        {
            if (!__instance.character.arbitrary.boughtLazyITOPOD
                || !__instance.character.arbitrary.lazyITOPODOn
                || __instance.zone != 1000)
            {
                return;
            }

            if (_floor == (__instance.character.adventure.highestItopodLevel - 1) && _killCount >= 10)
            {
                __instance.character.adventure.highestItopodLevel++;
                __instance.itopod.awardHighestLevelPP(_floor + 1);
                __instance.activateLazyItopod();
            }
            else
            {
                __instance.itopodKillCount = _killCount;
            }
        }
    }
}
