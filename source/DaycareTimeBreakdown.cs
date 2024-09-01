using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareTimeBreakdown
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayMisc")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayMisc_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var setText = typeof(Text).GetProperty("text").SetMethod;
            var statsBreakdown = typeof(StatsDisplay).GetField("statsBreakdown");
            var statValue = typeof(StatsDisplay).GetField("statValue");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n\n<b>Base Kitty Happiness</b> "))
                .SetOperandAndAdvance("\n\n<b>Base Kitty Happiness (speed):</b> ")

                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n<b>Total Kitty Happiness:</b> "))
                .SetOperandAndAdvance("\n<b>Total Kitty Happiness (speed):</b> ")

                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, setText), new CodeMatch(OpCodes.Ldarg_0))
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, setText))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, statsBreakdown)
                    , new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, statValue)
                    , Transpilers.EmitDelegate(InsertDaycareTimeBreakdown));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static void InsertDaycareTimeBreakdown(Text statsBreakdown, Text statValue)
        {
            var character = Plugin.Character;
            var statText = "\n\n<b>Base Kitty Happiness (time):</b> ";
            var valueText = "\n\n  100%";
            var totalModifier = 1f;

            var blindCompletions = character.allChallenges.blindChallenge.completions();
            if (blindCompletions > 0)
            {
                var blindModifier = 1f - 0.05f - blindCompletions * 0.01f;
                totalModifier *= blindModifier;

                statText += "\n<b>Normal Blind Challenge:</b> ";
                valueText += $"\nx {blindModifier * 100f}%";
            }

            var perk27 = character.adventure.itopod.perkLevel[27];
            var perk28 = character.adventure.itopod.perkLevel[28];
            if (perk27 > 0 || perk28 > 0)
            {
                var perkModifier = 1f - perk27 * character.adventureController.itopod.effectPerLevel[27];
                perkModifier *= 1f - perk28 * character.adventureController.itopod.effectPerLevel[28];
                totalModifier *= perkModifier;

                statText += "\n<b>Perks Modifier:</b> ";
                valueText += $"\nx {perkModifier * 100f}%";
            }

            if (character.arbitrary.hasDaycareSpeed)
            {
                totalModifier *= 0.9f;
                statText += "\n<b>AP Purchase:</b> ";
                valueText += "\nx 90%";
            }

            statText += "\n<b>Total Kitty Happiness (time):</b> ";
            valueText += $"\n  {totalModifier * 100f}%";

            statsBreakdown.text += statText;
            statValue.text += valueText;
        }
    }
}
