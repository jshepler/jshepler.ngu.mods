using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using jshepler.ngu.mods.Popups;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PerkList
    {
        private static List<int> _perkIds => Data.PerkList;
        private static int[] _fibPerkBonusLevels = [1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597];
        private static PerkListPopup _popup = new PerkListPopup(canBuyPerk, buyPerk, bulkBuyPerk, movePerk, removePerk);

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkUIController), "OnPointerClick")]
        private static bool ItopodPerkUIController_OnPointerClick_prefix(PointerEventData eventData, ItopodPerkUIController __instance)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !Plugin.ShiftIsDown)
                return true;

            var perkId = __instance.id;
            if (_perkIds.Contains(perkId))
                _perkIds.Remove(perkId);
            else
                _perkIds.Add(perkId);

            __instance.updateGraphic();

            return false;
        }

        private static Func<int, bool> isMaxed = (int perkId) => Plugin.Character.adventure.itopod.perkLevel[perkId] >= Plugin.Character.adventureController.itopod.maxLevel[perkId];

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "doEffect")]
        private static void ItopodPerkController_doEffect_postfix(int id)
        {
            if (_perkIds.Contains(id) && isMaxed(id))
                removePerk(id);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkUIController), "updateGraphic")]
        private static void ItopodPerkUIController_updateGraphic_postfix(ItopodPerkUIController __instance)
        {
            if (_perkIds.Contains(__instance.id))
                __instance.itemBorder.color = new Color(0, 1, 1);
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (Plugin.Character == null || !Plugin.Character.InMenu(Menu.Perks) || !Input.GetKeyDown(KeyCode.F1))
                return;

            if (_popup.IsOpen)
                _popup.Close();
            else
                _popup.Open(_perkIds);
        }

        private static bool canBuyPerk(int perkId)
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

        private static void buyPerk(int perkId)
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
                removePerk(perkId);
        }

        private static void bulkBuyPerk(int perkId)
        {
            while (canBuyPerk(perkId))
                buyPerk(perkId);
        }

        private static void movePerk(int perkId, bool moveDown)
        {
            var index = _perkIds.IndexOf(perkId);
            if (index == -1)
                return;

            if (moveDown && index < _perkIds.Count - 1)
            {
                _perkIds.Remove(perkId);
                _perkIds.Insert(index + 1, perkId);
            }

            else if (!moveDown && index > 0)
            {
                _perkIds.Remove(perkId);
                _perkIds.Insert(index - 1, perkId);
            }
        }

        private static void removePerk(int perkId)
        {
            if (_perkIds.Contains(perkId))
            {
                _perkIds.Remove(perkId);

                var controller = Plugin.Character.adventureController.itopod;
                controller.changePage(controller.page);
            }
        }
    }
}
