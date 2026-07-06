using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    //[HarmonyPatch]
    internal class FixWandoosItemBug
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(ItemController), "consumeItem")]
        private static IEnumerable<CodeInstruction> ItemController_consumeItem_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)

                // wandoos 98
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_2))
                .Advance(1)
                .RemoveInstructions(31)
                .Advance(2)
                .SetOpcodeAndAdvance(OpCodes.Blt)

                // wandoos XL
                .MatchForward(false, new CodeMatch(i => i.IsStLoc(5)))
                .Advance(1)
                .RemoveInstructions(33)
                .SetOpcodeAndAdvance(OpCodes.Blt);

            return cm.InstructionEnumeration();
        }
    }
}
