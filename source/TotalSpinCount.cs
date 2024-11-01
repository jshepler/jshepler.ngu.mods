using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TotalSpinCount
    {
        [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "showTierTooltipInfo")]
        private static bool DailyRewardController_showTierTooltipInfo_prefix(DailyRewardController __instance)
        {
            if (__instance.currentTier() < 7)
                return true;

            __instance.tooltip.showTooltip($"<b>Your total spin count is {__instance.character.daily.totalSpins}.</b>\n\nYou have reached the max Daily Spin tier! Thanks for a year (or more) of playing NGU IDLE!");
            return false;
        }
    }
}
