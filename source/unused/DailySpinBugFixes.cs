using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DailySpinBugFixes
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(DailyRewardController), "getTier0RewardIndex")]
        private static IEnumerable<CodeInstruction> DailyRewardController_getTier0RewardIndex_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.1f))
                .SetOperandAndAdvance(1f);

            return cm.InstructionEnumeration();//.DumpToLog();
        }
    }
}
