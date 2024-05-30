using System;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BloodMagic
    {
        // based on AllBloodMagicController.lootBonus() and .goldBonus()
        const double MINBLOOD_LOOT = 10000.0;
        const double MINBLOOD_GOLD = 1000000.0;
        private static Func<double, float> _calcLootBonusPct = blood => (blood < MINBLOOD_LOOT) ? 0f : 100F * (float)(Math.Floor(Math.Log(blood / MINBLOOD_LOOT, 2.0) + 1.0) * 0.0099999997764825821);
        private static Func<double, float> _calcGoldBonusPct = blood => (blood < MINBLOOD_GOLD) ? 0f : 100f * (float)(Math.Floor(Math.Pow(Math.Log(blood / MINBLOOD_GOLD, 2.0) + 1.0, 2.0)) * 0.0099999997764825821);

        [HarmonyPostfix, HarmonyPatch(typeof(AllBloodMagicController), "updateBloodDisplay")]
        private static void AllBloodMagicController_updateBloodDisplay_postifx(AllBloodMagicController __instance)
        {
            var totalBPS = __instance.bloodMagics.Sum(bm => bm.bloodGainedPerSecond());
            var fontSize = __instance.bloodText.fontSize * .9;
            
            __instance.bloodText.text += $"\n<size={fontSize}><b>Gain:</b> +{__instance.character.display(totalBPS)}/s</size>";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerSpell), "lootSpellTooltip")]
        private static bool RebirthPowerSpells_lootSpellTooltip_prefix(RebirthPowerSpell __instance, ref string ___message)
        {
            var character = __instance.character;

            var lootSpellBlood = character.bloodMagic.lootSpellBlood;
            var oldPct = _calcLootBonusPct(lootSpellBlood);
            
            var newTotal = lootSpellBlood + character.bloodMagic.bloodPoints;
            var newPct = _calcLootBonusPct(newTotal);
            var willGain = newPct - oldPct;

            ___message = $"<b>Blood Spaghetti</b>\n\nGather up all of your Blood and form it into something resembling spaghetti. You can slip spaghetti into a foe's pockets, causing it (and whatever loot they're holding onto) to fall out more often!\n\nFor you math nerds, it's log2(Blood/{MINBLOOD_LOOT}) % better drop chance.\n\n<b>Minimum Blood Required: </b>{MINBLOOD_LOOT}\n<b>Total Blood Invested: </b>{character.display(lootSpellBlood)}\n\n<b>Will gain +{willGain:#,##0.#}% if used now.</b>";
            __instance.tooltip.showTooltip(___message);

            if (!__instance.IsInvoking("lootSpellTooltip"))
            {
                __instance.InvokeRepeating("lootSpellTooltip", 0, 1);
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerSpell), "goldSpellTooltip")]
        private static bool RebirthPowerSpells_goldSpellTooltip_prefix(RebirthPowerSpell __instance, ref string ___message)
        {
            var character = __instance.character;

            var goldSpellBlood = character.bloodMagic.goldSpellBlood;
            var oldPct = _calcGoldBonusPct(goldSpellBlood);
            
            var newTotal = goldSpellBlood + character.bloodMagic.bloodPoints;
            var newPct = _calcGoldBonusPct(newTotal);
            var willGain = newPct - oldPct;

            ___message = $"<b>Counterfeit Gold</b>\n\nUse the power of Blood to create some counterfeit gold, and slip it into the time machine's time bubble to increase gold production! Lasts until rebirth.\n\nWARNING: MATH. Your bonus GPS is equal to log2(Blood/{MINBLOOD_GOLD})^2%.\n\n<b>Minimum Blood Required: </b>{MINBLOOD_GOLD}\n<b>Total Blood Invested: </b>{character.display(goldSpellBlood)}\n\n<b>Will gain +{willGain:#,##0.#}% if used now.</b>";
            __instance.tooltip.showTooltip(___message);

            if (!__instance.IsInvoking("goldSpellTooltip"))
            {
                __instance.InvokeRepeating("goldSpellTooltip", 0, 1);
            }

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
        }
    }
}
