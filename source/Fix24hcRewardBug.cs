using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Fix24hcRewardBug
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(BossController), "rewardExp")]
        private static IEnumerable<CodeInstruction> BossController_rewardExp_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var completions = typeof(Hour24ChallengeController).GetMethod("completions");
            var expFactor = typeof(AllChallengesController).GetMethod("expFactor");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, completions))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, completions))
                .Advance(-5)
                .RemoveInstructions(1)
                .Advance(3)
                .RemoveInstructions(6)
                .Insert(new CodeInstruction(OpCodes.Callvirt, expFactor));

            return cm.InstructionEnumeration();
        }
    }
}

/*

the 24hr challenges are supposed to give multipliers to fight boss for bosses >= 24, but in 1.260, only
normal 24hr challenge is applied and only 2% per completion instead of 10%

the code for titan exp correctly applies 10%/4%/2% per n/e/s 24hr challenge completions and this bugfix applies the
same to fight boss as the in-game descriptions says

changes:
        num = (Mathf.Max(((float)character.bossID - 13f) / 10f, 1f) + (float)num2) * (1f + (float)character.allChallenges.hour24Challenge.completions() * 0.02f);
to:
        num = (Mathf.Max(((float)character.bossID - 13f) / 10f, 1f) + (float)num2) * character.allChallenges.expFactor()

 */
