using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareSwapItems
    {
        [HarmonyPrefix, HarmonyPatch(typeof(DaycareItemController), "endDragAction")]
        private static bool endDragAction(DaycareItemController __instance)
        {
            var character = __instance.character;
            var item1 = character.inventory.item1;
            var item2 = character.inventory.item2;
            var dcId1 = __instance.daycareID(item1);
            var dcId2 = __instance.daycareID(item2);

            if (dcId1 == dcId2 || dcId1 < 0 || dcId2 < 0)
                return true;

            // modeled on Inventory.swapDaycareWithItem()
            character.inventory.markLoadoutIDSwap(item1, item2);

            var dc = character.inventory.daycare;
            var tmpEq = dc[dcId1];
            dc[dcId1] = dc[dcId2];
            dc[dcId2] = tmpEq;

            // modeled on InventoryController.swapDaycare()
            character.inventoryController.daycares[dcId1].updateItem();
            character.inventoryController.daycares[dcId2].updateItem();

            // instead of resetting the timers, swap them
            var dct = character.inventory.daycareTimers;
            var tmpTime = dct[dcId1].totalseconds;
            dct[dcId1].setTime(dct[dcId2].totalseconds);
            dct[dcId2].setTime(tmpTime);

            return false;
        }
    }
}
