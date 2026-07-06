using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardKeys
    {
        const int CARDSPERROW = 5;
        enum Direction { Up, Down, Left, Right }

        private static CardsController _controller;
        private static FieldInfo _deckPageId = typeof(CardsController).GetField("deckpageID", BindingFlags.Instance | BindingFlags.NonPublic);
        private static bool _allowPageChange;

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "Start")]
        private static void CardsController_Start_postfix(CardsController __instance)
        {
            _controller = __instance;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.Character.InMenu(Menu.Cards))
                    return;

                _allowPageChange = true;

                if (Input.GetKeyDown(KeyCode.Delete))
                    _controller.trashCurrentCard();

                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    _controller.tryConsumeCurrentCard();

                else if (Input.GetKeyDown(KeyCode.Space))
                    _controller.protectCurrentCard();

                else if (Input.GetKeyDown(KeyCode.PageUp))
                    _controller.deckPageBack();

                else if (Input.GetKeyDown(KeyCode.PageDown))
                    _controller.deckPageForward();

                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    MoveSelection(Direction.Up);

                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    MoveSelection(Direction.Down);

                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    MoveSelection(Direction.Left);

                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    MoveSelection(Direction.Right);

                _allowPageChange = false;
            };
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(CardsController), "deckPageBack"),
            HarmonyPatch(typeof(CardsController), "deckPageForward")]
        private static bool CardsController_changePage_prefix()
        {
            return _allowPageChange;
        }

        private static void MoveSelection(Direction direction)
        {
            var cardsPerPage = _controller.deckPods.Count;
            var pageId = (int)_deckPageId.GetValue(_controller);
            var firstIndex = pageId * cardsPerPage;
            var lastIndex = (pageId + 1) * cardsPerPage - 1;
            var newIndex = _controller.curSelectedCard;
            var cardsCount = Plugin.Character.cards.cards.Count;

            switch (direction)
            {
                case Direction.Up:
                    if (newIndex - CARDSPERROW >= firstIndex)
                        newIndex -= 5;
                    break;

                case Direction.Down:
                    if (newIndex + CARDSPERROW <= lastIndex && newIndex + CARDSPERROW < cardsCount)
                        newIndex += 5;
                    break;

                case Direction.Left:
                    if (newIndex > firstIndex)
                        newIndex--;
                    break;

                case Direction.Right:
                    if (newIndex < lastIndex && newIndex + 1 < cardsCount)
                        newIndex++;
                    break;
            }

            if (newIndex != _controller.curSelectedCard)
                _controller.selectNewCard(newIndex);
        }
    }
}
