using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardTooltip
    {
        private static Coroutine _cor;
        private static bool _altIsDown = false;

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "Start")]
        private static void CardsController_Start_postfix(CardsController __instance)
        {
            __instance.deckPods.Do((ui, i) =>
            {
                ui.pod.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter(i))
                .OnPointerExit(OnPointerExit);
            });

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.Character.InMenu(Menu.Cards))
                {
                    _altIsDown = false;
                    return;
                }

                _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
        }

        private static Action<PointerEventData> OnPointerEnter(int podId)
        {
            return e =>
            {
                StopShowingTooltip();
                _cor = Plugin.BeginCoroutine(ShowTooltip(podId));
            };
        }

        private static void OnPointerExit(PointerEventData e)
        {
            StopShowingTooltip();
        }

        private static void StopShowingTooltip()
        {
            if (_cor != null)
            {
                Plugin.EndCoroutine(_cor);
                Plugin.HideTooltip();
                _cor = null;
            }
        }

        private static WaitForSeconds _delay = new WaitForSeconds(.2f);

        private static IEnumerator ShowTooltip(int podId)
        {
            var character = Plugin.Character;
            var controller = character.cardsController;
            var pod = controller.deckPods[podId];
            var cards = character.cards.cards;

            while (true)
            {
                var cardId = pod.deckID;
                if (cardId < 0 || cardId >= cards.Count)
                    break;

                var card = cards[cardId];
                var isChonker = card.cardRarity == rarity.BigChonker;
                var minMayo = isChonker ? controller.minChonkerMana() : controller.minCardMana();
                var maxMayo = isChonker ? controller.maxChonkerMana() : controller.maxCardMana();

                var minVariance = isChonker ? controller.getMaxVariance() : controller.getMinVariance();
                var maxVariance = controller.getMaxVariance();
                var minBonus = controller.generateCardEffect(card.bonusType, card.tier, minMayo, minVariance, isChonker);
                var maxBonus = controller.generateCardEffect(card.bonusType, card.tier, maxMayo, maxVariance, isChonker);

                var iRarity = (int)card.cardRarity;
                var minRarityVariance = isChonker ? controller.getMaxVariance() : iRarity == 0 ? controller.getMinVariance() : _rarityMaxVariances[iRarity - 1];
                var maxRarityVariance = isChonker ? controller.getMaxVariance() : _rarityMaxVariances[iRarity];
                var minRarityBonus = controller.generateCardEffect(card.bonusType, card.tier, minMayo, minRarityVariance, isChonker);
                var maxRarityBonus = controller.generateCardEffect(card.bonusType, card.tier, maxMayo, maxRarityVariance, isChonker);

                var curBonusPerMayo = card.effectAmount / card.manaCosts.Sum();
                var bestBonusPerMayo = maxBonus / maxMayo;
                var efficiency = curBonusPerMayo / bestBonusPerMayo;

                var curVariance = GetCardVariance(card);
                var relativeCurVariance = curVariance * 100f - 100f;
                var relMinRarVar = minRarityVariance * 100f - 100f;
                var relMaxRarVar = maxRarityVariance * 100f - 100f;

                var rarityColor = controller.getRarityColorTag(card.cardRarity);
                var rarityName = controller.getRarityName(card.cardRarity);
                var effectName = card.type == cardType.end ? "END" : controller.getBonusName(card.bonusType);

                var text = $"<b>{rarityColor}{rarityName}</color> T{card.tier} {effectName}</b>"
                    + $"\n\n<b>Mayo Efficiency:</b> {efficiency * 100f:0.##}%";

                if (card.type != cardType.end)
                {
                    text += $"\n<b>Bonus Variance:</b> {relativeCurVariance:+0.##;-0.##}%";
                    var (qf, ef, fc, sc) = _constants[card.bonusType];
                    var tf = calcTierFactor(card);
                    var mayo = card.manaCosts.Sum();

                    if (_altIsDown)
                        text += $"\n\n<b>Variance Range (rarity):</b> {relMinRarVar:+0.##;-0.##}% to {relMaxRarVar:+0.##;-0.##}%"
                            + $"\n\n<b>Min Bonus:</b> {minBonus * 100f:#,##0.##}%"
                            + $"\n   <b>(in rarity):</b> {minRarityBonus * 100f:#,##0.##}%"
                            + $"\n\n<b>Max Bonus:</b> {maxBonus * 100f:#,##0.##}%"
                            + $"\n   <b>(in rarity):</b> {maxRarityBonus * 100f:#,##0.##}%"
                            + $"\n\n<b>Cur Bonus/Mayo:</b> {curBonusPerMayo * 100f:#,##0.####}%"
                            + $"\n<b>Best Bonus/Mayo:</b> {bestBonusPerMayo * 100f:#,##0.####}%"

                            + $"\n\n<b>quadFactor (QF):</b> {qf:0.#####}"
                            + $"\n<b>expFactor (EF):</b> {ef:0.######}"
                            + $"\n<b>tierFactor (TF):</b>"
                            + $"\n   = 1 + (tier ^ QF) × (EF ^ tier)"
                            + $"\n   = 1 + ({card.tier} ^ {qf:0.#####}) × ({ef:0.#####} ^ {card.tier})"
                            + $"\n   = {tf:r}"

                            + $"\n\n<b>fixedCoeff (FC):</b> {fc:0.#####}"
                            + $"\n<b>scalingCoeff (SC):</b> {sc:0.#####}"
                            + $"\n<b>Bonus:</b>"
                            + $"\n   = (FC + SC × TF × variance) × mayo"
                            + $"\n   = <size=10>({fc:0.#####} + {sc:0.#####} × {tf:r} × {curVariance:r}) × {mayo}</size>"
                            + $"\n   = {card.effectAmount:r}";
                }

                Plugin.ShowTooltip(text);
                yield return _delay;
            }

            StopShowingTooltip();
        }

        internal static float GetCardEfficiency(Card card)
        {
            var controller = Plugin.Character.cardsController;

            var isChonker = card.cardRarity == rarity.BigChonker;
            var maxMayo = isChonker ? controller.maxChonkerMana() : controller.maxCardMana();
            var maxVariance = controller.getMaxVariance();
            var maxBonus = controller.generateCardEffect(card.bonusType, card.tier, maxMayo, maxVariance, isChonker);

            var curBonusPerMayo = card.effectAmount / card.manaCosts.Sum();
            var bestBonusPerMayo = maxBonus / maxMayo;
            var efficiency = curBonusPerMayo / bestBonusPerMayo;

            return efficiency;
        }

        internal static float GetCardVariance(Card card)
        {
            return calcVariance(card.bonusType, card.tier, card.manaCosts.Sum(), card.effectAmount);
        }

        // math provided by blockdude on discord
        private static float calcVariance(cardBonus bonusType, int cardTier, int totalCost, float cardEffect)
        {
            var C = _constants[bonusType];
            return ((cardEffect / totalCost) - C.Item3) / (C.Item4 * Mathf.Pow(cardTier, C.quadFactor) * Mathf.Pow(C.expFactor, cardTier));
        }

        private static float calcTierFactor(Card card)
        {
            var C = _constants[card.bonusType];
            return 1f * Mathf.Pow(card.tier, C.quadFactor) * Mathf.Pow(C.expFactor, card.tier);
        }

        // pulled from CardsController.calculateXEffect methods
        private static Dictionary<cardBonus, (float quadFactor, float expFactor, float fixedCoefficient, float scalingCoefficient)> _constants = new()
        {
            { cardBonus.energyNGUSpeed, (1.2f, 1.03f, 0.0003f, 0.001f) },
            { cardBonus.magicNGUSpeed, (0.8f, 1.08f, 0.0002f, 0.001f) },
            { cardBonus.wandoosSpeed, (0.8f, 1.1f, 0.0002f, 0.001f) },
            { cardBonus.augSpeed, (0.8f, 1.1f, 0.0002f, 0.001f) },
            { cardBonus.TMSpeed, (0.8f, 1.15f, 0.0002f, 0.001f) },
            { cardBonus.hackSpeed, (0.4f, 1.05f, 0.0002f, 0.001f) },
            { cardBonus.wishSpeed, (0.5f, 1.05f, 0.0002f, 0.001f) },
            { cardBonus.atkDefStats, (1.5f, 2f, 0.05f, 0.01f) },
            { cardBonus.adventureStat, (0.4f, 1.07f, 0.0005f, 0.001f) },
            { cardBonus.dropChance, (1f, 1.15f, 0.0002f, 0.001f) },
            { cardBonus.goldDrop, (0.8f, 1.15f, 0.001f, 0.005f) },
            { cardBonus.dayCareSpeed, (0.4f, 1.04f, 5E-05f, 0.0002f) },
            { cardBonus.PP, (0.6f, 1.11f, 0.0001f, 0.0002f) },
            { cardBonus.QP, (0.6f, 1.08f, 0.0001f, 0.0002f) }
        };

        // from CardsController.generateRarity()
        private static float[] _rarityMaxVariances = [0.9f, 1.0f, 1.08f, 1.14f, 1.17f, 1.19f, 1.2f];

        [HarmonyPrefix, HarmonyPatch(typeof(ButtonShower), "showCardStatus")]
        private static bool ButtonShower_showCardStatus_prefix(ButtonShower __instance)
        {
            var character = __instance.character;
            if (!character.cards.cardsOn)
                return false;

            var controller = character.cardsController;
            var cardSpeed = controller.totalCardSpeed();

            var cardSpawnTime = controller.cardSpawnTime() / cardSpeed;
            var timeToNextCard = cardSpawnTime - character.cards.cardSpawnTimer.totalseconds / cardSpeed;
            var cardsPerDay = 86400f / cardSpawnTime;

            var text = $"<b>Card Spawn Time:</b> {NumberOutput.timeOutput(cardSpawnTime)}"
                + $"\n<b>Time to Next Card:</b> {NumberOutput.timeOutput(timeToNextCard)}"
                + $"\n<b>Cards per Day:</b> {cardsPerDay:#,##0.#}";

            if (character.cardsController.unlockedChonkers())
            {
                var chonkerSpawnTime = controller.chonkerSpawnTime() / cardSpeed;
                var timeToNextChonker = chonkerSpawnTime - character.cards.chonkerSpawnTimer.totalseconds / cardSpeed;

                text += $"\n\n<b>CHONKER Spawn Time:</b> {NumberOutput.timeOutput(chonkerSpawnTime)}"
                    + $"\n<b>Time to Next CHONKER:</b> {NumberOutput.timeOutput(timeToNextChonker)}";
            }

            __instance.tooltip.showTooltip(text);
            return false;
        }
    }
}
