using System.CodeDom;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class NumerKeysChangePage
    {
        private const int WISH_PODS_PER_PAGE = 21;

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "Update")]
        private static bool Character_Update_prefix(Character __instance)
        {
            // if typing in a text box...
            if (EventSystem.current.currentSelectedGameObject != null)
                return true;

            int numberKey;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                numberKey = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                numberKey = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                numberKey = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                numberKey = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                numberKey = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
                numberKey = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
                numberKey = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                numberKey = 8;
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                numberKey = 9;
            else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                numberKey = 10;
            else
                return true;

            switch (__instance.CurrentMenu())
            {
                case Menu.Yggdrasil:
                    if (numberKey < 4)
                    {
                        __instance.yggdrasilController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.GoldDiggers:
                    if (numberKey < 4)
                    {
                        __instance.allDiggers.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.NGU_Energy:
                    if (numberKey == 2)
                    {
                        __instance.menuSwapper.swapMenu((int)Menu.NGU_Magic);
                        return false;
                    }

                    return true;


                case Menu.NGU_Magic:
                    if (numberKey == 1)
                    {
                        __instance.menuSwapper.swapMenu((int)Menu.NGU_Energy);
                        return false;
                    }

                    return true;


                case Menu.Beards:
                    if (numberKey < 8)
                    {
                        __instance.allBeards.beard.changeID(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.Hacks:
                    if (numberKey < 3)
                    {
                        __instance.hacksController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.Wishes:
                    var lastPage = __instance.wishesController.curValidUpgradesList.Count / WISH_PODS_PER_PAGE;
                    if (numberKey - 1 <= lastPage)
                    {
                        __instance.wishesController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;

                case Menu.Perks:
                    if (numberKey < 4)
                    {
                        __instance.adventureController.itopod.changePage(numberKey - 1);
                        return false;
                    }

                    return true;

                case Menu.Quirks:
                    if (numberKey < 3)
                    {
                        __instance.beastQuestPerkController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;

                case Menu.ItemList:
                    if (numberKey < 6)
                    {
                        __instance.allItemList.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                default:
                    return true;
            }
        }


        // highlight current inventory page button
        private static int _inventoryPageId;

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "changePage", [typeof(int)])]
        private static void InventoryController_changePage_postfix(int pageID, InventoryController __instance)
        {
            _inventoryPageId = pageID;
            __instance.updatePageButtons();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updatePageButtons")]
        private static void InventoryController_updatePageButtons_postfix(InventoryController __instance)
        {
            var buttons = __instance.pageButtons;
            for (var x = 0; x < buttons.Length; x++)
                buttons[x].image.color = (x == _inventoryPageId) ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current item list page button
        private static Button[] _itemListPageButtons;

        [HarmonyPostfix,
            HarmonyPatch(typeof(AllItemListController), "refreshMenu"),
            HarmonyPatch(typeof(AllItemListController), "changePage")]
        private static void AllItemListController_updateSelectedPage()
        {
            if (Plugin.Character == null)
                return;

            if (_itemListPageButtons == null)
            {
                var menu = GameObject.Find("Canvas/Item Page 1 Canvas/Item List Page 1 Menu").transform;
                _itemListPageButtons =
                [
                    menu.Find("Page 1").GetComponent<Button>(),
                    menu.Find("Page 2").GetComponent<Button>(),
                    menu.Find("Page 3").GetComponent<Button>(),
                    menu.Find("Page 4").GetComponent<Button>(),
                    menu.Find("Page 4 (1)").GetComponent<Button>(),
                ];
            }

            var currentPage = Plugin.Character.allItemList.itemList[0].id / 108;
            for (var x = 0; x < 5; x++)
                _itemListPageButtons[x].image.color = x == currentPage ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current ygg page
        private static Button[] _yggButtons;

        [HarmonyPostfix,
            HarmonyPatch(typeof(AllYggdrasil), "updateDisplay"),
            HarmonyPatch(typeof(AllYggdrasil), "changePage")]
        private static void AllYggdresil_updateDisplay_postfix(AllYggdrasil __instance)
        {
            if (_yggButtons == null)
            {
                var menu = GameObject.Find("Canvas/Yggdrasil Canvas/Yggdrasil, The World Tree").transform;
                _yggButtons =
                [
                    menu.Find("Page 1").GetComponent<Button>(),
                    menu.Find("Page 2").GetComponent<Button>(),
                    menu.Find("Page 3").GetComponent<Button>()
                ];
            }

            var curPage = __instance.curPage;
            for (var x = 0; x < _yggButtons.Length; x++)
                _yggButtons[x].image.color = (x == curPage) ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current hacks page
        // Canvas/Hacks Canvas/Hacks Menu + HacksController/Page 1 Button
        // Canvas/Hacks Canvas/Hacks Menu + HacksController/Page 2
        private static Button[] _hacksButtons;

        [HarmonyPostfix,
            HarmonyPatch(typeof(HacksController), "refreshMenu"),
            HarmonyPatch(typeof(HacksController), "changePage")]
        private static void HacksController_changePage_postfix(HacksController __instance)
        {
            if (_hacksButtons == null)
            {
                var menu = GameObject.Find("Canvas/Hacks Canvas/Hacks Menu + HacksController").transform;
                _hacksButtons =
                [
                    menu.Find("Page 1 Button").GetComponent<Button>(),
                    menu.Find("Page 2").GetComponent<Button>()
                ];
            }

            var curPage = __instance.curPage;
            for (var x = 0; x < _hacksButtons.Length; x++)
                _hacksButtons[x].image.color = (x == curPage) ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current wishes page
        private static int _wishesPage;

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "changePage")]
        private static void WishesController_changePage_postfix(int newPage, WishesController __instance)
        {
            _wishesPage = newPage;
            __instance.updatePageButtons();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "updatePageButtons")]
        private static void WishesController_updatePageButtons_postfix(WishesController __instance)
        {
            var pageButtons = __instance.pageButtons;
            var numButtons = Mathf.Min(Mathf.CeilToInt((float)__instance.curValidUpgradesList.Count / (float)__instance.pods.Count), pageButtons.Count);
            for (var x = 0; x < numButtons; x++)
                pageButtons[x].image.color = (x == _wishesPage) ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current cards page
        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "setDeckPage")]
        private static void CardsController_setDeckPage_postfix(CardsController __instance)
        {
            __instance.updateDeckButtons();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "updateDeckButtons")]
        private static void CardsController_updateDeckButtons_postfix(CardsController __instance, int ___deckpageID)
        {
            var buttons = __instance.deckPageButtons;
            for (var x = 0; x < buttons.Count; x++)
                buttons[x].image.color = x == ___deckpageID ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current perks page button
        // Canvas/ITOPOD Perks Canvas/Item List Page 1 Menu/
        private static Button[] _perkPageButtons;

        [HarmonyPostfix,
            HarmonyPatch(typeof(ItopodPerkController), "updateMenu"),
            HarmonyPatch(typeof(ItopodPerkController), "changePage")]
        private static void ItopodPerkController_changePage_postfix(ItopodPerkController __instance)
        {
            if (_perkPageButtons == null)
            {
                var menu = GameObject.Find("Canvas/ITOPOD Perks Canvas/Item List Page 1 Menu").transform;
                _perkPageButtons =
                [
                    menu.Find("Page 1").GetComponent<Button>(),
                    menu.Find("Page 2").GetComponent<Button>(),
                    menu.Find("Page 3").GetComponent<Button>()
                ];
            }

            var curPage = __instance.page;
            for (var x = 0; x < _perkPageButtons.Length; x++)
                _perkPageButtons[x].image.color = (x == curPage) ? Plugin.ButtonColor_Yellow : Color.white;
        }


        // highlight current quirks page button
        private static Button[] _quirkPageButtons;

        // fixes bug where page field isn't set when changePage is called
        // compare BeastQuestPerkController.chagnePage() with ItopodPerkController.changePage()
        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestPerkController), "changePage")]
        private static IEnumerable<CodeInstruction> BeastQuestPerkController_changePage_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var page = typeof(BeastQuestPerkController).GetField("page");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ret))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Stfld, page));

            return cm.InstructionEnumeration();
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(BeastQuestPerkController), "refreshMenu"),
            HarmonyPatch(typeof(BeastQuestPerkController), "changePage")]
        private static void ItopodPerkController_changePage_postfix(BeastQuestPerkController __instance)
        {
            if (_quirkPageButtons == null)
            {
                var menu = GameObject.Find("Canvas/Beast Quirks Canvas/Beast Quirks Menu/").transform;
                _quirkPageButtons =
                [
                    menu.Find("Page 1").GetComponent<Button>(),
                    menu.Find("Page 2").GetComponent<Button>()
                ];
            }

            var curPage = __instance.page;
            for (var x = 0; x < _quirkPageButtons.Length; x++)
                _quirkPageButtons[x].image.color = (x == curPage) ? Plugin.ButtonColor_Yellow : Color.white;
        }
    }
}
