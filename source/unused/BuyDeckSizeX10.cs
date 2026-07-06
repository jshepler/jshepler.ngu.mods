using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BuyDeckSizeX10
    {
        private const int PURCHASE_ID = 75;
        private static bool _shiftIsDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static bool _buy10 = false;
        private static Text _buttonText;

        [HarmonyPostfix, HarmonyPatch(typeof(ArbitraryController), "Start")]
        private static void ArbitraryController_Start_postfix(ArbitraryController __instance)
        {
            if (__instance.id != PURCHASE_ID)
                return;

            _buttonText = __instance.buyAPButton.GetComponentInChildren<Text>();

            Plugin.OnUpdate += (o, e) =>
            {
                if (_buy10 != _shiftIsDown && Plugin.Character.InMenu(Menu.Shop_Shop8_Special4))
                {
                    _buy10 = _shiftIsDown;
                    __instance.updateMenu();
                }
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startDeckSlotAP")]
        private static bool ArbitraryController_startDeckSlotAP_prefix(ArbitraryController __instance)
        {
            if (!_buy10)
                return true;

            var character = __instance.character;
            var tooltip = __instance.tooltip;

            if (!character.cards.cardsOn)
                tooltip.showTooltip("You don't even have Cards to begin with - Come back once you actually have them, buttbreath.", 2.5f);

            else if (character.arbitrary.curArbitraryPoints < 250000L)
                tooltip.showTooltip("You don't have enough AP to buy " + __instance.itemName + "!", 2f);

            else if (character.arbitrary.deckSpaceBought >= __instance.maxDeckSpaces() * 10)
                tooltip.showTooltip("You've already bought all the Max Deck Size you can! That deck is so huge it could be used as a murder weapon...", 2.5f);

            else
            {
                character.arbitrary.curArbitraryPoints -= __instance.cost() * 10;
                character.arbitrary.deckSpaceBought += 10;
                tooltip.showTooltip("You've successfully bought 10x " + __instance.itemName + "!", 2f);
                __instance.updateMenu();
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ArbitraryController), "updateMenu")]
        private static void ArbitraryController_updateMenu_postfix(ArbitraryController __instance)
        {
            if (__instance.id != PURCHASE_ID || __instance.shouldDisableBuyButton(__instance.id))
                return;

            _buttonText.text = _buy10 ? "Buy 10 for 250,000 AP" : "Buy for 25,000 AP";
        }
    }
}
