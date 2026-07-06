using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InventoryCount
    {
        private static bool _updateCounters = false;

        private static Text _inventoryLabelText;
        private static Text _inventoryButtonText;
        private static Button _inventoryButton;

        private static List<Equipment> _inventory => Plugin.Character.inventory.inventory;
        private static bool _inventoryEnabled => Plugin.Character.settings.inventoryOn;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance, Text ___inventoryText)
        {
            _inventoryLabelText = GameObject.Find("Canvas/Inventory Menu Canvas/Inventory Menu/Inventory/Equipment Text (3)").GetComponent<Text>();
            _inventoryLabelText.alignment = TextAnchor.MiddleCenter;
            _inventoryLabelText.resizeTextForBestFit = true;

            _inventoryButton = __instance.inventory;
            _inventoryButtonText = ___inventoryText;

            Plugin.OnLateUpdate += OnLateUpdate;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "updateItem")]
        private static void ItemController_updateItem_postfix()
        {
            _updateCounters = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix()
        {
            updateCounters();
        }

        private static void OnLateUpdate(object sender, EventArgs e)
        {
            if (!_updateCounters)
                return;

            updateCounters();
            _updateCounters = false;
        }

        private static void updateCounters()
        {
            if (Plugin.Character == null)
                return;

            var count = _inventory.Count(i => i != null && i.id > 0);
            _inventoryLabelText.text = $"inventory ({count} / {_inventory.Count})";

            var mergeSlots = Plugin.Character.inventoryController.totalInvMergeSlots();
            var countIgnoringMergeSlots = _inventory.Skip(mergeSlots).Count(i => i != null && i.id > 0);
            var open = _inventory.Count - mergeSlots - countIgnoringMergeSlots;
            _inventoryButtonText.text = _inventoryEnabled ? $"Inventory ({open})" : "Really Locked";

            _inventoryButton.image.color = !_inventoryEnabled ? Color.white
                : open switch
                {
                    0 => Plugin.ButtonColor_Red,
                    <= 5 => Plugin.ButtonColor_Yellow,
                    _ => Color.white
                };
        }
    }
}
