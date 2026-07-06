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
        private static bool _altDown = false;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _tooltip = __instance.tooltip;
            _character = __instance.character;

            __instance.pit.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnPointerExit);

            Plugin.OnUpdate += (o, e) =>
            {
                if (!_showTooltip)
                    return;

                _altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
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

        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static IEnumerator ShowTooltip()
        {
            while (_showTooltip)
            {
                var secondsRemaining = _character.pitController.currentPitTime() - _character.pit.pitTime.totalseconds;
                if (secondsRemaining < 0)
                    secondsRemaining = 0;

                var nextReward = GetNextPitReward(_character.realGold);
                var text = $"<b>Next Pit Reward in:</b> {NumberOutput.timeOutput(secondsRemaining)}"
                    + $"\n  {nextReward.reward}"
                    + $"\n  and {nextReward.ap} AP";

                if (_altDown)
                    text += buildRewardGroups(nextReward.tier);

                _tooltip.showTooltip(text);
                yield return _wait;
            }
            
            _tooltip.hideTooltip();
        }

        private static string buildRewardGroups(int currentTier)
        {
            var line = currentTier switch
            {
                0 => 0,
                1 => 1,
                >= 2 and <= 4 => 2,
                5 => 3,
                6 => 4,
                >= 7 and <= 11 => 5,
                _ => 6
            };

            var format = (int i, string s) => $"\n\n{(line == i ? "<b>" : string.Empty)}{s}{(line == i ? "</b>" : string.Empty)}";

            var rewards = GameData.MoneyPit.TierRewards;
            var text = string.Empty;

            var reward = GetNextPitReward(1e+5);
            text += format(1, $"Tier 1 @{_character.display(1e+5)} gold:\n   {reward.reward}");

            reward = GetNextPitReward(1e+7);
            text += format(2, $"Tier 2-4 @{_character.display(1e+7)} gold:\n   {reward.reward}");

            reward = GetNextPitReward(1e+13);
            text += format(3, $"Tier 5 @{_character.display(1e+13)} gold:\n   {reward.reward}");

            reward = GetNextPitReward(1e+15);
            text += format(4, $"Tier 6 @{_character.display(1e+15)} gold:\n   {reward.reward}");

            reward = GetNextPitReward(1e+18);
            text += format(5, $"Tier 7-11 @{_character.display(1e+18)} gold:\n   {reward.reward}");

            if (_character.wishes.wishes[4].level < 1)
                return text;

            reward = GetNextPitReward(1e+50);
            text += format(6, $"Tier 12-16 @{_character.display(1e+50)} gold:\n   {reward.reward}");

            return text;
        }

        private static (int tier, double gold, long ap, string reward) GetNextPitReward(double gold)
        {
            var log10 = (int)Math.Log10(gold);
            if (log10 > 30 && _character.wishes.wishes[4].level < 1)
                log10 = 30;

            var tier = 0;
            for (var x = 0; x <= 16; x++)
                if (GameData.MoneyPit.TierExponents[x] <= log10)
                    tier = x;

            gold = Math.Pow(10, GameData.MoneyPit.TierExponents[tier]);
            var ap = _character.checkAPAdded(log10);

            if (tier == 0)
                return (tier, gold, ap, null);

            foreach (var rewards in GameData.MoneyPit.TierRewards)
            {
                if (log10 >= rewards.Key)
                    continue;

                // logically, since this is projecting what the next reward will be, you would
                // think need to add 1 to current toss count - to represent the toss # it will be
                // but...
                // tossFactor() already adds 1 to current toss count
                // presumably because toss count isn't incremented until after the reward is awarded
                var tossFactor = _character.pitController.tossFactor();
                var reward = SelectRandomString(rewards.Value)
                    .Replace("x?", tossFactor == 1 ? string.Empty : $"x{tossFactor}");

                return (tier, gold, ap, reward);
            }

            return (tier, gold, ap, null);
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
