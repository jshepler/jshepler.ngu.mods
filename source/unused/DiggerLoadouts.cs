using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DiggerLoadouts
    {
        private static Button[] _loadoutButtons = new Button[3];
        private static Text[] _loadoutTexts = new Text[3];
        private static Text _setLoadoutButtonText;
        private static Text _applyLoadoutButtonText;

        private static int _curPage => Plugin.Character.allDiggers.curPage;

        private static Dictionary<int, List<int>> _loadouts => ModSave.Data.DiggerLoadouts;
        private static Dictionary<int, List<long>> _levels => ModSave.Data.DiggerLevels;
        private static int _id
        {
            get => ModSave.Data.CurrentDiggerLoadoutId;
            set => ModSave.Data.CurrentDiggerLoadoutId = value;
        }

        private static bool isAltDown => (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "Start")]
        private static void AllGoldDiggerController_Start_postfix(AllGoldDiggerController __instance)
        {
            var menu = GameObject.Find("Canvas/Gold Digger Canvas/Gold Diggers Menu").transform;
            _loadoutButtons[0] = menu.Find("Page 1").GetComponent<Button>();
            _loadoutTexts[0] = menu.Find("Page 1/Text").GetComponent<Text>();
            _loadoutButtons[1] = menu.Find("Page 2").GetComponent<Button>();
            _loadoutTexts[1] = menu.Find("Page 2/Text").GetComponent<Text>();
            _loadoutButtons[2] = menu.Find("Page 3").GetComponent<Button>();
            _loadoutTexts[2] = menu.Find("Page 3/Text").GetComponent<Text>();
            _setLoadoutButtonText = menu.Find("Set Loadout/Text").GetComponent<Text>();
            _applyLoadoutButtonText = menu.Find("Apply Loadout/Text").GetComponent<Text>();

            Plugin.onGUI += OnGUI;
            Plugin.OnSaveLoaded += OnSaveLoaded;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllGoldDiggerController), "changePage")]
        private static bool AllGoldDiggerController_changePage_prefix(int newPage, AllGoldDiggerController __instance)
        {
            if (!isAltDown)
                return true;

            _id = newPage;

            __instance.clearAllActiveDiggers();
            __instance.character.diggers.loadoutDiggers = _loadouts[_id] ?? new();
            CapSavedDiggers.ApplyDiggerLoadout(__instance, _levels[_id]);

            __instance.refreshMenu();
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "setDiggerLoadout")]
        private static void AllGoldDiggerController_setDiggerLoadout_postfix()
        {
            SaveLoadout();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "updateText")]
        private static void AllGoldDiggerController_updateText_postfix()
        {
            _setLoadoutButtonText.text = $"Save Diggers L{_id + 1}";
            _applyLoadoutButtonText.text = $"Cap Diggers L{_id + 1}";
        }

        private static void OnGUI(object sender, EventArgs e)
        {
            if (Plugin.Character.CurrentMenu() != Menu.GoldDiggers)
                return;

            if (isAltDown)
                for (var x = 0; x < 3; x++)
                {
                    _loadoutTexts[x].text = $"GD L{x + 1}";
                    _loadoutButtons[x].image.color = x == _id ? Plugin.ButtonColor_Yellow : Color.white;
                }

            else
                for (var x = 0; x < 3; x++)
                {
                    _loadoutTexts[x].text = $"Page {x + 1}";
                    _loadoutButtons[x].image.color = x == _curPage ? Plugin.ButtonColor_Yellow : Color.white;
                }
        }

        private static void OnSaveLoaded(object sender, EventArgs e)
        {
            // this is really only for when first running the mod on an existing save to set the current loadout slot
            SaveLoadout();
        }

        private static void SaveLoadout()
        {
            var diggers = Plugin.Character.diggers;

            _loadouts[_id] = diggers.loadoutDiggers;
            _levels[_id] = _loadouts[_id].Select(i => diggers.diggers[i].curLevel).ToList();
        }
    }
}
