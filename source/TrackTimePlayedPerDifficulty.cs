using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackTimePlayedPerDifficulty
    {
        // moved to OfflineTime as this would still run even if SkipOffline was enabled,
        //[HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress", typeof(int))]
        //private static void Character_addOfflineProgress_postfix(int timeElapsed)
        //{
        //    if (Plugin.Character.challenges.levelChallenge10k.inChallenge
        //        || Plugin.Character.challenges.trollChallenge.inChallenge
        //        || Plugin.Character.challenges.hour24Challenge.inChallenge
        //        || timeElapsed <= 0)
        //        return;

        //    AddTime(timeElapsed, !OfflineTime.SkipOfflineProgress);
        //    CheckTime();
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(TotalTimePlayed), "updateTimer")]
        private static void TotalTimePlayed_updateTimer_postfix()
        {
            AddTime(Time.deltaTime);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TotalTimePlayed), "Start")]
        private static void TotalTimePlayed_Start_prefix(TotalTimePlayed __instance)
        {
            __instance.timerText.verticalOverflow = VerticalWrapMode.Overflow;
            __instance.timerText.font = Fonts.LiberationMono_Regular;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TotalTimePlayed), "updateText")]
        private static void TotalTimePlayed_updateText_postfix(TotalTimePlayed __instance)
        {
            __instance.timerText.text += $" (offline: {NumberOutput.timeOutput(Data.TotalTimeOffline)})"
                + $"\n         (normal): {NumberOutput.timeOutput(Data.TotalTimePlayedNormal)} (offline: {NumberOutput.timeOutput(Data.TotalTimeOfflineNormal)})"
                + $"\n           (evil): {NumberOutput.timeOutput(Data.TotalTimePlayedEvil)} (offline: {NumberOutput.timeOutput(Data.TotalTimeOfflineEvil)})"
                + $"\n       (sadistic): {NumberOutput.timeOutput(Data.TotalTimePlayedSadistic)} (offline: {NumberOutput.timeOutput(Data.TotalTimeOfflineSadistic)})";
        }

        internal static void AddTime(double totalSeconds, bool offline = false)
        {
            // I don't know of a way to know how much of current play time is in what difficulty,
            // but if player is in normal, assume all of it is normal;
            // otherwise, just start tracking from this point on
            if (Data.TotalTimePlayedNormal == 0.0 && Plugin.Character.settings.rebirthDifficulty == difficulty.normal)
            {
                Data.TotalTimePlayedNormal = Plugin.Character.totalPlaytime.totalseconds;
                return;
            }

            if (offline)
                Data.TotalTimeOffline += totalSeconds;

            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    Data.TotalTimePlayedNormal += totalSeconds;
                    if (offline)
                        Data.TotalTimeOfflineNormal += totalSeconds;
                    break;

                case difficulty.evil:
                    Data.TotalTimePlayedEvil += totalSeconds;
                    if (offline)
                        Data.TotalTimeOfflineEvil += totalSeconds;
                    break;

                case difficulty.sadistic:
                    Data.TotalTimePlayedSadistic += totalSeconds;
                    if (offline)
                        Data.TotalTimeOfflineSadistic += totalSeconds;
                    break;
            }
        }

        internal static void CheckTime()
        {
            //Data.TotalTimePlayedNormal = new System.TimeSpan(123, 23, 25, 54, 0).TotalSeconds;
            //Data.TotalTimePlayedEvil = new System.TimeSpan(207, 15, 11, 30, 0).TotalSeconds;

            var diff = Plugin.Character.totalPlaytime.totalseconds
                - Data.TotalTimePlayedNormal
                - Data.TotalTimePlayedEvil
                - Data.TotalTimePlayedSadistic;

            if (diff != 0.0)
            {
                //Plugin.LogInfo($"tracked time diff: {NumberOutput.timeOutput(System.Math.Abs(diff))}");
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
