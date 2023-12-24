using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackTimePlayedPerDifficulty
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress", typeof(int))]
        private static void Character_addOfflineProgress_postfix(int timeElapsed)
        {
            if(timeElapsed > 0)
                AddTime(timeElapsed);
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
                //    Total Time Played:
                  $"\n         (normal): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedNormal)}"
                + $"\n           (evil): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedEvil)}"
                + $"\n       (sadistic): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedSadistic)}";
            //  $"\n                 (normal): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedNormal)}"
            //+ $"\n                       (evil): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedEvil)}"
            //+ $"\n                (sadistic): {NumberOutput.timeOutput(ModSave.Data.TotalTimePlayedSadistic)}";
        }

        private static void AddTime(double totalSeconds)
        {
            // if game wasn't started with this mod, start it with current playtime;
            // if current difficulty isn't normal, there's no way to know how much was normal,
            // so make it all normal
            if (ModSave.Data.TotalTimePlayedNormal == 0.0)
            {
                ModSave.Data.TotalTimePlayedNormal = Plugin.Character.totalPlaytime.totalseconds;
                return;
            }

            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    ModSave.Data.TotalTimePlayedNormal += totalSeconds;
                    break;

                case difficulty.evil:
                    ModSave.Data.TotalTimePlayedEvil += totalSeconds;
                    break;

                case difficulty.sadistic:
                    ModSave.Data.TotalTimePlayedSadistic += totalSeconds;
                    break;
            }
        }
    }
}
