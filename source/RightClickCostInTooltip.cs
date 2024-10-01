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
    internal class RightClickCostInTooltip
    {
        private const int FIB_PERK_ID = 94;
        private static int[] _fibPerkBonusLevels = new[] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597 };
        private static ItopodPerkController _controller;

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "Start")]
        private static void ItopodPerkController_Start_postfix(ItopodPerkController __instance)
        {
            _controller = __instance;
        }

        // shift-right-click The Fibonacci Perk to only buy up to next unlock
        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "doLevelAll")]
        private static bool ItopodPerkController_doLevelAll_prefix(int id, ItopodPerkController __instance)
        {
            if (id != FIB_PERK_ID || !Input.GetKey(KeyCode.LeftShift))
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

        // use transpiler to insert our text after the cost, but before the fibonacci unlocks
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
            var currentLevel = _controller.character.adventure.itopod.perkLevel[perkId];
            var maxLevel = _controller.maxLevel[perkId];
            if (currentLevel >= maxLevel)
                return string.Empty;

            var pp = _controller.character.adventure.itopod.perkPoints;
            var costPerLevel = _controller.cost[perkId];
            var maxLevelsCanBuy = pp / costPerLevel;
            var text = string.Empty;

            if (pp < costPerLevel)
                text = $"\n\nPP to next level: <b>{display(costPerLevel - pp)}</b>";

            else
            {
                var buyLevels = Math.Min(maxLevelsCanBuy, maxLevel - currentLevel);
                var buyCost = buyLevels * costPerLevel;
                var newLevel = currentLevel + buyLevels;

                text = $"\n\n({display(pp)} / {display(costPerLevel)} = {maxLevelsCanBuy} level{(maxLevelsCanBuy > 1 ? "s" : string.Empty)})"
                    + $"\nRight-Click:"
                    + $"\n   PP: <b>{display(buyCost)}</b>"
                    + $"\n   Levels: <b>+{buyLevels}</b>"
                    + $"\n   New Level: <b>{(newLevel == maxLevel ? "<color=green>MAX</color>" : newLevel)}</b>";
            }

            if (perkId == FIB_PERK_ID)
            {
                var nextBonusLevel = _fibPerkBonusLevels.First(i => i > currentLevel);
                var levelsNeeded = nextBonusLevel - currentLevel;
                var ppNeeded = levelsNeeded * costPerLevel;

                if (ppNeeded > pp)
                    text += $"\n\nPP to next bonus: <b>{display(ppNeeded - pp)}</b>";

                else
                {
                    text += "\n\nShift-Right-Click to buy up to next bonus:"
                        + $"\n   PP: <b>{display(ppNeeded)}</b>"
                        + $"\n   Levels: <b>+{levelsNeeded}</b>";
                }
            }

            return text;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "showTooltip")]
        private static void BeastQuestPerkController_showTooltip_postfix(int id, BeastQuestPerkController __instance, ref string ___message)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Quirks) || id < 0 || id > character.beastQuest.quirkLevel.Count)
                return;

            var display = (double d) => Plugin.Character.display(d);
            var currentLevel = character.beastQuest.quirkLevel[id];
            var maxLevel = __instance.maxLevel[id];
            if (currentLevel >= maxLevel)
                return;

            var qp = character.beastQuest.quirkPoints;
            var costPerLevel = __instance.cost[id];
            var maxLevelsCanBuy = qp / costPerLevel;

            if (qp < costPerLevel)
                ___message += $"\n\nQP to next level: <b>{display(costPerLevel - qp)}</b>";

            else
            {
                var buyLevels = Math.Min(maxLevelsCanBuy, maxLevel - currentLevel);
                var buyCost = buyLevels * costPerLevel;
                var newLevel = currentLevel + buyLevels;

                ___message += $"\n\n({display(qp)} / {display(costPerLevel)} = {maxLevelsCanBuy} level{(maxLevelsCanBuy > 1 ? "s" : string.Empty)})"
                    + $"\nRight-Click:"
                    + $"\n   QP: <b>{display(buyCost)}</b>"
                    + $"\n   Levels: <b>+{buyLevels}</b>"
                    + $"\n   New Level: <b>{(newLevel == maxLevel ? "<color=green>MAX</color>" : newLevel)}</b>";
            }

            __instance.tooltip.showTooltip(___message);
        }
    }
}
