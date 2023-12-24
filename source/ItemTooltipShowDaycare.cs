using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ItemTooltipShowDaycare
    {
        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", typeof(Equipment))]
        private static void ItemController_itemTooltipText_postfix(Equipment item, InventoryController __instance, ref string __result)
        {
            var character = __instance.character;

            var daycare = character.inventory.daycare;
            var dcId = -1;
            for (var x = 0; x < daycare.Count; x++)
                if (daycare[x].id == item.id && daycare[x] != item)
                    dcId = x;

            if (dcId == -1)
                return;

            var dcLevel = daycare[dcId].level + __instance.daycares[dcId].levelsAdded();
            __result += $"\n\n<b>Item level in Daycare:</b> {dcLevel} ({dcLevel + item.level})";
        }
    }
}
