using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishPages
    {
        const int WISHESPERPAGE = 21;
        const int MAXPAGES = 9;
        const int MAXWISHESDISPLAYED = WISHESPERPAGE * MAXPAGES;
        const int FONTSIZE_0 = 16;
        const int FONTSIZE_1 = 14;

        private static int _row = 0;
        private static List<Text> _buttonTexts;

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _buttonTexts = __instance.pageButtons.Select(b => b.gameObject.GetComponentInChildren<Text>()).ToList();
            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "updatePageButtons")]
        private static bool WishesController_updatePageButtons_prefix(WishesController __instance)
        {
            if (_buttonTexts == null)
                return true;

            var wishCount = Math.Max(0, __instance.curValidUpgradesList.Count - (_row * MAXWISHESDISPLAYED));
            var numberOfPages = Mathf.Min(Mathf.CeilToInt((float)wishCount / (float)WISHESPERPAGE), MAXPAGES);

            var buttons = __instance.pageButtons;
            var offset = _row * MAXPAGES + 1;

            for (var x = 0; x < MAXPAGES; x++)
            {
                buttons[x].gameObject.SetActive(x < numberOfPages);
                _buttonTexts[x].fontSize = _row == 0 ? FONTSIZE_0 : FONTSIZE_1;
                _buttonTexts[x].text = $"Page {x + offset}";
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "changePage")]
        private static bool WishesController_changePage_prefix(int newPage, WishesController __instance)
        {
            var pods = __instance.pods;
            var curValidUpgradesList = __instance.curValidUpgradesList;

            int wishIndex = newPage * pods.Count + (_row * MAXWISHESDISPLAYED);
            for (int i = 0; i < pods.Count; i++)
            {
                if (wishIndex < 0 || wishIndex >= curValidUpgradesList.Count)
                {
                    pods[i].id = 100000;
                }
                else
                {
                    pods[i].id = curValidUpgradesList[wishIndex];
                }
                wishIndex++;
                pods[i].updatePod();
            }

            return false;
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (!Plugin.Character.InMenu(Menu.Wishes))
                return;

            var controller = Plugin.Character.wishesController;

            if (Input.GetKeyDown(KeyCode.PageDown) && _row == 0)
            {
                _row = 1;
                controller.updatePageButtons();
                controller.changePage(0);
            }

            else if (Input.GetKeyDown(KeyCode.PageUp) && _row == 1)
            {
                _row = 0;
                controller.updatePageButtons();
                controller.changePage(0);
            }
        }
    }
}
