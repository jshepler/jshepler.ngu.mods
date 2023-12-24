using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishTotalTime
    {
        private static Character _character;
        private static WishesController _controller;

        private const float BIAS = 0.17f;
        private static float SpeedDivider(int id) => _controller.properties[id].wishSpeedDivider;
        private static float SpeedBonus() => _controller.totalWishSpeedBonuses();
        private static float MinTime() => _controller.minimumWishTime();

        private static float EnergyFactor(int id) => Mathf.Pow(_character.totalEnergyPower() * (float)_character.wishes.wishes[id].energy, BIAS);
        private static float EnergyFactorMax() => Mathf.Pow(_character.totalEnergyPower() * (float)_character.totalCapEnergy(), BIAS);
        private static float MagicFactor(int id) => Mathf.Pow(_character.totalMagicPower() * (float)_character.wishes.wishes[id].magic, BIAS);
        private static float MagicFactorMax() => Mathf.Pow(_character.totalMagicPower() * (float)_character.totalCapMagic(), BIAS);
        private static float Res3Factor(int id) => Mathf.Pow(_character.totalRes3Power() * (float)_character.wishes.wishes[id].res3, BIAS);
        private static float Res3FactorMax() => Mathf.Pow(_character.totalRes3Power() * (float)_character.totalCapRes3(), BIAS);

        private static float TimeToLevel(int id) => (1f - _character.wishes.wishes[id].progress) / _controller.progressPerTick(id) / 50f;
        private static float TimeToLevelMax(int id) => (1f - _character.wishes.wishes[id].progress) / _controller.progressPerTickMax(id) / 50f;

        private static bool IsRunning(int id) => _character.wishes.wishes[id].energy > 0 && _character.wishes.wishes[id].magic > 0 && _character.wishes.wishes[id].res3 > 0;

        private static Dictionary<int, float> TotalTimeRemaining(int id)
        {
            var minTime = _controller.minimumWishTime();
            var ppt = IsRunning(id)
                ? EnergyFactor(id) * MagicFactor(id) * Res3Factor(id) * SpeedBonus() / SpeedDivider(id)
                : EnergyFactorMax() * MagicFactorMax() * Res3FactorMax() * SpeedBonus() / SpeedDivider(id);

            var maxLevel = _controller.properties[id].maxLevel;
            var curLevel = _character.wishes.wishes[id].level;
            var time = new Dictionary<int, float>();

            if (curLevel == maxLevel)
                return time;

            time.Add(curLevel + 1, IsRunning(id) ? TimeToLevel(id) : TimeToLevelMax(id));
            for (var L = curLevel + 2; L < maxLevel + 1; L++)
            {
                var tta = ppt * (1f / L);
                time.Add(L, 1f / Math.Min(minTime, tta) / 50f);
            }

            return time;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _controller = __instance;
            _character = __instance.character;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(WishesController), "showWishTooltip")]
        private static IEnumerable<CodeInstruction> WishesController_showWishTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var timeToLevel = typeof(WishesController).GetMethod("timeToLevel", new[] { typeof(int) });
            var concat2strings = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            var concat3strings = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) });

            var cm = new CodeMatcher(instructions)
                .MatchForward(true
                    , new CodeMatch(OpCodes.Call, timeToLevel)
                    , new CodeMatch(OpCodes.Call, concat3strings))
                .Advance(2)
                .Insert(new CodeInstruction(OpCodes.Ldloc_0)
                    , new CodeInstruction(OpCodes.Ldarg_1)
                    , Transpilers.EmitDelegate(TotalTimeRemainingString)
                    , new CodeInstruction(OpCodes.Call, concat2strings)
                    , new CodeInstruction(OpCodes.Stloc_0));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static string TotalTimeRemainingString(int id)
        {
            var times = TotalTimeRemaining(id);
            if (times == null || times.Count == 0)
                return string.Empty;

            var levels = Input.GetKey(KeyCode.LeftAlt) ? "\n" + times.Join(kv => $"{kv.Key}: {NumberOutput.timeOutput(kv.Value)}", "\n") : string.Empty;
            var total = times.Sum(kv => kv.Value);

            return $"\n<b>Time to max Level:</b> {NumberOutput.timeOutput(total)}{levels}";
        }
    }
}
