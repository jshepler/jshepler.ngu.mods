using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [Flags]
    internal enum NotifiedSpells { none = 0, IP = 1, GUFFA = 2, GUFFB = 4 }

    [HarmonyPatch]
    internal class BloodMagic
    {
        // based on AllBloodMagicController.lootBonus() and .goldBonus()
        const double MINBLOOD_LOOT = 10000.0;
        const double MINBLOOD_GOLD = 1000000.0;

        private static double _totalBPS => Plugin.Character.bloodMagicController.bloodMagics.Sum(bm => bm.bloodGainedPerSecond());

        [HarmonyPostfix, HarmonyPatch(typeof(AllBloodMagicController), "updateBloodDisplay")]
        private static void AllBloodMagicController_updateBloodDisplay_postifx(AllBloodMagicController __instance)
        {
            //_totalBPS = __instance.bloodMagics.Sum(bm => bm.bloodGainedPerSecond());
            var fontSize = __instance.bloodText.fontSize * .9;
            
            __instance.bloodText.text += $"\n<size={fontSize}><b>Gain:</b> +{__instance.character.display(_totalBPS)}/s</size>";
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "spellTooltip")]
        private static void RebirthPowerSpell_spellTooltip_postfix(RebirthPowerSpell __instance, ref string ___message)
        {
            if (_totalBPS <= 0)
                return;

            var curBlood = __instance.character.bloodMagic.bloodPoints;
            var curBonus = bloodToAdvStats(curBlood);
            var oneMinBlood = curBlood + _totalBPS * 60f;
            var oneMinBonus = bloodToAdvStats(oneMinBlood);

            ___message += $"\n\n+{Plugin.Character.display(oneMinBonus - curBonus)}/min";
            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "lootSpellTooltip")]
        private static void RebirthPowerSpells_lootSpellTooltip_postfix(RebirthPowerSpell __instance, ref string ___message)
        {
            if (!__instance.IsInvoking("lootSpellTooltip"))
                __instance.InvokeRepeating("lootSpellTooltip", 0, 0.1f);

            var bm = __instance.character.bloodMagic;
            var bloodInvested = bm.lootSpellBlood;

            var curBonus = bloodToLootBonus(bloodInvested);
            var autoCount = getAutoCastCount();
            var totalBlood = bloodInvested + (bm.bloodPoints / Math.Max(1, autoCount));
            var newBonus = bloodToLootBonus(totalBlood);
            ___message += $"\n\n<b>Total bonus if used now:</b> {newBonus:#,##0.#}% (+{(newBonus - curBonus):#,##0.#}%)";

            if (_totalBPS > 0)
            {
                var nextBonus = newBonus + 1;
                var nextTotalBlood = lootBonusToBlood(nextBonus);
                var bloodRemaining = nextTotalBlood - totalBlood;
                var secondsRemaining = bloodRemaining / (_totalBPS / Math.Max(1, autoCount));

                ___message += $"\n   ({nextBonus:#,##0.#}% in {NumberOutput.timeOutput(secondsRemaining)})";
            }

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "goldSpellTooltip")]
        private static void RebirthPowerSpells_goldSpellTooltip_postfix(RebirthPowerSpell __instance, ref string ___message)
        {
            if (!__instance.IsInvoking("goldSpellTooltip"))
                __instance.InvokeRepeating("goldSpellTooltip", 0, 0.1f);

            var bm = __instance.character.bloodMagic;
            var bloodInvested = bm.goldSpellBlood;

            var curBonus = bloodToGoldBonus(bloodInvested);
            var autoCount = getAutoCastCount();
            var totalBlood = bloodInvested + (bm.bloodPoints / Math.Max(1, autoCount));
            var newBonus = bloodToGoldBonus(totalBlood);
            ___message += $"\n\n<b>Total bonus if used now:</b> {newBonus:#,##0.#}% (+{(newBonus - curBonus):#,##0.#}%)";

            if (_totalBPS > 0)
            {
                var nextBonus = newBonus + 1;
                var nextTotalBlood = goldBonusToBlood(nextBonus);
                var bloodRemaining = nextTotalBlood - totalBlood;
                var secondsRemaining = bloodRemaining / (_totalBPS / Math.Max(1, autoCount));

                ___message += $"\n   ({nextBonus:#,##0.#}% in {NumberOutput.timeOutput(secondsRemaining)})";
            }

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "endSpellTooltip")]
        private static void RebirthPowerSpell_endSpellTooltip_postfix(RebirthPowerSpell __instance, ref string ___message)
        {
            if (!__instance.IsInvoking("endSpellTooltip"))
                __instance.InvokeRepeating("endSpellTooltip", 0, 0.1f);

            if (_totalBPS <= 0)
                return;

            var bloodRemaining = 5e+22 - __instance.character.bloodMagic.bloodPoints;
            if (bloodRemaining <= 0)
                return;

            var secondsRemaining = bloodRemaining / _totalBPS;
            ___message += $"\n\n<b>Time Remaining:</b> {NumberOutput.timeOutput(secondsRemaining)}";

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "hideTooltip")]
        private static void RebirthPowerSpells_hideTooltip_postfix(RebirthPowerSpell __instance)
        {
            __instance.CancelInvoke("lootSpellTooltip");
            __instance.CancelInvoke("goldSpellTooltip");
            __instance.CancelInvoke("endSpellTooltip");
        }

        // persist iron pill's last adventure amount gained (info purpose only)
        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "Start")]
        private static void RebirthPowerSpell_Start_postfix(RebirthPowerSpell __instance)
        {
            var field = Traverse.Create(__instance).Field<float>("lastAdventureAmount");

            Plugin.OnPreSave += (o, e) => ModSave.Data.BM_IronPill_LastGained = field.Value;
            Plugin.OnSaveLoaded += (o, e) => field.Value = ModSave.Data.BM_IronPill_LastGained;

            // left clicking number's "Auto Spell" text is suppoed to toggle the checkbox,
            // like it does for DC and gold, but it's missing this flag being set
            GameObject.Find("Canvas/Blood Magic Spells Canvas/Blood Magic Spells Menu /Auto Spell Button/Text")
                .GetComponent<Text>()
                .raycastTarget = true;
        }

        // from RebirthPowerSpells.spellTooltip()
        private static float bloodToAdvStats(double blood)
        {
            var bonus = (float)Math.Floor(Math.Pow(blood, 0.25));
            if (Plugin.Character.settings.rebirthDifficulty >= difficulty.evil)
                bonus *= Plugin.Character.adventureController.itopod.ironPillBonus();

            return bonus;
        }

        // AllBloodMagicController.lootBonus()
        // Math.Floor(Math.Log(character.bloodMagic.lootSpellBlood / character.bloodMagicController.spells.minLootBlood(), 2.0) + 1.0) * 0.009999999776482582
        private static long bloodToLootBonus(double blood)
        {
            if (blood < MINBLOOD_LOOT)
                return 0L;

            return (long)Math.Floor(Math.Log(blood / MINBLOOD_LOOT, 2.0) + 1.0);// * 0.01;
        }

        private static double lootBonusToBlood(long lootBonus)
        {
            return MINBLOOD_LOOT * Math.Pow(2.0, lootBonus - 1);
        }

        // AllBloodMagicController.goldBonus()
        // Math.Floor(Math.Pow(Math.Log(character.bloodMagic.goldSpellBlood / character.bloodMagicController.spells.minGoldBlood(), 2.0) + 1.0, 2.0)) * 0.009999999776482582
        private static long bloodToGoldBonus(double blood)
        {
            if (blood < MINBLOOD_GOLD)
                return 0L;

            return (long)Math.Floor(Math.Pow(Math.Log(blood / MINBLOOD_GOLD, 2.0) + 1.0, 2.0));// * 0.01;
        }

        private static double goldBonusToBlood(long goldBonus)
        {
            return MINBLOOD_GOLD * Math.Pow(2.0, Math.Sqrt(goldBonus) - 1);
        }

        private static int getAutoCastCount()
        {
            var count = 0;
            var bm = Plugin.Character.bloodMagic;

            if (bm.goldAutoSpell)
                count++;

            if (bm.lootAutoSpell)
                count++;

            if (bm.rebirthAutoSpell)
                count++;

            return count;
        }

        private static NotifiedSpells _notifySpells = Options.BloodMagic.NotifiedSpells.Value;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance)
        {
            var character = __instance.character;
            if (character.bossID < 37)
                return;

            var showIP = (_notifySpells & NotifiedSpells.IP) == NotifiedSpells.IP && character.bloodMagic.adventureSpellTime.totalseconds >= character.bloodMagicController.spells.adventureSpellCooldown;
            var showGuffA = (_notifySpells & NotifiedSpells.GUFFA) == NotifiedSpells.GUFFA && character.adventure.itopod.perkLevel[72] >= 1 && character.bloodMagic.macguffin1Time.totalseconds >= character.bloodMagicController.spells.macguffin1Cooldown;
            var showGuffB = (_notifySpells & NotifiedSpells.GUFFB) == NotifiedSpells.GUFFB && character.adventure.itopod.perkLevel[73] >= 1 && character.bloodMagic.macguffin2Time.totalseconds >= character.bloodMagicController.spells.macguffin2Cooldown;

            // the game should already be highlighting if any of those spells are ready
            if (showIP || showGuffA || showGuffB)
                return;

            __instance.bloodMagic.image.color = Color.white;
        }
    }
}
