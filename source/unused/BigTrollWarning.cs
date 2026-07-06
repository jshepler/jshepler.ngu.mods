using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BigTrollWarning
    {
        private static int _secondsBeforeBigTroll;
        private static Color _red = new Color(0.925f, 0.204f, 0.204f);

        [HarmonyPostfix, HarmonyPatch(typeof(CurrentChallengeInfo), "updateChallengeText")]
        private static void CurrentChallengeInfo_updateChallengeText_postfix(CurrentChallengeInfo __instance)
        {
            var character = __instance.character;
            var button = character.buttons.rebirth;

            if (!character.challenges.trollChallenge.inChallenge)
            {
                button.image.color = Color.white;
                return;
            }

            var tf = character.allChallenges.trollChallenge.trollFactor();
            _secondsBeforeBigTroll = tf * 5 - character.challenges.trollCounter % (tf * 5);

            button.image.color = _secondsBeforeBigTroll <= 20 ? _red : Color.white;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CurrentChallengeInfo), "challengeInfoMessage")]
        private static void CurrentChallengeInfo_challengeInfoMessage_postfix(CurrentChallengeInfo __instance, ref string __result)
        {
            if (!__instance.character.challenges.trollChallenge.inChallenge) return;

            __result += $"\n\n<b>Big Troll in:</b> {NumberOutput.timeOutput(_secondsBeforeBigTroll)}";
        }
    }
}
