using System.IO.Pipes;
using System.Linq;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    internal enum CardSortBy { RarityFirst, TypeFirst, Efficiency, Variance }
    internal enum CardSortDirection { Descending = -1, Ascending = 1 }
    internal enum CardYeetMode { Disabled, Efficiency, Variance, Rarity }

    [HarmonyPatch]
    internal class AutoCards
    {
        // skip doing auto sort/yeet when an auto-yeet is in progress to guard against changing the list while auto-yeeting
        private static bool _autoYeetInProgress = false;
        private static Card _autoYeetedCard = null;

        private static bool _autoSortEnabled => Options.Cards.AutoSortEnabled.Value;
        private static CardSortBy _autoSortBy => Options.Cards.AutoSortBy.Value;
        private static int _sortDirection => (int)Options.Cards.AutoSortDirection.Value;
        private static CardYeetMode _autoYeetMode => Options.Cards.AutoYeetMode.Value;
        private static rarity _maxYeetRarity => Options.Cards.MaxYeetRarity.Value;
        private static float _maxYeetEfficiency => Options.Cards.MaxYeetEfficiency.Value;
        private static float _maxYeetVariance => Options.Cards.MaxYeetVariance.Value;
        private static bool _autoProtectChonkers => Options.Cards.AutoProtectChonkers.Value;

        private static AutoCardsPopup _popup;

        [HarmonyPostfix
            , HarmonyPatch(typeof(CardsController), "addCard")
            , HarmonyPatch(typeof(CardsController), "addChonkerCard")]
        private static void CardsController_addCards_postfix()
        {
            if (_autoYeetInProgress)
                return;

            if (_autoYeetMode != CardYeetMode.Disabled)
            {
                _autoYeetInProgress = true;
                yeetCards();
                _autoYeetInProgress = false;
            }

            if (_autoSortEnabled)
                SortCards();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "generateCard")]
        private static void CardsController_generateCard_postfix(bool isChonker, Card __result)
        {
            if (isChonker)
                __result.isProtected = _autoProtectChonkers;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "Update")]
        private static void CardsController_Update()
        {
            if (!Plugin.Character.InMenu(Menu.Cards))
                return;

            if (Input.GetKeyDown(KeyCode.Y))
            {
                _autoYeetInProgress = true;
                yeetCards();
                _autoYeetInProgress = false;
            }

            if (Input.GetKeyDown(KeyCode.S))
                SortCards();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (_popup == null)
                    _popup = new AutoCardsPopup();

                _popup.Toggle();
            }
        }

        // uses a higher priority than ToastNotifications.cs to make sure the message is changed before the toast mod grabs it
        [HarmonyPrefix, HarmonyPriority(255), HarmonyPatch(typeof(HoverTooltip), "showTooltip", typeof(string), typeof(float))]
        private static void HoverTooltip_showTooltip_prefix(ref string message)
        {
            if (_autoYeetedCard == null)
                return;

            message = $"<b><color=blue>AUTO-YEET</color></b>\n\n{message}";
        }

        private static void yeetCards()
        {
            var character = Plugin.Character;
            var cards = character.cards.cards;
            var index = cards.Count;

            var alwaysYeet = Options.Cards.AlwaysYeetCSV.Value.Split(',').Select(s => s == "1").ToArray();

            while (index > 0)
            {
                var card = cards[--index];
                if (card.isProtected)
                    continue;

                _autoYeetedCard = card;

                if (alwaysYeet[(int)card.bonusType]
                    || (_autoYeetMode == CardYeetMode.Efficiency && CardTooltip.GetCardEfficiency(card) <= _maxYeetEfficiency)
                    || (_autoYeetMode == CardYeetMode.Variance && CardTooltip.GetCardVariance(card) <= _maxYeetVariance)
                    || (_autoYeetMode == CardYeetMode.Rarity && card.cardRarity <= _maxYeetRarity)
                )
                    character.cardsController.trashCard(index);

                _autoYeetedCard = null;
            }
        }

        internal static void SortCards()
        {
            var character = Plugin.Character;

            character.cards.cards.Sort(cardComparer);
            character.cardsController.updateDeckPods();
            character.cardsController.updateDeckButtons();
        }

        private static int cardComparer(Card a, Card b)
        {
            switch (_autoSortBy)
            {
                case CardSortBy.RarityFirst:
                    if (a.cardRarity != b.cardRarity)
                        return a.cardRarity.CompareTo(b.cardRarity) * _sortDirection;

                    if (a.bonusType != b.bonusType)
                        return a.bonusType.CompareTo(b.bonusType) * _sortDirection;

                    return a.effectAmount.CompareTo(b.effectAmount) * _sortDirection;

                case CardSortBy.TypeFirst:
                    if (a.bonusType != b.bonusType)
                        return a.bonusType.CompareTo(b.bonusType) * _sortDirection;

                    if (a.cardRarity != b.cardRarity)
                        return a.cardRarity.CompareTo(b.cardRarity) * _sortDirection;

                    return a.effectAmount.CompareTo(b.effectAmount) * _sortDirection;

                case CardSortBy.Efficiency:
                    return CardTooltip.GetCardEfficiency(a).CompareTo(CardTooltip.GetCardEfficiency(b)) * _sortDirection;

                case CardSortBy.Variance:
                    return CardTooltip.GetCardVariance(a).CompareTo(CardTooltip.GetCardVariance(b)) * _sortDirection;

                default:
                    return CardTooltip.GetCardEfficiency(a).CompareTo(CardTooltip.GetCardEfficiency(b)) * _sortDirection;
            }
        }
    }
}
