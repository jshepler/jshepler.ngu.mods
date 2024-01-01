using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InventoryCount
    {
        private static bool _update = false;
        private static Text _text;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            _text = GameObject.Find("Canvas/Inventory Menu Canvas/Inventory Menu/Inventory/Equipment Text (3)").GetComponent<Text>();
            _text.alignment = TextAnchor.MiddleCenter;
            _text.resizeTextForBestFit = true;

            Plugin.OnLateUpdate += OnLateUpdate;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "updateItem")]
        private static void ItemController_updateItem_postfix()
        {
            _update = true;
        }

        private static void OnLateUpdate(object sender, EventArgs e)
        {
            if (!_update)
                return;

            var count = Plugin.Character.inventory.inventory.Count(i => i != null && i.id > 0);
            _text.text = $"inventory ({count} / {Plugin.Character.inventory.inventory.Count})";

            _update = false;
        }
    }
}
