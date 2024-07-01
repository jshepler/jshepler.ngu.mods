using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BoostTooltipIncludesRecycling
    {
        private static int[] _stopIds = [1, 14, 27];

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", typeof(Equipment))]
        private static void InventoryController_itemTooltipText_postfix(Equipment item, InventoryController __instance, ref string __result)
        {
            if (!item.isBoost() || !__instance.character.settings.autoboostRecycledBoosts) return;

            var equip = __instance.itemInfo.genLoot(item.id, true);
            var boostBonus = __instance.character.allItemList.boostBonus();
            var cubeBoostDivider = InfinityCubeSoftCap.CubeBoostDivider;
            var totalBoost = 0f;
            var totalCubeBoost = 0f;

            var boostValue = equip.type switch
            {
                part.atkBoost => equip.capAttack,
                part.defBoost => equip.capDefense,
                part.specBoost => equip.spec1Cap,
                _ => 1
            };
            var cubeBoost = boostValue * boostBonus / cubeBoostDivider;

            while (true)
            {
                var boost = boostBonus * (equip.type switch
                {
                    part.atkBoost => equip.capAttack,
                    part.defBoost => equip.capDefense,
                    part.specBoost => equip.spec1Cap,
                    _ => 1
                });

                totalBoost += boost;
                totalCubeBoost += boost / cubeBoostDivider;

                if (_stopIds.Contains(equip.id))
                    break;

                equip = __instance.itemInfo.genLoot(equip.id - 1, true);
            }

            __result += $"\n     <b>To Cube:</b> {cubeBoost:#,##0.##}"
                + $"\n\n<b> ... with Boost Recycling:</b> {totalBoost:#,##0.##}"
                + $"\n     <b>To Cube:</b> {totalCubeBoost:#,##0.##}";
        }
    }
}
