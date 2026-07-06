using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    internal enum ShowFullResourceName { None, All, Res3Only }

    [HarmonyPatch]
    internal class AlwaysShowFullResourceNames
    {
        private static ShowFullResourceName _showFullResourceName = Options.ResourceNames.ShowFullName.Value;

        [HarmonyTranspiler, HarmonyPatch(typeof(Energy), "updateEnergyText")]
        private static IEnumerable<CodeInstruction> Energy_updateEnergyText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (_showFullResourceName != ShowFullResourceName.All)
                return instructions;

            var shortE = "<b>E: </b>";
            var longE = "<b>Energy: </b>";

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, shortE))
                .Repeat(m => m.SetOperandAndAdvance(longE));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(MagicDisplay), "updateMagicText")]
        private static IEnumerable<CodeInstruction> MagicDisplay_updateMagicText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (_showFullResourceName != ShowFullResourceName.All)
                return instructions;

            var shortM = "<b>M:</b> ";
            var longM = "<b>Magic:</b> ";

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, shortM))
                .Repeat(m => m.SetOperandAndAdvance(longM));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(Resource3Display), "updateRes3Text")]
        private static IEnumerable<CodeInstruction> Resource3Display_updateRes3Text_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (_showFullResourceName == ShowFullResourceName.None)
                return instructions;

            var res3Name = typeof(Resource3).GetField("res3Name");

            var cm = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, res3Name),
                    new CodeMatch(OpCodes.Ldc_I4_0))
                .Repeat(m => m.RemoveInstructions(6));

            return cm.InstructionEnumeration();
        }
    }
}
