using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class TwentyFourHour : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.Hour24];

        [HarmonyPrefix, HarmonyPatch(typeof(Hour24ChallengeController), "showchallengeInfo")]
        private static bool showChallengeInfo_prefix(Hour24ChallengeController __instance, Text ___challengeInfo)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfo.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.Hour24];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfo.text = $"<b>24 Hour Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfo.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Hour24ChallengeController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(Hour24ChallengeController __instance)
        {
            StartRoutine(__instance.showchallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Hour24ChallengeController), "complete")]
        private static void complete_prefix(Hour24ChallengeController __instance)
        {
            var character = __instance.character;

            var diff = (int)character.settings.rebirthDifficulty;
            var comp = diff switch
            {
                0 => __instance.completions(),
                1 => __instance.evilCompletions(),
                2 => __instance.sadisticCompletions(),
                _ => 0
            };

            // if already max completions, ignore it
            if (comp >= _times[diff].Length)
                return;

            _times[diff][comp] = character.challenges.hour24Challenge.challengeTime.getTimeAsHighscore();
        }
    }
}
