using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using jshepler.ngu.mods.Popups;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuirkList
    {
        private static List<int> _quirkIds => Data.QuirkList;
        private static QuirkListPopup _popup = new QuirkListPopup(canBuyQuirk, buyQuirk, bulkBuyQuirk, moveQuirk, removeQuirk);

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestPerkUIController), "OnPointerClick")]
        private static bool BeastQuestPerkUIController_OnPointerClick_prefix(PointerEventData eventData, BeastQuestPerkUIController __instance)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !Plugin.ShiftIsDown)
                return true;

            var quirkId = __instance.id;
            if (_quirkIds.Contains(quirkId))
                _quirkIds.Remove(quirkId);
            else
                _quirkIds.Add(quirkId);

            __instance.updateGraphic();

            return false;
        }

        private static Func<int, bool> isMaxed = (int quirkId) => Plugin.Character.beastQuest.quirkLevel[quirkId] >= Plugin.Character.beastQuestPerkController.maxLevel[quirkId];

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "doEffect")]
        private static void BeastQuestPerkController_doEffect_postfix(int id)
        {
            if (_quirkIds.Contains(id) && isMaxed(id))
                removeQuirk(id);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkUIController), "updateGraphic")]
        private static void BeastQuestPerkUIController_updateGraphic_postfix(BeastQuestPerkUIController __instance)
        {
            if (_quirkIds.Contains(__instance.id))
                __instance.itemBorder.color = new Color(0, 1, 1);
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (Plugin.Character == null || !Plugin.Character.InMenu(Menu.Quirks) || !Input.GetKeyDown(KeyCode.F1))
                return;

            if (_popup.IsOpen)
                _popup.Close();
            else
                _popup.Open(_quirkIds);
        }

        private static bool canBuyQuirk(int quirkId)
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

        private static void buyQuirk(int quirkId)
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
                removeQuirk(quirkId);
        }

        private static void bulkBuyQuirk(int quirkId)
        {
            while (canBuyQuirk(quirkId))
                buyQuirk(quirkId);
        }

        private static void moveQuirk(int quirkId, bool moveDown)
        {
            var index = _quirkIds.IndexOf(quirkId);
            if (index == -1)
                return;

            if (moveDown && index < _quirkIds.Count - 1)
            {
                _quirkIds.Remove(quirkId);
                _quirkIds.Insert(index + 1, quirkId);
            }

            else if (!moveDown && index > 0)
            {
                _quirkIds.Remove(quirkId);
                _quirkIds.Insert(index - 1, quirkId);
            }
        }

        private static void removeQuirk(int quirkId)
        {
            if (_quirkIds.Contains(quirkId))
            {
                _quirkIds.Remove(quirkId);

                var controller = Plugin.Character.beastQuestPerkController;
                controller.changePage(controller.page);
            }
        }
    }
}
