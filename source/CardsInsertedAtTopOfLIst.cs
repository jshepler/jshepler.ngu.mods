using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardsInsertedAtTopOfLIst
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(CardsController), "addCard")]
        private static IEnumerable<CodeInstruction> CardsController_addCard_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cards = typeof(Cards).GetField("cards");
            var insert = cards.FieldType.GetMethod("Insert");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldloc_0))
                .Insert(new CodeInstruction(OpCodes.Ldc_I4_0))
                .Advance(2)
                .SetOperandAndAdvance(insert);

            return cm.InstructionEnumeration();
        }
    }
}
