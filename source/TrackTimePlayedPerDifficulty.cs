using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackTimePlayedPerDifficulty
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress", typeof(int))]
        private static void Character_addOfflineProgress_postfix(int timeElapsed)
        {
            if (Plugin.Character.challenges.levelChallenge10k.inChallenge
                || Plugin.Character.challenges.trollChallenge.inChallenge
                || Plugin.Character.challenges.hour24Challenge.inChallenge
                || timeElapsed <= 0)
                return;

            AddTime(timeElapsed);
            CheckTime();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TotalTimePlayed), "updateTimer")]
        private static void TotalTimePlayed_updateTimer_postfix()
        {
            AddTime(Time.deltaTime);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TotalTimePlayed), "Start")]
        private static void TotalTimePlayed_Start_prefix(TotalTimePlayed __instance)
        {
            __instance.timerText.verticalOverflow = VerticalWrapMode.Overflow;
            __instance.timerText.font = Font.CreateDynamicFontFromOSFont("Lucida Console", 12);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TotalTimePlayed), "updateText")]
        private static void TotalTimePlayed_updateText_postfix(TotalTimePlayed __instance)
        {
            __instance.timerText.text +=
                  $"\n         (normal): {NumberOutput.timeOutput(Data.TotalTimePlayedNormal)}"
                + $"\n           (evil): {NumberOutput.timeOutput(Data.TotalTimePlayedEvil)}"
                + $"\n       (sadistic): {NumberOutput.timeOutput(Data.TotalTimePlayedSadistic)}";
        }

        private static void AddTime(double totalSeconds)
        {
            // if game wasn't started with this mod, start it with current playtime;
            // if current difficulty isn't normal, there's no way to know how much was normal,
            // so make it all normal
            if (Data.TotalTimePlayedNormal == 0.0)
            {
                Data.TotalTimePlayedNormal = Plugin.Character.totalPlaytime.totalseconds;
                return;
            }

            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    Data.TotalTimePlayedNormal += totalSeconds;
                    break;

                case difficulty.evil:
                    Data.TotalTimePlayedEvil += totalSeconds;
                    break;

                case difficulty.sadistic:
                    Data.TotalTimePlayedSadistic += totalSeconds;
                    break;
            }
        }

        private static void CheckTime()
        {
            var diff = Plugin.Character.totalPlaytime.totalseconds
                - Data.TotalTimePlayedNormal
                - Data.TotalTimePlayedEvil
                - Data.TotalTimePlayedSadistic;

            if (diff != 0.0)
            {
                Plugin.LogInfo($"tracked time diff: {NumberOutput.timeOutput(System.Math.Abs(diff))}");
                switch (Plugin.Character.settings.rebirthDifficulty)
                {
                    case difficulty.normal:
                        Data.TotalTimePlayedNormal += diff;
                        break;

                    case difficulty.evil:
                        Data.TotalTimePlayedEvil += diff;
                        break;

                    case difficulty.sadistic:
                        Data.TotalTimePlayedSadistic += diff;
                        break;
                }
            }
        }
    }
}
