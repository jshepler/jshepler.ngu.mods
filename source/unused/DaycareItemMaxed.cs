using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareItemMaxed
    {
        private static Button _dkButton;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static int[] _lastLevelsAdded = new int[6];
        private static bool _saveLoaded = false;

        [HarmonyPostfix, HarmonyPatch(typeof(AllDaycareController), "Start")]
        private static void AllDaycareController_Start_postfix(AllDaycareController __instance)
        {
            _dkButton = __instance.button;
            Plugin.BeginCoroutine(watchForMaxedItem());

            Plugin.OnOfflineProgressionComplete += afterFirstLoad;
        }

        private static void afterFirstLoad(object sender, EventArgs e)
        {
            _saveLoaded = true;
            Plugin.OnOfflineProgressionComplete -= afterFirstLoad;
        }

        private static IEnumerator watchForMaxedItem()
        {
            var character = Plugin.Character;
            var daycareControllers = character.inventoryController.daycares;

            var maxedItems =
                from c in daycareControllers
                where c.id < character.inventory.daycare.Count
                let i = character.inventory.daycare[c.id]
                where i.id > 0 && !i.isMacGuffin() && i.level + c.levelsAdded() >= 100
                select i;

            while (true)
            {
                yield return _wait;
                if (!_saveLoaded)
                    continue;

                if (character.InMenu(Menu.Inventory))
                    _dkButton.image.color = maxedItems.Any() ? Plugin.ButtonColor_Green : Color.white;

                // want to trigger an update of inventory items for the green border whenever
                // a daycare item "gains" a level - this tracks levelsAdded() per item to
                // notice when they change and trigger an update
                var doUpdate = false;
                for (var x = 0; x < daycareControllers.Count; x++)
                {
                    var controller = daycareControllers[x];
                    if (controller.id >= character.inventory.daycare.Count)
                    {
                        _lastLevelsAdded[x] = 0;
                        continue;
                    }

                    var levelsAdded = controller.levelsAdded();
                    if (levelsAdded != _lastLevelsAdded[x])
                    {
                        _lastLevelsAdded[x] = levelsAdded;
                        doUpdate = true;
                    }
                }

                if (doUpdate)
                    character.inventoryController.updateInventory();
            }
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(ItemController), "updateItem"),
            HarmonyPatch(typeof(LoadoutController), "updateItem")]
        private static void ItemController_updateItem_postfix(int ___id, Image ___border)
        {
            var character = Plugin.Character;
            if (character == null)
                return;

            var item = character.inventory.GetItem(___id);
            if (item == null || !item.isEquipment() || item.level >= 100)
                return;

            var dcSlotId = character.inventory.daycare.FindIndex(e => e.id == item.id);
            if (dcSlotId == -1)
                return;

            var levelsAdded = character.inventoryController.daycares[dcSlotId].levelsAdded();
            var dcLevel = character.inventory.daycare[dcSlotId].level + levelsAdded;
            if (dcLevel + item.level < 99)
                return;

            ___border.color = Color.green;
        }
    }
}
