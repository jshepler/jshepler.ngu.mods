using System.Linq;
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
            var character = Plugin.Character;

            if (!character.arbitrary.lootFilter
                || __instance.level < 100
                || !__instance.isEquipment()
                || _ignoreItemIds.Contains(__instance.id)

                // ignore gerbil in sad for those that want to farm it to red border grey liquid
                || (__instance.id == 195 && character.settings.rebirthDifficulty == difficulty.sadistic))

                return;

            character.inventory.itemList.itemFiltered[__instance.id] = true;
        }
    }
}
