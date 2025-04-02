using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedPerkTooltip
    {
        private const int FIB_PERK_ID = 94;
        private static ItopodPerkController _perkController;
        private static int[] _fibPerkBonusLevels = [1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597];
        private static List<int> _noPerkBonusStrings;
        private static bool _wasShiftClicked = false;

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "Start")]
        private static void ItopodPerkController_Start_postfix(ItopodPerkController __instance)
        {
            _perkController = __instance;
        }

        // shift-right-click The Fibonacci Perk to only buy up to next bonus

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkUIController), "levelAllEvent")]
        private static void ItopodPerkUIController_levelAllEvent_prefix()
        {
            _wasShiftClicked = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "doLevelAll")]
        private static bool ItopodPerkController_doLevelAll_prefix(int id, ItopodPerkController __instance)
        {
            if (id != FIB_PERK_ID || !_wasShiftClicked)
                return true;

            var character = __instance.character;
            var perkLevel = character.adventure.itopod.perkLevel[FIB_PERK_ID];
            if (perkLevel >= __instance.maxLevel[FIB_PERK_ID])
                return true;

            var nextBonus = _fibPerkBonusLevels.First(i => i > perkLevel);
            var buyLevels = nextBonus - perkLevel;
            var pp = character.adventure.itopod.perkPoints;
            var cost = __instance.cost[FIB_PERK_ID];
            if (pp < cost * buyLevels)
                return true;

            for (var x = 0; x < buyLevels; x++)
            {
                character.adventure.itopod.perkPoints -= cost;
                character.adventure.itopod.perkLevel[FIB_PERK_ID]++;
                __instance.doEffect(FIB_PERK_ID);
            }

            __instance.showTooltip(id);
            __instance.updateText();
            __instance.changePage(__instance.page);

            return false;
        }

        // use transpiler to insert our text after the cost, but before the fibonacci bonuses
        [HarmonyTranspiler, HarmonyPatch(typeof(ItopodPerkController), "showTooltip", typeof(int))]
        private static IEnumerable<CodeInstruction> ItopodPerkController_showTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var messageField = typeof(ItopodPerkController).GetField("message", BindingFlags.NonPublic | BindingFlags.Instance); //Traverse.Create<ItopodPerkController>().Field("message");
            var concat2strings = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }); //Traverse.Create<string>().Method("Concat", "", "");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)94));

            // we're inserting code before an instruction that's the target of an earlier branch,
            // so we need to move that label to the first instruction we're inserting so that branch doens't skip our code
            var oldBranchTarget = cm.Advance(-1).Instruction;
            var newBranchTarget = new CodeInstruction(OpCodes.Ldarg_0);
            newBranchTarget.MoveLabelsFrom(oldBranchTarget);

            cm.Insert(
                newBranchTarget
                , new CodeInstruction(OpCodes.Ldarg_0)
                , new CodeInstruction(OpCodes.Ldfld, messageField)
                , new CodeInstruction(OpCodes.Ldarg_1)
                , Transpilers.EmitDelegate(AddRightClickPPCost)
                , new CodeInstruction(OpCodes.Call, concat2strings)
                , new CodeInstruction(OpCodes.Stfld, messageField));

            return cm.InstructionEnumeration();
        }

        private static string AddRightClickPPCost(int perkId)
        {
            var display = (double d) => Plugin.Character.display(d);
            var currentLevel = _perkController.character.adventure.itopod.perkLevel[perkId];
            var maxLevel = _perkController.maxLevel[perkId];
            if (currentLevel >= maxLevel)
                return string.Empty;

            var pp = _perkController.character.adventure.itopod.perkPoints;
            var costPerLevel = _perkController.cost[perkId];
            var maxLevelsCanBuy = pp / costPerLevel;
            var buyLevels = (int)Math.Min(maxLevelsCanBuy, maxLevel - currentLevel);

            var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
            var ppGainedLastRB = TrackPPGained.PPGainedLastRB;
            var gainedPerSec = secondsLastRB == 0.0 ? 0.0 : ppGainedLastRB / secondsLastRB;

            var text = string.Empty;
            var ppToNextLevel = costPerLevel - pp;
            if (ppToNextLevel > 0)
            {
                text = $"\n\n<b>PP to next level: {display(ppToNextLevel)}</b>";

                if (gainedPerSec > 0)
                    text += $"\n ... est. days: {display(Math.Floor(ppToNextLevel / gainedPerSec / 86400.0))}";
            }

            else //if (buyLevels > 1)
            {
                var buyCost = buyLevels * costPerLevel;
                var newLevel = currentLevel + buyLevels;

                text = $"\n\n<b>Right-Click:</b> <size=10>(max levels: {maxLevelsCanBuy})</size>"
                    + $"\n   <b>New Level: {(newLevel == maxLevel ? "<color=green>MAX</color>" : $"{newLevel}</b> <size=10>(+{buyLevels})</size><b>")}"
                    + getNewPerkLevelBonus(perkId, buyLevels)
                    + $"\n   COST: {display(buyCost)} Perk Point{(buyCost > 1 ? "s" : string.Empty)}</b>";
            }

            var levelsToMax = maxLevel - currentLevel;
            var ppToMax = costPerLevel * levelsToMax - pp;
            if (levelsToMax > 1 && ppToMax > 0)
            {
                text += $"\n\n<b>PP to max level: {display(ppToMax)}</b>";

                if (gainedPerSec > 0)
                    text += $"\n ... est. days: {display(Math.Floor(ppToMax / gainedPerSec / 86400.0))}";
            }

            if (perkId == FIB_PERK_ID)
            {
                var nextBonusLevel = _fibPerkBonusLevels.First(i => i > currentLevel);
                var levelsNeeded = nextBonusLevel - currentLevel;
                var ppNeeded = levelsNeeded * costPerLevel;
                var ppToNextBonus = ppNeeded - pp;

                if (ppToNextBonus > 0)
                {
                    text += $"\n\n<b>PP to next bonus: {display(ppToNextBonus)}</b>";

                    if (gainedPerSec > 0)
                        text += $"\n ... est. days: {display(Math.Floor(ppToNextBonus / gainedPerSec / 86400.0))}";
                }

                else
                {
                    text += "\n\n<b>Shift-Right-Click:</b> <size=10>(buy up to next bonus)</size>"
                        + $"\n   <b>New Level: {nextBonusLevel}"
                        + $"\n   COST: {display(ppNeeded)} Perk Points</b>";
                }
            }

            return text;
        }

        private static string getNewPerkLevelBonus(int perkId, int offset)
        {
            if (_noPerkBonusStrings == null)
            {
                _noPerkBonusStrings = Enumerable.Range(36, 14).Select(i => i).ToList();
                _noPerkBonusStrings.AddRange([18, 21, 22, 25, 51, 54, 55]);
            }

            if (!_perkController.hasStatEffect[perkId]
                || _noPerkBonusStrings.Contains(perkId)
                || _perkController.character.adventure.itopod.perkLevel[perkId] >= _perkController.capLevel(perkId))
                return string.Empty;

            return perkId switch
            {
                84 => $"\n   New Level Bonus: {_perkController.statEffect(perkId, offset)}x",
                85 => $"\n   New Level Bonus: {_perkController.statEffect(perkId, offset)}x",
                93 => $"\n   New Level Bonus: {_perkController.invertedEffectPercent(perkId, offset)}%",
                _ => $"\n   New Level Bonus: {_perkController.percentEffect(perkId, offset)}%"
            };
        }
    }
}
