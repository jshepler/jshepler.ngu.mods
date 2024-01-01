using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CurrentLoadout
    {
        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "updateLoadoutButtons")]
        private static void InventoryController_updateLoadoutButtons_postfix(InventoryController __instance)
        {
            __instance.loadoutButtons.Do((b, i) => b.image.color = IsLoadoutEquipped(i) ? Color.yellow : Color.white);
        }

        // when dragging an item from inventory to equipped, it doesn't call updateInventory (which calls updateLoadoutButtons)
        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "OnEndDrag")]
        private static void ItemController_OnEndDrag_postfix()
        {
            Plugin.Character.inventoryController.updateLoadoutButtons();
        }

        private static int GetEquippedLoadoutId()
        {
            if (Plugin.Character == null)
                return -1;

            var loadouts = Plugin.Character.inventory.loadouts;
            for (var x = 0; x < loadouts.Count; x++)
            {
                if (IsLoadoutEquipped(x))
                    return x;
            }

            return -1;
        }

        private static bool IsLoadoutEquipped(int loadoutId)
        {
            if (Plugin.Character == null)
                return false;

            var l = Plugin.Character.inventory.loadouts[loadoutId];

            if (l.IsEmpty())
                return false;

            return (l.head == -1 || l.head == -1000)
                && (l.chest == -2 || l.chest == -1000)
                && (l.legs == -3 || l.legs == -1000)
                && (l.boots == -4 || l.boots == -1000)
                && (l.weapon == -5 || l.weapon == -1000)
                && (l.weapon2 == -6 || l.weapon2 == -1000)
                && l.accessories.All(a => a == -1000 || a >= 10000);
        }
    }
}
