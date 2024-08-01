using System;
using System.Collections;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestRewardsBreakdown
    {
        private static HoverTooltip _tooltip;
        private static Coroutine _coroutine;
        private static WaitForSeconds _wait1 = new WaitForSeconds(1f);

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "Start")]
        private static void BeastQuestController_Start_postfix(BeastQuestController __instance)
        {
            _tooltip = __instance.character.tooltip;

            __instance.questDescription.gameObject.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnPointerExit);
        }

        private static void OnPointerEnter(PointerEventData e)
        {
            if (_coroutine != null)
                Plugin.Character.StopCoroutine(_coroutine);

            _coroutine = Plugin.Character.StartCoroutine(ShowTooltip());
        }

        private static void OnPointerExit(PointerEventData e)
        {
            if (_coroutine != null)
                Plugin.Character.StopCoroutine(_coroutine);

            _coroutine = null;
            _tooltip.hideTooltip();
        }

        private static IEnumerator ShowTooltip()
        {
            while (true)
            {
                _tooltip.showTooltip(BuildBreakdown());
                yield return _wait1;
            }
        }

        private static string BuildBreakdown()
        {
            var c = Plugin.Character;
            var q = c.beastQuest;
            var qc = c.beastQuestController;

            if (!q.inQuest)
                return "not in quest";

            var sb = new StringBuilder();
            var baseReward = 10L;

            if (q.reducedRewards)
            {
                sb.Append($"base: {baseReward} (minor)");

                if (c.adventure.itopod.perkLevel[87] > 0)
                {
                    sb.Append($"\n  perk 87: +2");
                    baseReward += 2;
                }

                if (c.adventure.itopod.perkLevel[148] > 0)
                {
                    sb.Append($"\n  perk 148: +{c.adventure.itopod.perkLevel[148]}");
                    baseReward += c.adventure.itopod.perkLevel[148];
                }

                if (c.wishes.wishes[102].level > 0)
                {
                    sb.Append($"\n  wish 102: +{c.wishes.wishes[102].level}");
                    baseReward += c.wishes.wishes[102].level;
                }

                if (baseReward > 16)
                {
                    sb.Append($"\n  (capped at 16)");
                    baseReward = 16;
                }
            }

            else
            {
                baseReward = 50L;
                sb.Append($"base: {baseReward} (major)");

                if (c.adventure.itopod.perkLevel[147] > 0)
                {
                    sb.Append($"\n  perk 147: +{c.adventure.itopod.perkLevel[147]}");
                    baseReward += c.adventure.itopod.perkLevel[147];
                }

                if (c.wishes.wishes[101].level > 0)
                {
                    sb.Append($"\n  wish 101: +{c.wishes.wishes[101].level}");
                    baseReward += c.wishes.wishes[101].level;
                }
            }

            sb.Append($"\n<b>Total Base QP:</b> {baseReward:#,##0}\n");
            var totalMulti = 1d;

            var questItemsMaxed = qc.questItemsMaxxed();
            if (questItemsMaxed > 0)
            {
                var questItemsMaxedMulti = Math.Pow(1.02f, questItemsMaxed);
                sb.Append($"\n{questItemsMaxed} Quest Items Maxed: x{questItemsMaxedMulti:0.#####}");
                totalMulti *= questItemsMaxedMulti;
            }

            if (c.inventory.itemList.orangeHeartComplete)
            {
                sb.Append($"\nOrange Heart: x1.2");
                totalMulti *= 1.2d;
            }

            if (c.inventory.itemList.godmotherComplete)
            {
                sb.Append($"\nMobster Set: x1.15");
                totalMulti *= 1.15d;
            }

            var perksMulti = c.adventureController.itopod.totalQPBonus();
            if (c.adventure.itopod.perkLevel[94] >= 233)
                perksMulti *= 1.1f;

            if (perksMulti > 1)
            {
                sb.Append($"\nPerks Multiplier: x{perksMulti:0.#####}");
                totalMulti *= perksMulti;
            }

            var hacksMulti = c.hacksController.totalQPGainBonus();
            if (hacksMulti > 1)
            {
                sb.Append($"\nHacks Multiplier: x{hacksMulti:0.#####}");
                totalMulti *= hacksMulti;
            }

            var wishesMulti = c.wishesController.totalQPBonus();
            if (wishesMulti > 1)
            {
                sb.Append($"\nWishes Multiplier: x{wishesMulti:0.#####}");
                totalMulti *= wishesMulti;
            }

            var cardsMulti = c.cardsController.getBonus(cardBonus.QP);
            if (cardsMulti > 1)
            {
                sb.Append($"\nCards Multiplier: x{cardsMulti:0.#####}");
                totalMulti *= cardsMulti;
            }

            if (q.usedButter)
            {
                var butterMulti = c.allArbitrary.butterModifier();
                sb.Append($"\nUsed Butter: x{butterMulti:0.#####}");
                totalMulti *= butterMulti;
            }

            sb.Append($"\n<b>Total Multiplier:</b> x{totalMulti:#,##0.#####}");
            
            var subTotal = (long)(baseReward * totalMulti);
            sb.Append($"\n\n<b>Sub-Total Reward:</b> {subTotal} QP (truncated)");

            var activeModifier = q.allActive ? qc.allActiveModifier() : 1f;
            sb.Append($"\n\n{(q.allActive ? "Manual" : "Idle")}: x{activeModifier:0.#####}");
            
            var total = (long)(subTotal * activeModifier);
            sb.Append($"\n<b>Total Quest Reward:</b> {total} QP (truncated)");

            return sb.ToString();
        }
    }
}
