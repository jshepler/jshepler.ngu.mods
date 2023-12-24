using System.Collections.Generic;
using System;
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

        // this is needed because the game assumes the number will never be < 0 and so large negative numbers wouldn't
        // be displayed in scientific notation and I want to display negative net gps if consumption > incoming gps
        [HarmonyPostfix, HarmonyPatch(typeof(NumberOutput), "sciFormat")]
        private static void NumberOutput_sciFormat(ref double number, ref string __result)
        {
            __result = (System.Math.Abs(number) < 1000000.0) ? number.ToString("###,##0") : number.ToString("e3");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NumberOutput), "realSuffixFormat")]
        private static bool NumberOutput_realSuffixFormat_prefix(ref double number, ref string __result, Dictionary<int, string> ___suffixString)
        {
            if (number >= 0)
                return true;

            var abs = Math.Abs(number);

            if (abs < 1.0)
                __result = number.ToString();

            else if (abs < 1000000.0)
                __result = number.ToString("###,##0");

            else
            {
                var log = (int)Math.Floor(Math.Log(abs, 1000.0));
                number /= Math.Pow(1000.0, log);

                __result = $"{number:###.000}{___suffixString[log]}";
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NumberOutput), "engineerFormat")]
        private static bool NumberOutput_engineerFormat_prefix(ref double number, ref string __result)
        {
            if (number >= 0)
                return true;

            var abs = Math.Abs(number);

            if (abs < 1000000.0)
                __result = number.ToString("###,##0");

            else
            {
                var num = Math.Floor(Math.Log10(abs) / 3.0) * 3.0;
                number /= Math.Pow(10.0, num);
                __result = number.ToString("###.000") + "E+" + num;
            }

            return false;
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
