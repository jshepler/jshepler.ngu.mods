using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using jshepler.ngu.mods.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PerkList
    {
        private static int[] _fibPerkBonusLevels = [1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597];
        private static PerkListPopup _popup = new();

        internal static List<int> Perks => Data.PerkList;

        internal static bool FilterEnabled
        {
            get => Options.PerkList.FilterEnabled.Value;
            set => Options.PerkList.FilterEnabled.Value = value;
        }

        internal static bool OrderEnabled
        {
            get => Options.PerkList.OrderEnabled.Value;
            set => Options.PerkList.OrderEnabled.Value = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "Start")]
        private static void ItopodPerkController_Start_postfix()
        {
            cloneFilters();

            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkUIController), "OnPointerClick")]
        private static bool ItopodPerkUIController_OnPointerClick_prefix(PointerEventData eventData, ItopodPerkUIController __instance)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !Plugin.ShiftIsDown)
                return true;

            var perkId = __instance.id;
            if (Perks.Contains(perkId))
                Perks.Remove(perkId);
            else
                Perks.Add(perkId);

            __instance.updateGraphic();

            return false;
        }

        private static Func<int, bool> isMaxed = (int perkId) => Plugin.Character.adventure.itopod.perkLevel[perkId] >= Plugin.Character.adventureController.itopod.maxLevel[perkId];

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "doEffect")]
        private static void ItopodPerkController_doEffect_postfix(int id)
        {
            if (Perks.Contains(id) && isMaxed(id))
                RemovePerk(id);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkUIController), "updateGraphic")]
        private static void ItopodPerkUIController_updateGraphic_postfix(ItopodPerkUIController __instance)
        {
            if (Perks.Contains(__instance.id))
                __instance.itemBorder.color = new Color(0, 1, 1);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "constructFullList")]
        private static bool ItopodPerkController_constructFullList_prefix(ItopodPerkController __instance)
        {
            if (!FilterEnabled)
                return true;

            __instance.curValidUpgradesList.Clear();
            __instance.curValidUpgradesList.AddRange(Perks);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "orderList")]
        private static void ItopodPerkController_orderList_postfix(ItopodPerkController __instance)
        {
            if (!OrderEnabled)
                return;

            var showing = Perks.Intersect(__instance.curValidUpgradesList);
            var except = __instance.curValidUpgradesList.Except(Perks);
            __instance.curValidUpgradesList = [.. showing, .. except];
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (Plugin.Character == null || !Plugin.Character.InMenu(Menu.Perks) || !Input.GetKeyDown(KeyCode.F1))
                return;

            if (_popup.IsOpen)
                _popup.Close();
            else
                _popup.Open();
        }

        internal static bool CanBuyPerk(int perkId)
        {
            var character = Plugin.Character;
            var controller = character.adventureController.itopod;
            var itopod = character.adventure.itopod;

            var level = itopod.perkLevel[perkId];
            var maxLevel = controller.maxLevel[perkId];
            if (level >= maxLevel)
                return false;

            var pp = itopod.perkPoints;
            var cost = controller.perkCost(perkId);

            if (perkId == 94)
            {
                var nextBonusLevel = _fibPerkBonusLevels.First(i => i > level);
                cost *= (nextBonusLevel - level);
            }

            return pp >= cost;
        }

        internal static void BuyPerk(int perkId)
        {
            var character = Plugin.Character;
            var controller = character.adventureController.itopod;
            var itopod = character.adventure.itopod;

            var level = itopod.perkLevel[perkId];
            var maxLevel = controller.maxLevel[perkId];
            if (level >= maxLevel)
                return;

            var pp = itopod.perkPoints;
            var cost = controller.perkCost(perkId);
            var buyLevels = 1L;

            if (perkId == 94)
            {
                var nextBonusLevel = _fibPerkBonusLevels.First(i => i > level);
                buyLevels = nextBonusLevel - level;
                cost *= buyLevels;
            }

            if (cost > pp)
                return;

            itopod.perkPoints -= cost;
            itopod.perkLevel[perkId] += buyLevels;
            controller.doEffect(perkId);

            if (level + buyLevels == maxLevel)
                RemovePerk(perkId);
        }

        internal static void BulkBuyPerk(int perkId)
        {
            while (CanBuyPerk(perkId))
                BuyPerk(perkId);
        }

        internal static void MovePerk(int perkId, bool moveDown)
        {
            var index = Perks.IndexOf(perkId);
            if (index == -1)
                return;

            if (moveDown && index < Perks.Count - 1)
            {
                Perks.Remove(perkId);
                Perks.Insert(index + 1, perkId);
            }

            else if (!moveDown && index > 0)
            {
                Perks.Remove(perkId);
                Perks.Insert(index - 1, perkId);
            }
        }

        internal static void RemovePerk(int perkId)
        {
            if (Perks.Contains(perkId))
            {
                Perks.Remove(perkId);

                var controller = Plugin.Character.adventureController.itopod;
                controller.changePage(controller.page);
            }
        }

        internal static void ClearPerks()
        {
            Perks.Clear();

            var controller = Plugin.Character.adventureController.itopod;
            controller.changePage(controller.page);
        }


        private static Image _filterCheckmark;
        private static Image _orderCheckmark;

        private static void cloneFilters()
        {
            var orig = GameObject.Find("Canvas/ITOPOD Perks Canvas/Item List Page 1 Menu/Top Panel/Filter Title");
            var clone = GameObject.Instantiate(orig, orig.transform.parent, false);
            clone.name = "List Options";

            var rectClone = clone.GetComponent<RectTransform>();
            rectClone.anchoredPosition += new Vector2(110f, -1f);

            var textClone = clone.GetComponent<Text>();
            textClone.text = "<b>Perk List:</b>";
            textClone.alignment = TextAnchor.MiddleLeft;

            var filterToggleClone = clone.transform.Find("Difficulty Toggle");
            filterToggleClone.Find("Text").GetComponent<Text>().text = "Filter";
            _filterCheckmark = filterToggleClone.Find("Checkmark").GetComponent<Image>();

            var filterToggleButtonClone = filterToggleClone.GetComponent<Button>();
            filterToggleButtonClone.onClick.RemoveAllListeners();

            // RemoveAllListeners() only removes non-persistent listeners - i.e. those added from code
            // can't remove persistent listeners (added via unity editor), but can disable them
            var count = filterToggleButtonClone.onClick.GetPersistentEventCount();
            for (var i = count - 1; i >= 0; i--)
                filterToggleButtonClone.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);

            filterToggleButtonClone.onClick.AddListener(onListFilterClick);


            var orderToggleClone = clone.transform.Find("Affordable Toggle");
            orderToggleClone.Find("Text").GetComponent<Text>().text = "Order";
            _orderCheckmark = orderToggleClone.Find("Checkmark").GetComponent<Image>();

            var orderToggleButtonClone = orderToggleClone.GetComponent<Button>();
            orderToggleButtonClone.onClick.RemoveAllListeners();

            count = orderToggleButtonClone.onClick.GetPersistentEventCount();
            for (var i = count - 1; i >= 0; i--)
                orderToggleButtonClone.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);

            orderToggleButtonClone.onClick.AddListener(onListOrderClick);


            GameObject.Destroy(clone.transform.Find("Maxxed Toggle").gameObject);
        }

        private static void onListFilterClick()
        {
            FilterEnabled = !FilterEnabled;
            Plugin.Character.adventureController.itopod.onFilterChange();
        }

        private static void onListOrderClick()
        {
            OrderEnabled = !OrderEnabled;
            Plugin.Character.adventureController.itopod.onOrderChange();
            Plugin.Character.adventureController.itopod.updateFilters();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "updateFilters")]
        private static void ItopodPerkController_updateFilters_postfix()
        {
            _filterCheckmark.color = FilterEnabled ? Color.white : Color.clear;
            _orderCheckmark.color = OrderEnabled ? Color.white : Color.clear;
        }
    }
}
