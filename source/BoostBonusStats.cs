using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BoostBonusStats
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
                .Insert(Transpilers.EmitDelegate(BoostBonusStatsText))
                .InstructionEnumeration();

            return newInstructions;
        }

        private static void BoostBonusStatsText(StatsDisplay __instance)
        {
            var completedBoostsCount = __instance.character.inventory.itemList.itemMaxxed.Take(39).Count(b => b);
            var completedBoostsBonus = (completedBoostsCount * .02f) + 1f;
            var bdwCompleteBonus = __instance.character.inventory.itemList.badlyDrawnComplete ? 1.2f : 1f;
            var constructionCompleteBonus = __instance.character.inventory.itemList.constructionComplete ? 1.2f : 1f;
            var perksBonus = __instance.character.adventureController.itopod.totalBoostBonus();
            var quirksBonus = __instance.character.beastQuestPerkController.totalBoostBonus();

            var totalBonus = completedBoostsBonus * bdwCompleteBonus * constructionCompleteBonus * perksBonus * quirksBonus;

            __instance.statsBreakdown.text =
                $"\n<b>Base Boost Modifier:</b> "
                + $"\n<b>Completed Boosts ({completedBoostsCount}):</b> "
                + (bdwCompleteBonus == 1f ? string.Empty : $"\n<b>Completed BDW set:</b> ")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\n<b>Completed Construction Set:</b> ")
                + (perksBonus == 1f ? string.Empty : $"\n<b>Perks Modifier:</b> ")
                + (quirksBonus == 1f ? string.Empty : $"\n<b>Quirks Modifier:</b> ")
                + $"\n<b>Total Modifier:</b> ";

            __instance.statValue.text =
                $"\n  100%"
                + $"\nx {completedBoostsBonus * 100f}%"
                + (bdwCompleteBonus == 1f ? string.Empty : $"\nx {bdwCompleteBonus * 100f}%")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\nx {constructionCompleteBonus * 100f}%")
                + (perksBonus == 1f ? string.Empty : $"\nx {perksBonus * 100f}%")
                + (quirksBonus == 1f ? string.Empty : $"\nx {quirksBonus * 100f}%")
                + $"\n  {totalBonus * 100f}%";
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