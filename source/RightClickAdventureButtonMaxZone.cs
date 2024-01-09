using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RightClickAdventureButtonMaxZone
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            __instance.adventure.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    Plugin.Character.adventureController.zoneForward.goToMaxZone();
                    Plugin.Character.StartCoroutine(FlashButton(__instance.adventure));
                });
        }

        private static WaitForSeconds _wait = new WaitForSeconds(0.05f);
        private static IEnumerator FlashButton(Button b)
        {
            for (var x = 0; x < 3; x++)
            {
                yield return _wait;
                b.image.color = Plugin.ButtonColor_Green;
                yield return _wait;
                b.image.color = Color.white;
            }
        }
    }
}
