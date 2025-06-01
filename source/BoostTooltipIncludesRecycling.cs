using System;
using System.Collections.Generic;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    // merged into ImprovedItemTooltip.cs but keeping this class for now, jic
    //[HarmonyPatch]
    internal class BoostTooltipIncludesRecycling
    {
        private static List<int> _boosts = [1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000];

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", typeof(Equipment))]
        private static void InventoryController_itemTooltipText_postfix(Equipment item, InventoryController __instance, ref string __result)
        {
            if (!item.isBoost())
                return;

            var equip = __instance.itemInfo.genLoot(item.id, true);
            var boostValue = equip.type switch
            {
                part.atkBoost => equip.capAttack,
                part.defBoost => equip.capDefense,
                part.specBoost => equip.spec1Cap,
                _ => 1
            };

            var boostBonus = __instance.character.allItemList.boostBonus();
            var probability = 1f;

            // totalRecycleBonus() has a bug that could result in a chance > 100%
            // since we're going to use it in probability math, cap it to 100%
            // instead of doing the correct calc, going to use the buggy method because it's what the game uses when doing recycling
            var recycleChance = Math.Min(1f, Plugin.Character.totalRecycleBonus());

            var avgBoostWithRecycling = 0f;
            var startIndex = (item.id - 1) % 13;
            for (var x = startIndex; x >= 0; x--)
            {
                avgBoostWithRecycling += probability * _boosts[x] * boostBonus;
                probability *= recycleChance;
            }

            var cubeBoost = boostValue * boostBonus / InfinityCubeSoftCap.CubeBoostDivider;
            var cubeBoostRecycling = avgBoostWithRecycling / InfinityCubeSoftCap.CubeBoostDivider;
            var avg = (recycleChance > 0 && recycleChance < 1) ? " (avg)" : string.Empty;

            __result += $"\n     <b>To Cube:</b> {cubeBoost:#,##0.##}"
                + $"\n\n<b> ... with Boost Recycling ({recycleChance * 100f:0.#}%):</b> {avgBoostWithRecycling:#,##0.##}{avg}"
                + $"\n     <b>To Cube:</b> {cubeBoostRecycling:#,##0.##}{avg}";
        }
    }
}
