using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AugmentStatsBreakdown
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayAugments")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayAugments_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldString = "Augment Speed Breakdown";
            var newString = "Augment Stats Breakdown";

            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, oldString))
                .SetOperandAndAdvance(newString)
                .InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StatsDisplay), "displayAugments")]
        private static void StatsDisplay_displayAugments_postfix(StatsDisplay __instance)
        {
            var character = __instance.character;

            var augsMultSum = 1d + character.augmentsController.augments.Sum(a => a.getTotalStatBoost());
            var noAugsChallengeMult = 1d + (double)character.allChallenges.noAugsChallenge.completions() * 0.25d;
            var nguAugsMult = character.NGUController.augmentBonus();
            var sadDivider = character.settings.rebirthDifficulty == difficulty.sadistic ? character.augmentsController.sadisticNerfModifier() : 1d;
            
            var totalMult = augsMultSum * noAugsChallengeMult * nguAugsMult / sadDivider;

            __instance.statsBreakdown.text +=
                $"\n\n<b>Base Augments Modifier:</b> "
                + (noAugsChallengeMult > 1d ? $"\n<b>No Augs Challenge Modifier:</b> " : string.Empty)
                + (nguAugsMult > 1d ? $"\n<b>NGU Augments Modifier:</b> " : string.Empty)
                + (sadDivider > 1d ? $"\n<b>Sadistic Nerf DIVIDER:</b> " : string.Empty)
                + $"\n<b>Total Attack/Defense Modifier:</b> ";

            __instance.statValue.text +=
                $"\n\n  {character.display(augsMultSum * 100d)}%"
                + (noAugsChallengeMult > 1d ? $"\nx {character.display(noAugsChallengeMult * 100d)}%" : string.Empty)
                + (nguAugsMult > 1d ? $"\nx {character.display(nguAugsMult * 100d)}%" : string.Empty)
                + (sadDivider > 1d ? $"\n/ {character.display(sadDivider * 100d)}%" : string.Empty)
                + $"\n  {character.display(totalMult * 100d)}%";
        }
    }
}
