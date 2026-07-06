using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ConvertPotions
    {
        private const long MUFFIN_SEEDS_COST = 1000000000L;

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

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseEnergyBarBar1")]
        private static bool ArbitraryController_startUseEnergyBarBar1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.energyBarBar1Count < 5)
                return true;

            character.arbitrary.energyBarBar1Count -= 5;
            character.arbitrary.macGuffinBooster1Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startUseMagicBarBar1")]
        private static bool ArbitraryController_startUseMagicBarBar1_prefix(ArbitraryController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (character.arbitrary.magicBarBar1Count < 5)
                return true;

            character.arbitrary.magicBarBar1Count -= 5;
            character.arbitrary.macGuffinBooster1Count++;
            __instance.character.allArbitrary.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ArbitraryController), "startMacguffinBooster1AP")]
        private static bool ArbitraryController_startMacguffinBooster1AP_prefix(ArbitraryController __instance, UnityAction ___noAction)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var character = __instance.character;
            if (!character.achievements.achievementComplete[145])
            {
                Plugin.ShowOverrideNotification("You haven't even unlocked MacGuffins yet! Don't bother trying to buy this til you do. It's for your own good.", 3f);
                return false;
            }

            if (character.yggdrasil.seeds < MUFFIN_SEEDS_COST)
            {
                Plugin.ShowOverrideNotification($"You do not have {character.display(MUFFIN_SEEDS_COST)} seeds!");
                return false;
            }

            UnityAction yesAction = buyMuffinWithSeeds;
            __instance.box.displayBox($"Are you sure you want to buy MacGuffin Muffin for {character.display(MUFFIN_SEEDS_COST)} seeds?", yesAction, ___noAction);

            return false;
        }

        private static void buyMuffinWithSeeds()
        {
            Plugin.Character.yggdrasil.seeds -= MUFFIN_SEEDS_COST;
            Plugin.Character.arbitrary.macGuffinBooster1Count++;
            Plugin.ShowNotification("You've successfully bought MacGuffin Muffin with seeds!", 2f);
            Plugin.Character.allArbitrary.updateMenu();
        }
    }
}
