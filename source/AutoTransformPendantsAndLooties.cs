using HarmonyLib;
using System.Linq;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoTransformPendantsAndLooties
    {
        private static readonly int[] _pendantIds = { 53, 76, 94, 142, 170, 229, 295, 388, 430, 504 };
        private static readonly int[] _lootyIds = { 67, 128, 169, 230, 296, 389, 431, 505 };
        private static readonly int[] _flubberIds = { 120, 121 };
        private static int[] _allIds = _pendantIds.Concat(_lootyIds).Concat(_flubberIds).ToArray();

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "autoMerge")]
        private static void InventoryController_autoMerge_postifx(InventoryController __instance)
        {
            var numberOfInventorySlots = __instance.curSpaces();
            var inventory = __instance.character.inventory;
            var equipmentList = inventory.inventory;

            if (numberOfInventorySlots > equipmentList.Count)
            {
                // should never happen, but just in case...
                Plugin.LogInfo($"number of inventory slots > equipment list count: {numberOfInventorySlots} > {equipmentList.Count}");
            }

            //AutoSaves.DoSave(__instance.character, $"AutoTransform - {DateTime.UtcNow:yyyyMMdd_HHmmss}");

            var doAnotherPass = true;
            while (doAnotherPass)
            {
                doAnotherPass = false;
                for (var slotIndex = 0; slotIndex < numberOfInventorySlots; slotIndex++)
                {
                    if (slotIndex >= equipmentList.Count)
                    {
                        break;
                    }

                    var item = equipmentList[slotIndex];
                    if(item == null || !item.removable) continue;

                    if (_allIds.Contains(item.id))
                    {
                        __instance.mergeAll(slotIndex);
                        item = equipmentList[slotIndex]; // mergeAll replaces item instance

                        // sanity check
                        if (item == null)
                        {
                            Plugin.LogInfo($"item became null after merge - skipping");
                            continue;
                        }

                        // from ItemController.consumeItem, modified because ItemController is the visible slot regardless of page,
                        // if the item to transform isn't on the current page, using ItemController.consumeItem won't work right
                        var transformItemId = __instance.checkItemTransform(item);
                        if (transformItemId > 0)
                        {
                            inventory.deleteItem(slotIndex);
                            __instance.itemInfo.makeLoot(transformItemId, slotIndex);
                            __instance.updateItem(slotIndex);

                            doAnotherPass = true;
                        }
                    }
                }
            }

            __instance.updateInventory();
        }
    }
}
