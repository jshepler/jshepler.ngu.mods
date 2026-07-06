using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ChallengeTimes
{
    [HarmonyPatch]
    internal class OneHundredLevel : BaseChallenge
    {
        private static int[][] _times => ModSave.Data.ChallengeCompletionTimes[(int)ChallengeType.Level100];

        [HarmonyPrefix, HarmonyPatch(typeof(LevelChallenge10KController), "showChallengeInfo")]
        private static bool showChallengeInfo_prefix(LevelChallenge10KController __instance, Text ___challengeInfoText)
        {
            if (!Plugin.AltIsDown)
            {
                ___challengeInfoText.font = Fonts.LiberationSans_Regular;
                return true;
            }

            int[] curCompletions = [__instance.completions(), __instance.evilCompletions(), __instance.sadisticCompletions()];
            var maxCompletions = MaxCompletions[ChallengeType.Level100];
            var chart = BuildCompletionTimes(_times, curCompletions, maxCompletions);

            ___challengeInfoText.text = $"<b>100 Levels Challenge Completion Times</b>\n\n{chart}";
            ___challengeInfoText.font = Fonts.LiberationMono_Regular;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LevelChallenge10KController), "OnPointerEnter")]
        private static bool OnPointerEnter_prefix(LevelChallenge10KController __instance)
        {
            StartRoutine(__instance.showChallengeInfo);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LevelChallenge10KController), "complete")]
        private static void complete_prefix(LevelChallenge10KController __instance)
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

            _times[diff][comp] = character.challenges.levelChallenge10k.challengeTime.getTimeAsHighscore();
        }
    }
}
