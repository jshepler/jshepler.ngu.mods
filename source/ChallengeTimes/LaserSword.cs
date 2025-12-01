using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class LaserSword : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.LaserSword];

        [HarmonyPrefix, HarmonyPatch(typeof(LaserSwordChallengeController), "showChallengeInfo")]
        private static bool showChallengeInfo_prefix(LaserSwordChallengeController __instance, Text ___challengeInfo)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfo.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.LaserSword];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfo.text = $"<b>Laser Sword Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfo.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LaserSwordChallengeController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(LaserSwordChallengeController __instance)
        {
            StartRoutine(__instance.showChallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LaserSwordChallengeController), "complete")]
        private static void complete_prefix(LaserSwordChallengeController __instance)
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

            _times[diff][comp] = character.challenges.laserSwordChallenge.challengeTime.getTimeAsHighscore();
        }
    }
}
