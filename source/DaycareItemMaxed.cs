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
        //private static int[] _lastLevelsAdded = new int[6];

        [HarmonyPostfix, HarmonyPatch(typeof(AllDaycareController), "Start")]
        private static void AllDaycareController_Start_postfix(AllDaycareController __instance)
        {
            _dkButton = __instance.button;
            Plugin.BeginCoroutine(watchForMaxedItem());
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
                if (character.InMenu(Menu.Inventory))
                    _dkButton.image.color = maxedItems.Any() ? Plugin.ButtonColor_Green : Color.white;

                //var doUpdate = false;
                //for (var x = 0; x < daycareControllers.Count; x++)
                //{
                //    Plugin.LogInfo($"x:{x}");
                //    var levelsAdded = daycareControllers[x].levelsAdded();
                //    if (levelsAdded != _lastLevelsAdded[x])
                //    {
                //        _lastLevelsAdded[x] = levelsAdded;
                //        doUpdate = true;
                //    }
                //}

                //if (doUpdate)
                //    character.inventoryController.updateInventory();

                yield return _wait;
            }
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(ItemController), "updateItem")]
        private static void ItemController_updateItem_postfix(ItemController __instance)
        {
            var inventory = __instance.character.inventory;

            var slotId = __instance.id;
            if (slotId >= inventory.inventory.Count)
                return;

            var item = inventory.GetItem(slotId);
            if (!item.isEquipment())
                return;

            var dcSlotId = inventory.daycare.FindIndex(e => e.id == item.id);
            if (dcSlotId == -1)
                return;

            var controller = Plugin.Character.inventoryController.daycares[dcSlotId];
            var dcLevel = inventory.daycare[dcSlotId].level + controller.levelsAdded();
            if (dcLevel + item.level < 99)
                return;

            __instance.border.color = Plugin.ButtonColor_Green;
        }
    }
}
