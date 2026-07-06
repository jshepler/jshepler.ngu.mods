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
    internal class QuirkList
    {
        private static QuirkListPopup _popup = new();

        internal static List<int> Quirks => Data.QuirkList;

        internal static bool FilterEnabled
        {
            get => Options.QuirkList.FilterEnabled.Value;
            set => Options.QuirkList.FilterEnabled.Value = value;
        }

        internal static bool OrderEnabled
        {
            get => Options.QuirkList.OrderEnabled.Value;
            set => Options.QuirkList.OrderEnabled.Value = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
        private static void BeastQuestPerkController_Start_postfix()
        {
            cloneFilters();
            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestPerkUIController), "OnPointerClick")]
        private static bool BeastQuestPerkUIController_OnPointerClick_prefix(PointerEventData eventData, BeastQuestPerkUIController __instance)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !Plugin.ShiftIsDown)
                return true;

            var quirkId = __instance.id;
            if (Quirks.Contains(quirkId))
                Quirks.Remove(quirkId);
            else
                Quirks.Add(quirkId);

            __instance.updateGraphic();

            return false;
        }

        private static Func<int, bool> isMaxed = (int quirkId) => Plugin.Character.beastQuest.quirkLevel[quirkId] >= Plugin.Character.beastQuestPerkController.maxLevel[quirkId];

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "doEffect")]
        private static void BeastQuestPerkController_doEffect_postfix(int id)
        {
            if (Quirks.Contains(id) && isMaxed(id))
                RemoveQuirk(id);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkUIController), "updateGraphic")]
        private static void BeastQuestPerkUIController_updateGraphic_postfix(BeastQuestPerkUIController __instance)
        {
            if (Quirks.Contains(__instance.id))
                __instance.itemBorder.color = new Color(0, 1, 1);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestPerkController), "constructFullList")]
        private static bool ItopodPerkController_constructFullList_prefix(BeastQuestPerkController __instance)
        {
            if (!FilterEnabled)
                return true;

            __instance.curValidUpgradesList.Clear();
            __instance.curValidUpgradesList.AddRange(Quirks);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "orderList")]
        private static void ItopodPerkController_orderList_postfix(BeastQuestPerkController __instance)
        {
            if (!OrderEnabled)
                return;

            var showing = Quirks.Intersect(__instance.curValidUpgradesList);
            var except = __instance.curValidUpgradesList.Except(Quirks);
            __instance.curValidUpgradesList = [.. showing, .. except];
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (Plugin.Character == null || !Plugin.Character.InMenu(Menu.Quirks) || !Input.GetKeyDown(KeyCode.F1))
                return;

            if (_popup.IsOpen)
                _popup.Close();
            else
                _popup.Open();
        }

        internal static bool CanBuyQuirk(int quirkId)
        {
            var beastQuest = Plugin.Character.beastQuest;
            var controller = Plugin.Character.beastQuestPerkController;

            var level = beastQuest.quirkLevel[quirkId];
            var maxLevel = controller.maxLevel[quirkId];
            if (level >= maxLevel)
                return false;

            var qp = beastQuest.quirkPoints;
            var cost = controller.quirkCost(quirkId);

            return qp >= cost;
        }

        internal static void BuyQuirk(int quirkId)
        {
            var beastQuest = Plugin.Character.beastQuest;
            var controller = Plugin.Character.beastQuestPerkController;

            var level = beastQuest.quirkLevel[quirkId];
            var maxLevel = controller.maxLevel[quirkId];
            if (level >= maxLevel)
                return;

            var qp = beastQuest.quirkPoints;
            var cost = controller.quirkCost(quirkId);
            if (cost > qp)
                return;

            beastQuest.quirkPoints -= cost;
            beastQuest.quirkLevel[quirkId]++;
            controller.doEffect(quirkId);

            if (level + 1 == maxLevel)
                RemoveQuirk(quirkId);
        }

        internal static void BulkBuyQuirk(int quirkId)
        {
            while (CanBuyQuirk(quirkId))
                BuyQuirk(quirkId);
        }

        internal static void MoveQuirk(int quirkId, bool moveDown)
        {
            var index = Quirks.IndexOf(quirkId);
            if (index == -1)
                return;

            if (moveDown && index < Quirks.Count - 1)
            {
                Quirks.Remove(quirkId);
                Quirks.Insert(index + 1, quirkId);
            }

            else if (!moveDown && index > 0)
            {
                Quirks.Remove(quirkId);
                Quirks.Insert(index - 1, quirkId);
            }
        }

        internal static void RemoveQuirk(int quirkId)
        {
            if (Quirks.Contains(quirkId))
            {
                Quirks.Remove(quirkId);

                var controller = Plugin.Character.beastQuestPerkController;
                controller.changePage(controller.page);
            }
        }

        internal static void ClearQuirks()
        {
            Quirks.Clear();

            var controller = Plugin.Character.beastQuestPerkController;
            controller.changePage(controller.page);
        }


        private static Image _filterCheckmark;
        private static Image _orderCheckmark;

        private static void cloneFilters()
        {
            var topPanel = GameObject.Find("Canvas/Beast Quirks Canvas/Beast Quirks Menu/Top Panel");

            var titleRect = topPanel.transform.Find("Title").GetComponent<RectTransform>();
            titleRect.anchoredPosition += new Vector2(20f, 0);

            var orderRect = topPanel.transform.Find("Order Button").GetComponent<RectTransform>();
            orderRect.anchoredPosition += new Vector2(20f, 0);

            var orig = topPanel.transform.Find("Quirk Filters");
            var rectOrig = orig.GetComponent<RectTransform>();
            rectOrig.anchoredPosition -= new Vector2(20f, 0f);

            var clone = GameObject.Instantiate(orig, orig.transform.parent, false);
            clone.name = "List Options";

            var rectClone = clone.GetComponent<RectTransform>();
            rectClone.anchoredPosition += new Vector2(110f, -1f);

            var textClone = clone.GetComponent<Text>();
            textClone.text = "<b>Quirk List:</b>";
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
            Plugin.Character.beastQuestPerkController.onFilterChange();
        }

        private static void onListOrderClick()
        {
            OrderEnabled = !OrderEnabled;
            Plugin.Character.beastQuestPerkController.onOrderChange();
            Plugin.Character.beastQuestPerkController.updateFilters();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "updateFilters")]
        private static void BeastQuestPerkController_updateFilters_postfix()
        {
            _filterCheckmark.color = FilterEnabled ? Color.white : Color.clear;
            _orderCheckmark.color = OrderEnabled ? Color.white : Color.clear;
        }
    }
}
