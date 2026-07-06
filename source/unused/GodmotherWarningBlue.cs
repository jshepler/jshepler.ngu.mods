using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class GodmotherWarningBlue
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "godmotherAI")]
        private static IEnumerable<CodeInstruction> EnemyAI_godmotherAI_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, " starts glowing white! HIT THE FREAKIN' DECK!"))
                .Advance(2)
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_3));

            return cm.InstructionEnumeration();
        }
    }
}
