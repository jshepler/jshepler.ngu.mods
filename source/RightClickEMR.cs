using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RightClickEMR
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Energy), "Start")]
        private static void Energy_Start_prostfix(Energy __instance)
        {
            var character = __instance.character;

            __instance.energyBar.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        RemoveAllResourceCurrentFeature.Character_removeMostEnergy_prefix(character);

                    else
                        Plugin.Character.removeMostEnergy();

                    character.refreshMenus();
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MagicDisplay), "Start")]
        private static void MagicDisplay_Start_prostfix(MagicDisplay __instance)
        {
            var character = __instance.character;

            __instance.magicBar.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        RemoveAllResourceCurrentFeature.Character_removeMostMagic_prefix(character);

                    else
                        Plugin.Character.removeMostMagic();

                    character.refreshMenus();
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Resource3Display), "Start")]
        private static void Resource3Display_Start_prostfix(Resource3Display __instance)
        {
            var character = __instance.character;

            __instance.res3Bar.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        RemoveAllResourceCurrentFeature.Character_removeAllRes3_prefix(character);

                    else
                        Plugin.Character.removeAllRes3();

                    character.refreshMenus();
                });
        }
    }
}
