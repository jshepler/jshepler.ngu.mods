using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class EffectiveGoldPerSecond
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AttackDefense), "updateGold")]
        private static void AttackDefense_updateGold_postfix(AttackDefense __instance)
        {
            var character = __instance.character;
            if (character.challenges.blindChallenge.inChallenge) return;

            var effectiveGPS = calcEffectiveGPS(character);
            if (effectiveGPS < 0)
                __instance.goldText.text += $"\n<color=#990000><b>{character.display(effectiveGPS)}/s</b></color>";
            else
                __instance.goldText.text += $"\n+{character.display(effectiveGPS)}/s";
        }

        private static double calcEffectiveGPS(Character character)
        {
            var bloodGPS = character.bloodMagicController.bloodMagics.Sum(bm => bm.goldConsumedPerSecond());
            if (bloodGPS < 0) bloodGPS = 0;

            var augGPS = character.augmentsController.augments.Sum(calcAugGPS);
            if (augGPS < 0) augGPS = 0;

            var timeMachineGPS = calcTimeMachineGPS(character.timeMachineController);
            if (timeMachineGPS < 0) timeMachineGPS = 0;

            // character.goldPerSecond() includes drain from diggers
            var netGPS = character.goldPerSecond() - (bloodGPS + augGPS + timeMachineGPS);

            return netGPS;
        }

        private static float calcAugGPS(AugmentController ac)
        {
            return augGPS(ac) + augUpgradeGPS(ac);
        }

        private static float augGPS(AugmentController ac)
        {
            var progressPerTick = ac.getAugProgressPerTick();
            if (progressPerTick < 1E-09f) return 0f;

            var barFillsPerSecond = 50f / Mathf.CeilToInt(1f / Mathf.Min(progressPerTick, 1f));
            var gps = barFillsPerSecond * ac.getAugCost();

            return gps;
        }

        private static float augUpgradeGPS(AugmentController ac)
        {
            var progressPerTick = ac.getUpgradeProgressPerTick();
            if (progressPerTick < 1E-09f) return 0f;

            var barFillsPerSecond = 50f / Mathf.CeilToInt(1f / Mathf.Min(progressPerTick, 1f));
            var gps = barFillsPerSecond * ac.getUpgradeCost();

            return gps;
        }

        private static float calcTimeMachineGPS(TimeMachineController tmc)
        {
            return tmMachineSpeedGPS(tmc) + tmGoldMultGPS(tmc);
        }

        private static float tmMachineSpeedGPS(TimeMachineController tmc)
        {
            var progressPerTick = tmc.speedProgressPerTick();
            if (progressPerTick < 1E-09f) return 0f;

            var barFillsPerSecond = 50f / Mathf.CeilToInt(1f / Mathf.Min(progressPerTick, 1f));
            var gps = barFillsPerSecond * tmc.machineSpeedGoldCost();

            return gps;
        }

        private static float tmGoldMultGPS(TimeMachineController tmc)
        {
            var progressPerTick = tmc.goldMultiProgressPerTick();
            if (progressPerTick < 1E-09f) return 0f;

            var barFillsPerSecond = 50f / Mathf.CeilToInt(1f / Mathf.Min(progressPerTick, 1f));
            var gps = barFillsPerSecond * tmc.machineGoldMultiCost();

            return gps;
        }
    }
}
