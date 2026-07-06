using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AchievementsNotify
    {
        [HarmonyPrefix, HarmonyPatch(typeof(AllAchievementsController), "markAchievementAsComplete")]
        private static void markAchievementAsComplete_prefix(int id, AllAchievementsController __instance)
        {
            if (__instance.character.achievements.achievementComplete[id] || id == 145)
                return;

            __instance.character.achievements.achievementComplete[id] = true;
            Plugin.ShowNotification($"<b>Achievement completed!</b>\n\n{__instance.achievementHint(id)}", 5f);
        }
    }
}
