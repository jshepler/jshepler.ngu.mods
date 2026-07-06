using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HighlightItemsInLoadouts
    {
        private static bool _highlight = false;

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "Start")]
        private static void InventoryController_Start_postfix(InventoryController __instance)
        {
            var button = __instance.loadoutTabButton;

            button.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    _highlight = !_highlight;
                    button.image.color = _highlight ? Plugin.ButtonColor_Green : Color.white;
                    Plugin.Character.inventoryController.updateInventory();
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "updateItem")]
        private static void ItemController_updateItem_postfix(ItemController __instance)
        {
            if (__instance.id >= __instance.character.inventory.inventory.Count)
                return;

            __instance.image.color = (_highlight && InAnyLoadout(__instance.id)) ? Plugin.ButtonColor_Green : Color.white;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutController), "updateItem")]
        private static void LoadoutController_updateItem_postfix(LoadoutController __instance)
        {
            if (__instance.id >= 100000)
                return;

            __instance.image.color = (_highlight && InAnyLoadout(__instance.id)) ? Plugin.ButtonColor_Green : Color.white;
        }

        private static bool InAnyLoadout(int slotId)
        {
            foreach (var loadout in Plugin.Character.inventory.loadouts)
            {
                if (loadout.head == slotId)
                    return true;

                if (loadout.chest == slotId)
                    return true;

                if (loadout.legs == slotId)
                    return true;

                if (loadout.boots == slotId)
                    return true;

                if (loadout.weapon == slotId)
                    return true;

                if (loadout.weapon2 == slotId)
                    return true;

                if (loadout.accessories.Any(i => i == slotId))
                    return true;
            }

            return false;
        }
    }
}
