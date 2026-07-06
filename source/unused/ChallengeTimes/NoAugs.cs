using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class NoAugs : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.NoAugs];

        [HarmonyPrefix, HarmonyPatch(typeof(NoAugsChallengeController), "showChallengeInfo")]
        private static bool showChallengeInfo_prefix(NoAugsChallengeController __instance, Text ___challengeInfo)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfo.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.NoAugs];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfo.text = $"<b>No Augs Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfo.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NoAugsChallengeController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(NoAugsChallengeController __instance)
        {
            StartRoutine(__instance.showChallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NoAugsChallengeController), "complete")]
        private static void complete_prefix(NoAugsChallengeController __instance)
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

            _times[diff][comp] = character.challenges.noAugsChallenge.challengeTime.getTimeAsHighscore();
        }
    }
}
