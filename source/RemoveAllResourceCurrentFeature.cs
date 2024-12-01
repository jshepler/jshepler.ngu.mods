using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RemoveAllResourceCurrentFeature
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "removeMostEnergy")]
        internal static bool Character_removeMostEnergy_prefix(Character __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            switch (__instance.CurrentMenu())
            {
                case Menu.BasicTraining:
                    __instance.allOffenseController.removeAllEnergy();
                    __instance.allDefenseController.removeAllEnergy();
                    return false;

                case Menu.Augments:
                    __instance.augmentsController.removeAllEnergy();
                    return false;

                case Menu.AdvancedTraining:
                    __instance.advancedTrainingController.removeAllEnergy();
                    return false;

                case Menu.TimeMachine:
                    __instance.timeMachineController.removeAllEnergy();
                    return false;

                case Menu.Wandoos:
                    __instance.wandoos98Controller.removeAllEnergy();
                    return false;

                case Menu.NGU_Energy:
                    __instance.NGUController.removeAllEnergy();
                    return false;

                case Menu.Wishes:
                    __instance.wishesController.removeAllEnergy();
                    return false;

                default:
                    return true;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "removeMostMagic")]
        internal static bool Character_removeMostMagic_prefix(Character __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            switch (__instance.CurrentMenu())
            {
                case Menu.TimeMachine:
                    __instance.timeMachineController.removeAllMagic();
                    return false;

                case Menu.BloodMagic_Rituals:
                    __instance.bloodMagicController.removeAllMagic();
                    return false;

                case Menu.Wandoos:
                    __instance.wandoos98Controller.removeAllMagic();
                    return false;

                case Menu.NGU_Magic:
                    __instance.NGUController.removeAllMagic();
                    return false;

                case Menu.Wishes:
                    __instance.wishesController.removeAllMagic();
                    return false;

                default:
                    return true;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "removeAllRes3")]
        internal static bool Character_removeAllRes3_prefix(Character __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            switch (__instance.CurrentMenu())
            {
                case Menu.Hacks:
                    __instance.hacksController.removeAllR3();
                    return false;

                case Menu.Wishes:
                    __instance.wishesController.removeAllRes3();
                    return false;

                default:
                    return true;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "clearSelectedWish")]
        private static bool WishesController_clearSelectedWish_prefix(WishesController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            __instance.removeAllResources();
            __instance.updateText();
            __instance.updateAllPods();

            return false;
        }
    }
}
