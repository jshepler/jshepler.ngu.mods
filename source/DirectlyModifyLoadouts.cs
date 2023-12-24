using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DirectlyModifyLoadouts
    {
        private const int EmptyInventorySlotId = -1000;

        private static MultiLoadoutController _multiLoadoutController;
        private static LoadoutDisplayController _slotBeingAssigned;
        private static int[] _acc;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            _acc = new int[16];
            for (var x = 0; x < 16; x++) _acc[x] = x + 10000;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutDisplayController), "Start")]
        private static void LoadoutDisplayController_Start_postfix(LoadoutDisplayController __instance)
        {
            _multiLoadoutController = __instance.inventoryController.loadoutsController;
            
            var handler = __instance.gameObject.AddComponent<ClickHandlerComponent>();
            
            handler.OnRightClick(e =>
            {
                __instance.SetInventorySlotId(EmptyInventorySlotId);
                __instance.updateItem();
            });

            handler.OnLeftClick(e =>
            {
                if (_slotBeingAssigned != null) return;

                _slotBeingAssigned = __instance;
                _multiLoadoutController.hidePanel();
            });

            Plugin.OnUpdate += (o, e) =>
            {
                if (_slotBeingAssigned == null || !Input.GetKeyDown(KeyCode.Escape))
                    return;

                _slotBeingAssigned = null;
                _multiLoadoutController.showPanel();
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemController), "OnPointerClick")]
        private static bool ItemController_OnPointerClick_prefix(PointerEventData eventData, ItemController __instance)
        {
            if (_slotBeingAssigned != null && eventData.button == PointerEventData.InputButton.Left)
            {
                var item = __instance.character.inventory.inventory[__instance.id];
                if (item.type == _slotBeingAssigned.GetEqupmentType())
                {
                    _slotBeingAssigned.SetInventorySlotId(__instance.id);
                    _slotBeingAssigned.updateItem();
                    
                    _slotBeingAssigned = null;
                    _multiLoadoutController.showPanel();
                    
                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutController), "OnPointerClick")]
        private static bool LoadoutController_OnPointerClick_prefix(PointerEventData eventData, LoadoutController __instance)
        {
            if (_slotBeingAssigned == null || eventData.button != PointerEventData.InputButton.Left) return true;

            if (__instance.id == _slotBeingAssigned.id
                // or one accessory slot to a different accessory slot
                || (_acc.Contains(__instance.id) && _acc.Contains(_slotBeingAssigned.id)))
            {
                _slotBeingAssigned.SetInventorySlotId(__instance.id);
                _slotBeingAssigned.updateItem();

                _slotBeingAssigned = null;
                _multiLoadoutController.showPanel();

                return false;
            }

            return true;
        }
    }
}
