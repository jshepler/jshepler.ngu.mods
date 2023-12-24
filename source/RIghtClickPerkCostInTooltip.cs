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
    internal class RightClickPerkCostInTooltip
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

            var pp = character.adventure.itopod.perkPoints;
            var cost = __instance.cost[FIB_PERK_ID];
            if (pp < cost)
                return true;

            var nextBonus = _fibPerkBonusLevels.First(i => i > perkLevel);
            var buyLevels = nextBonus - perkLevel;

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
                , Transpilers.EmitDelegate(AddRightClickCost)
                , new CodeInstruction(OpCodes.Call, concat2strings)
                , new CodeInstruction(OpCodes.Stfld, messageField));

            return cm.InstructionEnumeration();
        }

        private static string AddRightClickCost(int perkId)
        {
            var perkLevel = _controller.character.adventure.itopod.perkLevel[perkId];
            var maxLevel = _controller.maxLevel[perkId];
            if (perkLevel >= maxLevel || (maxLevel - perkLevel) < 2)
                return string.Empty;

            var pp = _controller.character.adventure.itopod.perkPoints;
            var cost = _controller.cost[perkId];
            if (pp < cost * 2)
                return string.Empty;

            var maxLevelsCanBuy = pp / cost;
            var buyLevels = Math.Min(maxLevelsCanBuy, maxLevel - perkLevel);
            var buyCost = buyLevels * cost;

            var text = $"\n\nRight-Click: <b>{buyCost} PP, +{buyLevels} Level{(buyLevels > 1 ? "s" : "")} = Level {perkLevel + buyLevels}</b>";

            if (perkId == FIB_PERK_ID)
            {
                var nextBonus = _fibPerkBonusLevels.First(i => i > perkLevel);
                var levelsToNextUnlock = nextBonus - perkLevel;
                maxLevelsCanBuy = pp / cost;
                buyLevels = Math.Min(maxLevelsCanBuy, levelsToNextUnlock);
                buyCost = buyLevels * cost;

                text += $"\n\nShift-Right-Click (stops at next bonus):\n  <b>{buyCost} PP, +{buyLevels} Level{(buyLevels > 1 ? "s" : "")} = Level {perkLevel + buyLevels}</b>";
            }

            return text;
        }
    }
}
