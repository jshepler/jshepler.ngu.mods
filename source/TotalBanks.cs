using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TotalBanks
    {
        private enum CurrentButton
        {
            none = 0,
            AdvancedTraining = 1,
            TimeMachine = 2,
            Beards = 3
        }

        private static HoverTooltip _tooltip;
        private static Character _character;
        private static CurrentButton _button = CurrentButton.none;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _tooltip = __instance.tooltip;
            _character = __instance.character;

            __instance.advancedTraining.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(AT_OnPointerEnter)
                .OnPointerExit(AT_OnPointerExit);

            __instance.brokenTimeMachine.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(TM_OnPointerEnter)
                .OnPointerExit(TM_OnPointerExit);

            __instance.beards.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(Beards_OnPointerEnter)
                .OnPointerExit(Beards_OnPointerExit);
        }

        private static void AT_OnPointerEnter(PointerEventData e)
        {
            _button = CurrentButton.AdvancedTraining;
            _character.StartCoroutine(ShowTooltip());
        }

        private static void AT_OnPointerExit(PointerEventData e)
        {
            _button = CurrentButton.none;
        }

        private static void TM_OnPointerEnter(PointerEventData e)
        {
            _button = CurrentButton.TimeMachine;
            _character.StartCoroutine(ShowTooltip());
        }

        private static void TM_OnPointerExit(PointerEventData e)
        {
            _button = CurrentButton.none;
        }

        private static void Beards_OnPointerEnter(PointerEventData e)
        {
            _button = CurrentButton.Beards;
            _character.StartCoroutine(ShowTooltip());
        }

        private static void Beards_OnPointerExit(PointerEventData e)
        {
            _button = CurrentButton.none;
        }

        private static IEnumerator ShowTooltip()
        {
            while (_button != CurrentButton.none)
            {
                var bank = _button switch
                {
                    CurrentButton.AdvancedTraining => _character.adventureController.itopod.totalBankedAdvTraining(),
                    CurrentButton.TimeMachine => _character.adventureController.itopod.totalBankedTimeMachine(),
                    CurrentButton.Beards => _character.adventureController.itopod.totalBankedBeardTemp(),
                    _ => 0
                };

                _tooltip.showTooltip($"<b>Bank:</b> {(bank * 100):#,##0.0}%");
                yield return new WaitForSeconds(1);
            }

            _tooltip.hideTooltip();
        }
    }
}
