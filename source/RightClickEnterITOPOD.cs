using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RightClickEnterITOPOD
    {
        private static Button _optimalFloorButton;
        private static Button _enterPodButton;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void AdventureController_Start_postfix(AdventureController __instance)
        {
            _optimalFloorButton = GameObject.Find("Canvas/Adventure Menu Canvas/Adventure Menu/ITOPOD Panel/\"Optimal\" Floor").GetComponent<Button>();
            _enterPodButton = GameObject.Find("Canvas/Adventure Menu Canvas/Adventure Menu/ITOPOD Panel/ENTER button").GetComponent<Button>();

            __instance.enterItopodButton.gameObject
                .AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if(__instance.character.arbitrary.lazyITOPODOn)
                        _optimalFloorButton.onClick.Invoke();

                    _enterPodButton.onClick.Invoke();
                });
        }
    }
}
