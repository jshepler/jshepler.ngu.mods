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


        private static Button[] _itemListPageButtons = null;

        [HarmonyPostfix,
            HarmonyPatch(typeof(AllItemListController), "refreshMenu"),
            HarmonyPatch(typeof(AllItemListController), "changePage")]
        private static void AllItemListController_updateSelectedPage()
        {
            if (Plugin.Character == null)
                return;

            if (_itemListPageButtons == null)
            {
                _itemListPageButtons = new Button[5];
                var pages = GameObject.Find("Canvas/Item Page 1 Canvas/Item List Page 1 Menu")?.transform;

                _itemListPageButtons[0] = pages.Find("Page 1").GetComponent<Button>();
                _itemListPageButtons[1] = pages.Find("Page 2").GetComponent<Button>();
                _itemListPageButtons[2] = pages.Find("Page 3").GetComponent<Button>();
                _itemListPageButtons[3] = pages.Find("Page 4").GetComponent<Button>();
                _itemListPageButtons[4] = pages.Find("Page 4 (1)").GetComponent<Button>();
            }

            var currentPage = Plugin.Character.allItemList.itemList[0].id / 108;

            for (var x = 0; x < 5; x++)
                _itemListPageButtons[x].image.color = x == currentPage ? Plugin.ButtonColor_Yellow : Color.white;
        }
    }
}
