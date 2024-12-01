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
                var secondsRemaining = _character.pitController.currentPitTime() - _character.pit.pitTime.totalseconds;
                if (secondsRemaining < 0)
                    secondsRemaining = 0;

                var log10 = (int)Math.Log10(_character.realGold);
                var nextReward = GetNextPitReward(log10);
                var ap = _character.checkAPAdded(log10);

                var text = $"<b>Next Pit Reward in:</b> {NumberOutput.timeOutput(secondsRemaining)}"
                    + $"\n  {nextReward}"
                    + $"\n  and {ap} AP";

                _tooltip.showTooltip(text);
                yield return _wait1;
            }
            
            _tooltip.hideTooltip();
        }

        private static string GetNextPitReward(int log10)
        {
            if (log10 > 30 && _character.wishes.wishes[4].level < 1)
                log10 = 30;

            foreach (var tier in GameData.MoneyPit.TierRewards)
            {
                if (log10 >= tier.Key)
                    continue;

                var tossFactor = _character.pitController.tossFactor();
                var reward = SelectRandomString(tier.Value)
                    .Replace("x?", tossFactor == 1 ? string.Empty : $"x{tossFactor}");

                return reward;
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
