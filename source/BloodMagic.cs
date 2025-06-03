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

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerSpell), "lootSpellTooltip")]
        private static bool RebirthPowerSpells_lootSpellTooltip_prefix(RebirthPowerSpell __instance, ref string ___message)
        {
            var bm = __instance.character.bloodMagic;
            var bloodInvested = bm.lootSpellBlood;

            ___message = "<b>Blood Spaghetti</b>"
                + "\n\nGather up all of your Blood and form it into something resembling spaghetti. You can slip spaghetti into a foe's pockets, causing it (and whatever loot they're holding onto) to fall out more often!"
                + $"\n\nFor you math nerds, it's log2(Blood/{MINBLOOD_LOOT}) % better drop chance."
                + $"\n\n<b>Minimum Blood Required: </b>{MINBLOOD_LOOT}"
                + $"\n<b>Total Blood Invested: </b>{__instance.character.display(bloodInvested)}";

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
            if (!__instance.IsInvoking("lootSpellTooltip"))
                __instance.InvokeRepeating("lootSpellTooltip", 0, 0.1f);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerSpell), "goldSpellTooltip")]
        private static bool RebirthPowerSpells_goldSpellTooltip_prefix(RebirthPowerSpell __instance, ref string ___message)
        {
            var bm = __instance.character.bloodMagic;
            var bloodInvested = bm.goldSpellBlood;

            ___message = "<b>Counterfeit Gold</b>"
                + "\n\nUse the power of Blood to create some counterfeit gold, and slip it into the time machine's time bubble to increase gold production! Lasts until rebirth."
                + $"\n\nWARNING: MATH. Your bonus GPS is equal to log2(Blood/{MINBLOOD_GOLD})^2%."
                + $"\n\n<b>Minimum Blood Required: </b>{MINBLOOD_GOLD}"
                + $"\n<b>Total Blood Invested: </b>{__instance.character.display(bloodInvested)}";

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
            if (!__instance.IsInvoking("goldSpellTooltip"))
                __instance.InvokeRepeating("goldSpellTooltip", 0, 0.1f);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerSpell), "hideTooltip")]
        private static void RebirthPowerSpells_hideTooltip_postfix(RebirthPowerSpell __instance)
        {
            __instance.CancelInvoke("lootSpellTooltip");
            __instance.CancelInvoke("goldSpellTooltip");
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
