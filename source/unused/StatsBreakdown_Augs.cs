using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StatsBreakdown_Augs
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayAugments")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayAugments_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldString = "Augment Speed Breakdown";
            var newString = "Augment Stats Breakdown";

            var statsBreakdown = typeof(StatsDisplay).GetField("statsBreakdown");
            var statValue = typeof(StatsDisplay).GetField("statValue");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, oldString))
                .SetOperandAndAdvance(newString)

                // fixes bug where "Welcome to Sadistic" perk isn't included
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n<b>Total Augment Speed Factor:</b> "))
                .Advance(-1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, statValue),
                    new CodeInstruction(Transpilers.EmitDelegate(addSadPerk)));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static void addSadPerk(Text statText, Text valueText)
        {
            if (Plugin.Character.settings.rebirthDifficulty >= difficulty.sadistic
                && Plugin.Character.adventure.itopod.perkLevel[144] >= 1)
            {
                statText.text += "\n<b>Welcome to Sadistic Perk</b> ";
                valueText.text += "\nx 120%";
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllAugsController), "getTotalSpeedFactor")]
        private static void AllAugsController_getTotalSpeedFactor_postfix(ref float __result)
        {
            if (Plugin.Character.settings.rebirthDifficulty >= difficulty.sadistic
                && Plugin.Character.adventure.itopod.perkLevel[144] >= 1)
            {
                __result *= 1.2f;
            }
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
