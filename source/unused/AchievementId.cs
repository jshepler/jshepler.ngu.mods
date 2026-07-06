using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AchievementId
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AllAchievementsController), "achievementHint")]
        private static void AllAchievementsController_achievementHint_postfix(int i, ref string __result)
        {
            __result = $"({i}) {__result}";
        }
    }
}
