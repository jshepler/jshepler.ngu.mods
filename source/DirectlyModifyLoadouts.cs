using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DirectlyModifyLoadouts
    {
        private const int EMPTY_SLOT_ID = -1000;

        private static MultiLoadoutController _multiLoadoutController;
        private static LoadoutDisplayController _slotBeingAssigned;
        private static int[] _acc;

        private static Button _loadoutTabButton;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            _acc = new int[16];
            for (var x = 0; x < 16; x++)
                _acc[x] = x + 10000;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutDisplayController), "Start")]
        private static void LoadoutDisplayController_Start_postfix(LoadoutDisplayController __instance)
        {
            _multiLoadoutController = __instance.inventoryController.loadoutsController;
            _loadoutTabButton = __instance.inventoryController.loadoutTabButton;

            var handler = __instance.gameObject.AddComponent<ClickHandlerComponent>();
            
            handler.OnRightClick(e =>
            {
                __instance.SetInventorySlotId(EMPTY_SLOT_ID);
                __instance.updateItem();
            });

            handler.OnLeftClick(e =>
            {
                if (_slotBeingAssigned != null)
                    return;

                _slotBeingAssigned = __instance;
                _multiLoadoutController.hidePanel();
                UpdateLoadoutsButton();
            });

            Plugin.OnUpdate += (o, e) =>
            {
                if (_slotBeingAssigned == null || !Input.GetKeyDown(KeyCode.Escape))
                    return;

                _slotBeingAssigned = null;
                _multiLoadoutController.showPanel();
                UpdateLoadoutsButton();
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
                    RemoveDupes(_slotBeingAssigned.loadoutID, __instance.id);
                    _slotBeingAssigned.SetInventorySlotId(__instance.id);
                    _slotBeingAssigned.updateItem();
                    
                    _slotBeingAssigned = null;
                    _multiLoadoutController.showPanel();
                    UpdateLoadoutsButton();


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
                RemoveDupes(_slotBeingAssigned.loadoutID, __instance.id);
                _slotBeingAssigned.SetInventorySlotId(__instance.id);
                _slotBeingAssigned.updateItem();

                _slotBeingAssigned = null;
                _multiLoadoutController.showPanel();
                UpdateLoadoutsButton();

                return false;
            }

            return true;
        }

        private static void UpdateLoadoutsButton()
        {
            _loadoutTabButton.interactable = (_slotBeingAssigned == null);
            _loadoutTabButton.image.color = (_slotBeingAssigned == null) ? Color.white : Color.red;
        }

        private static void RemoveDupes(int loadoutId, int dupeSlotId)
        {
            var loadout = Plugin.Character.inventory.loadouts[loadoutId];

            for (var loadoutSlotId = -6; loadoutSlotId < 0; loadoutSlotId++)
                if (loadout.GetInventorySlotId(loadoutSlotId) == dupeSlotId)
                    loadout.SetInventorySlotId(loadoutSlotId, EMPTY_SLOT_ID);

            for (var loadoutSlotId = 10000; loadoutSlotId < loadout.accessories.Count + 10000; loadoutSlotId++)
                if (loadout.GetInventorySlotId(loadoutSlotId) == dupeSlotId)
                    loadout.SetInventorySlotId(loadoutSlotId, EMPTY_SLOT_ID);
        }
    }
}
