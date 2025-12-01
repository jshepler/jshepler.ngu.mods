using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class Blind : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.Blind];

        [HarmonyPrefix, HarmonyPatch(typeof(BlindChallengeController), "showChallengeInfo")]
        private static bool showChallengeInfo_prefix(BlindChallengeController __instance, Text ___challengeInfo)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfo.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.Blind];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfo.text = $"<b>Blind Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfo.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BlindChallengeController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(BlindChallengeController __instance)
        {
            StartRoutine(__instance.showChallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BlindChallengeController), "complete")]
        private static void complete_prefix(BlindChallengeController __instance)
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

            _times[diff][comp] = character.challenges.blindChallenge.challengeTime.getTimeAsHighscore();
        }
    }
}
