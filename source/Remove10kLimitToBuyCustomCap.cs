using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Remove10kLimitToBuyCustomCap
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EnergyPurchases), "updateCustomCapInput")]
        private static IEnumerable<CodeInstruction> EnergyPurchases_updateCustomCapInput_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(new CodeInstruction(OpCodes.Ldc_I4, 10000)))
                .Advance(-5);

            var i = cm.Instruction;
            cm.Advance(-7).SetOperandAndAdvance(i.operand);

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(MagicPurchases), "updateCustomCapInput")]
        private static IEnumerable<CodeInstruction> MagicPurchases_updateCustomCapInput_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(new CodeInstruction(OpCodes.Ldc_I4, 10000)))
                .Advance(-5);

            var i = cm.Instruction;
            cm.Advance(-7).SetOperandAndAdvance(i.operand);

            return cm.InstructionEnumeration();
        }
    }
}
