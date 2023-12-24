using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoFilterWhenMaxxed
    {
        private static Character _character;
        private static int[] _ignoreItemIds =
        {
            53, 76, 94, 142, 170, 229, 295, 388, 430,   // pendant ids except last one
            67, 128, 169, 230, 296, 389, 431,           // looty ids, except last one
            120, 154                                    // flubber, walderp's cane
        };

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null) return;
            
            Plugin.OnSaveLoaded += (o, e) => _character = e.Character;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Equipment), "mergeItem"), HarmonyPatch(typeof(Equipment), "levelUp")]
        private static void InventoryController_levelUp_postfix(Equipment __instance)
        {
            if (!_character.arbitrary.lootFilter
                || __instance.level < 100
                || !__instance.isEquipment()
                || _ignoreItemIds.Contains(__instance.id)
                ) return;

            _character.inventory.itemList.itemFiltered[__instance.id] = true;
        }
    }
}
