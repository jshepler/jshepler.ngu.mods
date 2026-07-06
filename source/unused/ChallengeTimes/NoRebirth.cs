using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class NoRebirth : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.NoRebirth];

        [HarmonyPrefix, HarmonyPatch(typeof(NoRebirthChallengeController), "showchallengeInfo")]
        private static bool showChallengeInfo_prefix(NoRebirthChallengeController __instance, Text ___challengeInfo)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfo.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.NoRebirth];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfo.text = $"<b>No Rebirth Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfo.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NoRebirthChallengeController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(NoRebirthChallengeController __instance)
        {
            StartRoutine(__instance.showchallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NoRebirthChallengeController), "complete")]
        private static void complete_prefix(NoRebirthChallengeController __instance)
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

            _times[diff][comp] = character.challenges.noRebirthChallenge.challengeTime.getTimeAsHighscore();
        }
    }
}
