using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishTooltip
    {
        private static Character _character;
        private static WishesController _controller;

        private const float BIAS = 0.17f;
        private static float SpeedDivider(int id) => _controller.properties[id].wishSpeedDivider;
        private static float SpeedBonus() => _controller.totalWishSpeedBonuses();
        private static float MinTime() => _controller.minimumWishTime();

        private static WishWrapper _wish(int id) => Wishes.AllWishes[id];

        private static float EnergyFactor(int id) => Mathf.Pow(_character.totalEnergyPower() * _wish(id).Energy, BIAS);
        private static float EnergyFactorMax() => Mathf.Pow(_character.totalEnergyPower() * _character.totalCapEnergy(), BIAS);
        private static float MagicFactor(int id) => Mathf.Pow(_character.totalMagicPower() * _wish(id).Magic, BIAS);
        private static float MagicFactorMax() => Mathf.Pow(_character.totalMagicPower() * _character.totalCapMagic(), BIAS);
        private static float Res3Factor(int id) => Mathf.Pow(_character.totalRes3Power() * _wish(id).Res3, BIAS);
        private static float Res3FactorMax() => Mathf.Pow(_character.totalRes3Power() * _character.totalCapRes3(), BIAS);

        private static float TimeToLevel(int id) => (1f - _wish(id).Progress) / _controller.progressPerTick(id) / 50f;
        private static float TimeToLevelMax(int id) => (1f - _wish(id).Progress) / _controller.progressPerTickMax(id) / 50f;

        private static bool IsRunning(int id) => _wish(id).IsRunning;
        private static string Display(double d) => _controller.character.display(d, 2);

        private static Dictionary<int, float> TotalTimeRemaining(int id)
        {
            var wish = _wish(id);
            var minTime = _controller.minimumWishTime();
            var ppt = IsRunning(id)
                ? EnergyFactor(id) * MagicFactor(id) * Res3Factor(id) * SpeedBonus() / SpeedDivider(id)
                : EnergyFactorMax() * MagicFactorMax() * Res3FactorMax() * SpeedBonus() / SpeedDivider(id);

            var time = new Dictionary<int, float>();
            if (wish.Level == wish.MaxLevel)
                return time;

            time.Add(wish.Level + 1, wish.IsRunning ? TimeToLevel(id) : TimeToLevelMax(id));
            for (var L = wish.Level + 2; L < wish.MaxLevel + 1; L++)
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
            var timeToLevel = typeof(WishesController).GetMethod("timeToLevel", [typeof(int)]);
            var concat2strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]);
            var concat3strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string)]);

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

            var total = times.Sum(kv => kv.Value);
            var text = $"\n<b>Time to max Level:</b> {NumberOutput.timeOutput(total)}";

            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altDown)
            {
                var levels = times.Join(kv => $"{kv.Key}: {NumberOutput.timeOutput(kv.Value)}", "\n");
                text += $"\n{levels}";
            }

            //text += "\n\n% of cap (min time) at cur/max resources";

            var curPPT = _controller.progressPerTick(id);
            //var maxPPT = _controller.progressPerTickMax(id);
            var capPPT = _controller.minimumWishTime();
            //var curPctPPT = curPPT / capPPT * 100f;
            //var maxPctPPT = maxPPT / capPPT * 100f;
            //text += $"\n         <b>Speed:</b> {curPctPPT:0.##}% / {maxPctPPT:0.##}%";

            var curEMRF = _controller.energyFactor(id) * _controller.magicFactor(id) * _controller.res3Factor(id);
            var maxEMRF = _controller.energyFactorMax(id) * _controller.magicFactorMax(id) * _controller.res3FactorMax(id);
            var capEMRF = capPPT * SpeedDivider(id) * (_wish(id).Level + 1) / SpeedBonus();
            var curPctEMRF = curEMRF / capEMRF * 100f;
            var maxPctEMRF = maxEMRF / capEMRF * 100f;
            text += $"\n\n<b>EMR3 (cur | max):</b> {curPctEMRF:0.##}% | {maxPctEMRF:0.##}%";

            if (altDown)
            {
                var prog = _wish(id).Progress;
                var min = prog.MinNeededToRoundToNextFloat();

                text += $"\n\n<b>Progress:</b> {prog * 100f:00.00000000000000000}%"
                    + $"\n     per tick: {curPPT * 100f:00.00000000000000000}%"
                    + $"\n           min: {min * 100f:00.00000000000000000}%";

                //if (prog < .5)
                //{
                //    var next = prog.MinFloatOfNextExponent();
                //    text += $"\n        inc at: {next * 100f:r}% progress";
                //}
            }

            return text;
        }

        //private static float getMinPPT(float curProgress)
        //{
        //    var next = curProgress.NextFloat();
        //    var halfDiff = (next - curProgress) / 2f;
        //    var min = halfDiff.NextFloat();

        //    return min;
        //}
    }
}
