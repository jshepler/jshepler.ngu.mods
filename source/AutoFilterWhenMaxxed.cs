using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoFilterWhenMaxxed
    {
        private static int[] _ignoreItemIds =
        {
            53, 76, 94, 142, 170, 229, 295, 388, 430,   // pendant ids except last one
            67, 128, 169, 230, 296, 389, 431,           // looty ids, except last one
            120, 154                                    // flubber, walderp's cane
        };

        [HarmonyPostfix, HarmonyPatch(typeof(Equipment), "mergeItem"), HarmonyPatch(typeof(Equipment), "levelUp")]
        private static void InventoryController_levelUp_postfix(Equipment __instance)
        {
            if (!Plugin.Character.arbitrary.lootFilter
                || __instance.level < 100
                || !__instance.isEquipment()
                || _ignoreItemIds.Contains(__instance.id)
                ) return;

            Plugin.Character.inventory.itemList.itemFiltered[__instance.id] = true;
        }
    }
}
