using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StatBreakdowns_Misc
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayMisc")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayMisc_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var statsBreakdownField = typeof(StatsDisplay).GetField("statsBreakdown");
            var statValueField = typeof(StatsDisplay).GetField("statValue");
            var setTextMethod = typeof(Text).GetProperty("text").GetSetMethod();

            var matcher = new CodeMatcher(instructions);
            var newInstructions = matcher
                .MatchForward(false // false = leave cursor at start of matches, true = move cursor to end of matches
                    // statsBreakdown.text = ""
                    , new CodeMatch(OpCodes.Ldarg_0)
                    , new CodeMatch(OpCodes.Ldfld, statsBreakdownField)
                    , new CodeMatch(OpCodes.Ldstr, string.Empty)
                    , new CodeMatch(OpCodes.Callvirt, setTextMethod)
                    // statValue.text = ""
                    , new CodeMatch(OpCodes.Ldarg_0)
                    , new CodeMatch(OpCodes.Ldfld, statValueField)
                    , new CodeMatch(OpCodes.Ldstr, string.Empty)
                    , new CodeMatch(OpCodes.Callvirt, setTextMethod))
                .Advance(1) // leave the first Ldarg_0 to pass as argument to delegate below
                .RemoveInstructions(7)
                .Insert(Transpilers.EmitDelegate(PrependMiscStats))
                .InstructionEnumeration();

            return newInstructions;
        }

        private static void PrependMiscStats(StatsDisplay __instance)
        {
            var character = __instance.character;
            var completedBoostsCount = character.inventory.itemList.itemMaxxed.Take(39).Count(b => b);
            var completedBoostsBonus = (completedBoostsCount * .02f) + 1f;
            var bdwCompleteBonus = character.inventory.itemList.badlyDrawnComplete ? 1.2f : 1f;
            var constructionCompleteBonus = character.inventory.itemList.constructionComplete ? 1.2f : 1f;
            var perksBonus = character.adventureController.itopod.totalBoostBonus();
            var quirksBonus = character.beastQuestPerkController.totalBoostBonus();

            var totalBonus = completedBoostsBonus * bdwCompleteBonus * constructionCompleteBonus * perksBonus * quirksBonus;

            __instance.statsBreakdown.text =
                $"\n<b>Base Boost Modifier:</b> "
                + $"\n<b>Completed Boosts ({completedBoostsCount}):</b> "
                + (bdwCompleteBonus == 1f ? string.Empty : $"\n<b>Completed BDW set:</b> ")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\n<b>Completed Construction Set:</b> ")
                + (perksBonus == 1f ? string.Empty : $"\n<b>Perks Modifier:</b> ")
                + (quirksBonus == 1f ? string.Empty : $"\n<b>Quirks Modifier:</b> ")
                + $"\n<b>Total Boost Modifier:</b> ";

            __instance.statValue.text =
                $"\n  100%"
                + $"\nx {completedBoostsBonus * 100f}%"
                + (bdwCompleteBonus == 1f ? string.Empty : $"\nx {bdwCompleteBonus * 100f}%")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\nx {constructionCompleteBonus * 100f}%")
                + (perksBonus == 1f ? string.Empty : $"\nx {perksBonus * 100f:#,##0.##}%")
                + (quirksBonus == 1f ? string.Empty : $"\nx {quirksBonus * 100f:#,##0.##}%")
                + $"\n  {totalBonus * 100f:#,##0.##}%";

            if (character.bossID < 37)
                return;

            var diggerBloodGainMulti = character.allDiggers.totalBloodBonus();
            var guffBloodGainMulti = character.inventory.macguffinBonuses[18];
            var quirkBloodGainMulti = character.beastQuestPerkController.quirkEffect(91);
            var hacksBloodGainMulti = character.hacksController.totalBloodGainBonus();
            var totalBloogGainMulti = diggerBloodGainMulti * quirkBloodGainMulti * guffBloodGainMulti * hacksBloodGainMulti;

            if (totalBloogGainMulti == 1f)
                return;

            var statText = "\n\n<b>Base Blood Gain Modifier:</b> ";
            var valueText = "\n\n  100%";

            if (diggerBloodGainMulti > 1f)
            {
                statText += "\n<b>Blood Digger:</b> ";
                valueText += $"\nx {character.display(diggerBloodGainMulti * 100.0, 0, 2)}%";
            }

            if (guffBloodGainMulti > 1f)
            {
                statText += "\n<b>Blood MacGuffin:</b> ";
                valueText += $"\nx {character.display(guffBloodGainMulti * 100.0, 0, 2)}%";
            }

            if (quirkBloodGainMulti > 1f)
            {
                statText += "\n<b>Better Blood Magic (quirk):</b> ";
                valueText += $"\nx {character.display(quirkBloodGainMulti * 100.0, 0, 2)}%";
            }

            if (hacksBloodGainMulti > 1f)
            {
                statText += "\n<b>Blood Gain Hack:</b> ";
                valueText += $"\nx {character.display(hacksBloodGainMulti * 100.0, 0, 2)}%";
            }

            statText += "\n<b>Total Blood Gain Modifier:</b> ";
            valueText += $"\n  {character.display(totalBloogGainMulti * 100.0, 0, 2)}%";

            __instance.statsBreakdown.text += statText;
            __instance.statValue.text += valueText;
        }
    }
}

/*

replace:
	statsBreakdown.text = "";
	statValue.text = "";

with the call to BoostBonusStatsText() above


	// scrollbar.value = 1f;
	IL_0000: ldarg.0
	IL_0001: ldfld class [UnityEngine.UI]UnityEngine.UI.Scrollbar StatsDisplay::scrollbar
	IL_0006: ldc.r4 1
	IL_000b: callvirt instance void [UnityEngine.UI]UnityEngine.UI.Scrollbar::set_value(float32)
	// statTitle.text = "Misc Breakdowns";
	IL_0010: ldarg.0
	IL_0011: ldfld class [UnityEngine.UI]UnityEngine.UI.Text StatsDisplay::statTitle
	IL_0016: ldstr "Misc Breakdowns"
	IL_001b: callvirt instance void [UnityEngine.UI]UnityEngine.UI.Text::set_text(string)

delete:
	// statsBreakdown.text = "";
	IL_0020: ldarg.0
	IL_0021: ldfld class [UnityEngine.UI]UnityEngine.UI.Text StatsDisplay::statsBreakdown
	IL_0026: ldstr ""
	IL_002b: callvirt instance void [UnityEngine.UI]UnityEngine.UI.Text::set_text(string)
    // statValue.text = "";
	IL_0030: ldarg.0
	IL_0031: ldfld class [UnityEngine.UI]UnityEngine.UI.Text StatsDisplay::statValue
	IL_0036: ldstr ""
	IL_003b: callvirt instance void [UnityEngine.UI]UnityEngine.UI.Text::set_text(string)

insert:
    ldarg.0
    call delegate

	// if (character.purchases.hasDaycare)
	IL_0040: ldarg.0
	IL_0041: ldfld class Character StatsDisplay::character
	IL_0046: ldfld class Purchases Character::purchases
	IL_004b: ldfld bool Purchases::hasDaycare
	IL_0050: brfalse IL_0488

    ...
 */