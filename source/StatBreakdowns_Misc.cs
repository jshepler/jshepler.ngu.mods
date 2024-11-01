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

        [HarmonyPostfix, HarmonyPatch(typeof(StatsDisplay), "displayMisc")]
        private static void StatsDisplay_displayMisc_postfix(StatsDisplay __instance)
        {
            var character = __instance.character;
            (var mayoGenStats, var mayoGenValues) = BuildMayoGenRate(character);

            __instance.statsBreakdown.text += mayoGenStats;
            __instance.statValue.text += mayoGenValues;
        }

        private static void PrependMiscStats(StatsDisplay __instance)
        {
            var character = __instance.character;

            (var boostsTexts, var boostsValues) = BuildBoosts(character);
            (var tmTexts, var tmValues) = BuildTM(character);
            (var bmTexts, var bmValues) = BuildBM(character);

            __instance.statsBreakdown.text = boostsTexts + tmTexts + bmTexts;
            __instance.statValue.text = boostsValues + tmValues + bmValues;
        }

        private static (string, string) BuildBoosts(Character character)
        {
            var display = (double d) => character.display(d);

            var completedBoostsCount = character.inventory.itemList.itemMaxxed.Take(39).Count(b => b);
            var completedBoostsBonus = (completedBoostsCount * .02f) + 1f;
            var bdwCompleteBonus = character.inventory.itemList.badlyDrawnComplete ? 1.2f : 1f;
            var constructionCompleteBonus = character.inventory.itemList.constructionComplete ? 1.2f : 1f;
            var perksBonus = character.adventureController.itopod.totalBoostBonus();
            var quirksBonus = character.beastQuestPerkController.totalBoostBonus();

            var totalBonus = completedBoostsBonus * bdwCompleteBonus * constructionCompleteBonus * perksBonus * quirksBonus;

            var statsText =
                $"\n<b>Base Boost Modifier:</b> "
                + $"\n<b>Completed Boosts ({completedBoostsCount}):</b> "
                + (bdwCompleteBonus == 1f ? string.Empty : $"\n<b>Completed BDW set:</b> ")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\n<b>Completed Construction Set:</b> ")
                + (perksBonus == 1f ? string.Empty : $"\n<b>Perks Modifier:</b> ")
                + (quirksBonus == 1f ? string.Empty : $"\n<b>Quirks Modifier:</b> ")
                + $"\n<b>Total Boost Modifier:</b> ";

            var statsValues =
                $"\n  100%"
                + $"\nx {completedBoostsBonus * 100f}%"
                + (bdwCompleteBonus == 1f ? string.Empty : $"\nx {display(bdwCompleteBonus * 100f)}%")
                + (constructionCompleteBonus == 1f ? string.Empty : $"\nx {display(constructionCompleteBonus * 100f)}%")
                + (perksBonus == 1f ? string.Empty : $"\nx {display(perksBonus * 100f)}%")
                + (quirksBonus == 1f ? string.Empty : $"\nx {display(quirksBonus * 100f)}%")
                + $"\n  {totalBonus * 100f:#,##0.##}%";

            return (statsText, statsValues);
        }

        private static (string, string) BuildTM(Character character)
        {
            if (character.bossID < 30)
                return (string.Empty, string.Empty);

            var display = (double d) => character.display(d);

            var totalEnergyPower = character.totalEnergyPower();
            var totalMagicPower = character.totalMagicPower();
            var challMulti = character.allChallenges.timeMachineChallenge.TMSpeedBonus();
            var hacksMulti = character.hacksController.totalTMSpeedBonus();
            var cardMulti = character.cardsController.getBonus(cardBonus.TMSpeed);

            var eTexts = "\n\n<b>Base Energy Time Machine Speed:</b> ";
            var mTexts = "\n\n<b>Base Magic Time Machine Speed:</b> ";
            var eValues = "\n\n  100%";
            var mValues = "\n\n  100%";

            eTexts += "\n<b>Energy Power Modifier:</b> ";
            mTexts += "\n<b>Magic Power Modifier:</b> ";
            eValues += $"\nx {display(totalEnergyPower * 100f)}%";
            mValues += $"\nx {display(totalMagicPower * 100f)}%";

            if (challMulti > 1f)
            {
                eTexts += "\n<b>Evil No TM Challenge:</b> ";
                mTexts += "\n<b>Evil No TM Challenge:</b> ";
                eValues += $"\nx {display(challMulti * 100f)}%";
                mValues += $"\nx {display(challMulti * 100f)}%";
            }

            if (hacksMulti > 1f)
            {
                eTexts += "\n<b>TM Speed Hack Modifier:</b> ";
                mTexts += "\n<b>TM Speed Hack Modifier:</b> ";
                eValues += $"\nx {display(hacksMulti * 100f)}%";
                mValues += $"\nx {display(hacksMulti * 100f)}%";
            }

            if (cardMulti > 1f)
            {
                eTexts += "\n<b>TM Speed Card Modifier:</b> ";
                mTexts += "\n<b>TM Speed Card Modifier:</b> ";
                eValues += $"\nx {display(cardMulti * 100f)}%";
                mValues += $"\nx {display(cardMulti * 100f)}%";
            }

            var totalMulti = challMulti * hacksMulti * cardMulti;
            eTexts += "\n<b>Total Energy Time Machine Speed:</b> ";
            mTexts += "\n<b>Total Magic Time Machine Speed:</b> ";
            eValues += $"\n  {display(totalMulti * totalEnergyPower * 100f)}%";
            mValues += $"\n  {display(totalMulti * totalMagicPower * 100f)}%";

            return (eTexts + mTexts, eValues + mValues);
        }

        private static (string, string) BuildBM(Character character)
        {
            if (character.bossID < 37)
                return (string.Empty, string.Empty);

            var display = (double d) => character.display(d);

            var diggerBloodGainMulti = character.allDiggers.totalBloodBonus();
            var guffBloodGainMulti = character.inventory.macguffinBonuses[18];
            var quirkBloodGainMulti = character.beastQuestPerkController.quirkEffect(91);
            var hacksBloodGainMulti = character.hacksController.totalBloodGainBonus();
            var totalBloogGainMulti = diggerBloodGainMulti * quirkBloodGainMulti * guffBloodGainMulti * hacksBloodGainMulti;

            if (totalBloogGainMulti == 1f)
                return (string.Empty, string.Empty);

            var statsText = "\n\n<b>Base Blood Gain Modifier:</b> ";
            var statsValues = "\n\n  100%";

            if (diggerBloodGainMulti > 1f)
            {
                statsText += "\n<b>Blood Digger:</b> ";
                statsValues += $"\nx {display(diggerBloodGainMulti * 100f)}%";
            }

            if (guffBloodGainMulti > 1f)
            {
                statsText += "\n<b>Blood MacGuffin:</b> ";
                statsValues += $"\nx {display(guffBloodGainMulti * 100f)}%";
            }

            if (quirkBloodGainMulti > 1f)
            {
                statsText += "\n<b>Better Blood Magic (quirk):</b> ";
                statsValues += $"\nx {display(quirkBloodGainMulti * 100f)}%";
            }

            if (hacksBloodGainMulti > 1f)
            {
                statsText += "\n<b>Blood Gain Hack:</b> ";
                statsValues += $"\nx {display(hacksBloodGainMulti * 100f)}%";
            }

            statsText += "\n<b>Total Blood Gain Modifier:</b> ";
            statsValues += $"\n  {display(totalBloogGainMulti * 100f)}%";

            return (statsText, statsValues);
        }

        private static (string, string) BuildMayoGenRate(Character character)
        {
            var display = (double d) => character.display(d, 0, 2);

            var gensUnlocked = character.cardsController.maxManaGenSize();
            var genBonus = 1f + (float)(gensUnlocked - 1) * 0.02f;

            var rhComplete = character.inventory.itemList.rainbowHeartComplete ? 1.1f : 1f;
            var duckComplete = character.inventory.itemList.duckComplete ? 1.06f : 1f;
            var perks = character.adventureController.itopod.totalMayoSpeed();
            var quirks = character.beastQuestPerkController.totalMayoSpeed();
            var wishes = character.wishesController.totalMayoSpeed();
            var sadTC6 = character.allChallenges.trollChallenge.sadisticCompletions() >= 6 ? 1.1f : 1f;
            var potion = character.arbitrary.mayoSpeedPotTime.totalseconds > 0.0 ? character.allArbitrary.potionModifier() : 1f;

            var stats = "\n\n<b>Base Mayo Generation Rate:</b> "
                + $"\n  <b># Generators ({gensUnlocked}):</b> ";

            var values = "\n\n  100% (1:00:00)"
                + $"\nx {genBonus * 100f}%";

            if (rhComplete > 1f)
            {
                stats += "\n<b>Rainbow Heart Set:</b> ";
                values += $"\nx {display(rhComplete * 100f)}%";
            }

            if (duckComplete > 1f)
            {
                stats += "\n<b>Duck Set:</b> ";
                values += $"\nx {display(duckComplete * 100f)}%";
            }

            if (perks > 1f)
            {
                stats += "\n<b>Perks Modifier:</b> ";
                values += $"\nx {display(perks * 100f)}%";
            }

            if (quirks > 1f)
            {
                stats += "\n<b>Quirks Modifier:</b> ";
                values += $"\nx {display(quirks * 100f)}%";
            }

            if (wishes > 1f)
            {
                stats += "\n<b>Wishes Modifier:</b> ";
                values += $"\nx {display(wishes * 100f)}%";
            }

            if (sadTC6 > 1f)
            {
                stats += "\n<b>Sad TC6:</b> ";
                values += $"\nx {display(sadTC6 * 100f)}%";
            }

            if (potion > 1f)
            {
                stats += "\n<b>Infuser:</b> ";
                values += $"\nx {display(potion * 100f)}%";
            }

            var total = character.cardsController.totalMayoSpeed();
            var ppt = 5.555555E-06f * total;
            var seconds = 1f / ppt / 50f;

            stats += "\n<b>Total Mayo Generation Rate:</b> ";
            values += $"\n  {display(total * 100f)}% ({NumberOutput.timeOutput(seconds)})";

            return (stats, values);
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