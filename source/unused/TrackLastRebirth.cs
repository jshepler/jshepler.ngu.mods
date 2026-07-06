using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackLastRebirth
    {
        internal static double LastRebirthTotalSeconds
        {
            get => ModSave.Data.LastRebirthTime;
            set => ModSave.Data.LastRebirthTime = value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_prefix()
        {
            LastRebirthTotalSeconds = Plugin.Character.rebirthTime.totalseconds;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthButtonHover), "showTooltip")]
        private static bool RebirthButtonHover_showTooltip_prefix(RebirthButtonHover __instance)
        {
            var character = __instance.character;
            var message = $"<b>Current Rebirth Time:</b> {character.rebirthTime.timeDisplayColon()}";

            if (PauseGame.IsPaused)
                message += " (PAUSED)";

            message += $"\n<b>      Last Rebirth Time:</b> {NumberOutput.timeOutput(TrackLastRebirth.LastRebirthTotalSeconds)}";

            if (character.challenges.inChallenge)
                message += $"\n\n{__instance.challengeInfo.challengeInfoMessage()}";

            __instance.tooltip.showTooltip(message);

            return false;
        }
    }
}
