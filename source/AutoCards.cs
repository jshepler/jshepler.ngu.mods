using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    internal enum CardSortDirection { Descending = -1, Ascending = 1 }

    [HarmonyPatch]
    internal class AutoCards
    {
        // skip doing auto sort/yeet when an auto-yeet is in progress to guard against changing the list while auto-yeeting
        private static bool _autoYeetInProgress = false;
        private static Card _autoYeetedCard = null;

        private static bool _autoSortEnabled = Options.AutoCards.AutoSortEnabled.Value;
        private static int _sortDirection = (int)Options.AutoCards.AutoSortDirection.Value;
        private static bool _autoYeetEnabled = Options.AutoCards.AutoYeetEnabled.Value;
        private static rarity _maxYeetRarity = Options.AutoCards.MaxYeetRarity.Value;

        [HarmonyPostfix
            , HarmonyPatch(typeof(CardsController), "addCard")
            , HarmonyPatch(typeof(CardsController), "addChonkerCard")]
        private static void CardsController_addCards_postfix()
        {
            if (_autoYeetInProgress)
                return;

            if (_autoYeetEnabled)
            {
                _autoYeetInProgress = true;
                yeetCards();
                _autoYeetInProgress = false;
            }

            if (_autoSortEnabled)
                sortCards();
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
                sortCards();
        }

        // uses a higher priority than ToastNotifications.cs to make sure the message is changed before the toast mod grabs it
        [HarmonyPrefix, HarmonyPriority(255), HarmonyPatch(typeof(HoverTooltip), "showTooltip", typeof(string), typeof(float))]
        private static void HoverTooltip_showTooltip_prefix(ref string message)
        {
            if (_autoYeetedCard == null)
                return;

            message = $"<b><color=blue>AUTO-YEET:</color> <color=red>{_autoYeetedCard.cardRarity}</color></b>\n\n{message}";
        }

        private static void yeetCards()
        {
            var character = Plugin.Character;
            var cards = character.cards.cards;
            var index = cards.Count;

            while (index > 0)
            {
                var card = cards[--index];
                if (card.cardRarity <= _maxYeetRarity && card.type != cardType.end)
                {
                    _autoYeetedCard = card;
                    character.cardsController.trashCard(index);
                    _autoYeetedCard = null;
                }
            }
        }

        private static void sortCards()
        {
            var character = Plugin.Character;

            character.cards.cards.Sort(cardComparer);
            character.cardsController.updateDeckPods();
            character.cardsController.updateDeckButtons();
        }

        private static int cardComparer(Card a, Card b)
        {
            if (a.cardRarity != b.cardRarity)
                return a.cardRarity.CompareTo(b.cardRarity) * _sortDirection;

            if (a.bonusType != b.bonusType)
                return a.bonusType.CompareTo(b.bonusType) * _sortDirection;

            return a.effectAmount.CompareTo(b.effectAmount) * _sortDirection;
        }
    }
}
