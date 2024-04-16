using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Move69Indicator
    {
        private static bool _isReady = false;

        [HarmonyPostfix, HarmonyPatch(typeof(Move69), "Update")]
        private static void Move69_Update_postfix(Move69 __instance, float ___move69Timer)
        {
            _isReady = ___move69Timer > __instance.character.move69Cooldown();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance, Text ___adventureText)
        {
            if (__instance.adventure.IsInteractable() && __instance.character.adventure.move69Used < 69 && _isReady)
            {
                __instance.adventure.image.color = Plugin.ButtonColor_Yellow;
                ___adventureText.text = "Move 69 Ready";
            }
        }
    }
}
