using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ConvertPotions
    {
        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseEnergyPotion1")]
        private static bool ArbitraryController_startUseEnergyPotion1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.energyPotion1Count < 24)
                return true;

            character.arbitrary.energyPotion1Count -= 24;
            character.arbitrary.energyPotion3Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseMagicPotion1")]
        private static bool ArbitraryController_startUseMagicPotion1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.magicPotion1Count < 24)
                return true;

            character.arbitrary.magicPotion1Count -= 24;
            character.arbitrary.magicPotion3Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseRes3Potion1")]
        private static bool ArbitraryController_startUseRes3Potion1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.res3Potion1Count < 24)
                return true;

            character.arbitrary.res3Potion1Count -= 24;
            character.arbitrary.res3Potion3Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseLootCharm1")]
        private static bool ArbitraryController_startUseLootCharm1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.lootCharm1Count < 24)
                return true;

            character.arbitrary.lootCharm1Count -= 24;
            character.arbitrary.lootCharm2Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }
    }
}
