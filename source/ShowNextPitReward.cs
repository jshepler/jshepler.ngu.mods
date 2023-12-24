using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShowNextPitReward
    {
        private static HoverTooltip _tooltip;
        private static Character _character;
        private static bool _showTooltip = false;

        private static WaitForSeconds _wait1 = new WaitForSeconds(1);

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _tooltip = __instance.tooltip;
            _character = __instance.character;

            __instance.pit.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnPointerExit);
        }

        private static void OnPointerEnter(PointerEventData e)
        {
            _showTooltip = true;
            _character.StartCoroutine(ShowTooltip());
        }

        private static void OnPointerExit(PointerEventData e)
        {
            _showTooltip = false;
        }

        private static IEnumerator ShowTooltip()
        {
            while (_showTooltip)
            {
                var nextReward = GetNextPitReward();
                var secondsRemaining = _character.pitController.currentPitTime() - _character.pit.pitTime.totalseconds;
                if (secondsRemaining < 0)
                    secondsRemaining = 0;

                _tooltip.showTooltip($"<b>Next Pit Reward in:</b> {NumberOutput.timeOutput(secondsRemaining)}\n  {nextReward}");
                yield return _wait1;
            }
            
            _tooltip.hideTooltip();
        }

        private static string GetNextPitReward()
        {
            var log = (int)Math.Log10(_character.realGold);
            if (log > 30 && _character.wishes.wishes[4].level < 1)
                log = 30;

            foreach (var tier in GameData.PitTierRewards)
            {
                if (log < tier.Key)
                    return SelectRandomString(tier.Value);
            }

            return null;
        }

        private static string SelectRandomString(List<string> strings)
        {
            var currentState = UnityEngine.Random.state;
            UnityEngine.Random.state = _character.pit.pitState;

            var selection = UnityEngine.Random.Range(1, strings.Count + 1);
            UnityEngine.Random.state = currentState;

            return strings[selection - 1];
        }
    }
}
