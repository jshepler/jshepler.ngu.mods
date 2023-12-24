using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BoostTooltipIncludesRecycling
    {
        private static int[] _stopIds = new[] { 1, 14, 27 };

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", typeof(Equipment))]
        private static void InventoryController_itemTooltipText_postfix(Equipment item, InventoryController __instance, ref string __result)
        {
            if (!item.isBoost() || !__instance.character.settings.autoboostRecycledBoosts) return;

            var equip = __instance.itemInfo.genLoot(item.id, true);
            var boostBonus = __instance.character.allItemList.boostBonus();
            var totalBoost = 0f;

            while (true)
            {
                totalBoost += boostBonus * (equip.type switch
                {
                    part.atkBoost => equip.capAttack,
                    part.defBoost => equip.capDefense,
                    part.specBoost => equip.spec1Cap,
                    _ => 1
                });

                if (_stopIds.Contains(equip.id)) break;

                equip = __instance.itemInfo.genLoot(equip.id - 1, true);
            }

            __result += $"\n<b> ... with Boost Recycling:</b> {totalBoost:#,##0.##}";
        }
    }
}
