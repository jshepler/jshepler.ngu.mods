using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AdvancedTrainingTitanAK
    {
        private static Character character;
        private static AllAdvancedTraining _advancedTraining;

        private static float totalPowerWithoutAdvPower;
        private static float totalDefWithoutAdvDef;
        private static float totalRegenWithoutAdvDef;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_showTitanTimer_Start(ButtonShower __instance)
        {
            character = __instance.character;
            _advancedTraining = character.advancedTrainingController;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(ButtonShower), "showTitanTimers")]
        private static IEnumerable<CodeInstruction> ButtonShower_showTitanTimers_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 1f))
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_R4, 0.1f));

            return cm.InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showTitanTimer"), HarmonyPriority(2)]
        private static void ButtonShower_showTitanTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            if (!Plugin.GameHasStarted || __instance.adventure.interactable == false || string.IsNullOrWhiteSpace(___message))
                return;

            var text = BuildTooltipText();
            if (text != null)
            {
                ___message += $"\n\n<b>Adv. Training needed to autokill Titans:</b>{text}";
                __instance.tooltip.showTooltip(___message);
            }
        }

        private static string BuildTooltipText()
        {
            totalPowerWithoutAdvPower = character.totalAdvAttack() / (_advancedTraining.adventurePowerBonus(0) + 1f);
            totalDefWithoutAdvDef = character.totalAdvDefense() / (_advancedTraining.adventureToughnessBonus(0) + 1f);
            totalRegenWithoutAdvDef = character.totalAdvHPRegen() / (_advancedTraining.adventureToughnessBonus(0) + 1f);

            var effectiveBossId = character.effectiveBossID();
            var enemies = character.bestiary.enemies;
            var currentAT = new PTR(character.advancedTraining.level[1], character.advancedTraining.level[0]);
            var sb = new StringBuilder();

            foreach (var req in TitanAK.Requirements)
            {
                var enemyKills = enemies[req.enemyId].kills;

                if (effectiveBossId < req.effectiveBossId && enemyKills == 0)
                    continue;

                if (req.optionalKills > 0 && enemyKills >= req.optionalKills)
                    continue;

                var neededAT = GetNeededAT(req.ptr);
                if (neededAT.IsNothing)
                    continue;

                var haveIt = neededAT <= currentAT;
                var color = haveIt ? "green" : "red";
                sb.Append($"\n<color={color}>{req.name}:  T={character.display(neededAT.Toughness)}, P={character.display(neededAT.Power)}</color>");

                if (req.optionalKills > 0 && enemyKills < req.optionalKills && enemyKills > 0)
                {
                    var killsLeft = req.optionalKills - enemyKills;
                    sb.Append($"\n   ({killsLeft} kill{(killsLeft == 1 ? string.Empty : "s")} left for perma AK)");
                }

                // only show the one titan/version that cannot AK as presumably it's the next one to work towards
                if (!haveIt && !Input.GetKey(KeyCode.LeftAlt))
                    break;
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        internal static PTR GetNeededAT(PTR ak)
        {
            var neededAT = new PTR(0, 0);

            if (totalPowerWithoutAdvPower < ak.Power)
            {
                var atPowerPct = ((ak.Power / totalPowerWithoutAdvPower) - 1) * 100f; // -1 to convert form "multiplier" to "bonus"
                neededAT.Power = (float)Math.Ceiling(Math.Pow(atPowerPct / 10, 2.5)); // https://ngu-idle.fandom.com/wiki/Advanced_Training#Formulas
                if (neededAT.Power < 0)
                    neededAT.Power = 0;
            }

            if (totalDefWithoutAdvDef < ak.Toughness)
            {
                var atDefPct = ((ak.Toughness / totalDefWithoutAdvDef) - 1) * 100f; // -1 to convert from "multiplier" to "bonus"
                neededAT.Toughness = (float)Math.Ceiling(Math.Pow(atDefPct / 10, 2.5)); // https://ngu-idle.fandom.com/wiki/Advanced_Training#Formulas
                if (neededAT.Toughness < 0)
                    neededAT.Toughness = 0;
            }

            // regen also gets AT toughness multiplier, calc AT toughness needed for regen
            // and if higher than what's needed for ak.Toughness, use it instead
            if (ak.Regen > 0f && totalRegenWithoutAdvDef < ak.Regen)
            {
                var regenPct = ((ak.Regen / totalRegenWithoutAdvDef) - 1) * 100f;
                var regenNeeded = (float)Math.Ceiling(Math.Pow(regenPct / 10, 2.5));

                if (regenNeeded > neededAT.Toughness)
                    neededAT.Toughness = regenNeeded;
            }

            if (neededAT.Power > long.MaxValue)
                neededAT.Power = float.PositiveInfinity;
            if (neededAT.Toughness > long.MaxValue)
                neededAT.Toughness = float.PositiveInfinity;

            return neededAT;
        }

        // this removes an extra line-feed that's only on the "BEAST SPAWN READY" line, it bothered me
        // game also has a bug that shows T10 for wrong boss, effective boss id should be 777 (sad boss 175) instead of 727 (sad boss 125)
        [HarmonyTranspiler, HarmonyPriority(2), HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        private static IEnumerable<CodeInstruction> ButtonShower_showTitanTimer_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldString = "\n<b>THE BEAST SPAWN READY</b>\n";
            var newString = "\n<b>THE BEAST SPAWN READY</b>";

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, oldString))
                .SetOperandAndAdvance(newString)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 727))
                .SetOperandAndAdvance(777);

            return cm.InstructionEnumeration();//.DumpToLog();
        }
    }
}
