using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    //[HarmonyPatch]
    internal class SellConsumables
    {
        private static int[] _sellable = [1, 3, 5, 6, 26, 27, 30, 43, 60, 61, 78, 79];

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "buyAP")]
        private static bool ArbitraryController_buyAP_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var id = __instance.id;
            if (!_sellable.Contains(id))
                return true;

            var count = __instance.count();
            if (count > 0)
            {
                var cost = __instance.cost();
                if (id == 78)
                    cost /= 25; // black pens

                var a = __instance.character.arbitrary;
                a.curArbitraryPoints += cost;

                _ = id switch
                {
                    1 => a.energyPotion2Count--,
                    3 => a.magicPotion2Count--,
                    5 => a.energyBarBar1Count--,
                    6 => a.magicBarBar1Count--,
                    26 => a.energyPotion3Count--,
                    27 => a.magicPotion3Count--,
                    30 => a.lootCharm2Count--,
                    43 => a.macGuffinBooster1Count--,
                    60 => a.res3Potion2Count--,
                    61 => a.res3Potion3Count--,
                    78 => a.cardTierUpperCount--,
                    79 => a.mayoSpeedPotCount--,
                    _ => 0
                };

                __instance.character.allArbitrary.updateMenu();
            }

            return false;
        }
    }
}
