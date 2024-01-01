using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DefaultPlayerPortrait
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "Start")]
        private static void BossController_Start_postfix(BossController __instance)
        {
            var bossId = Options.DefaultPlayerPortait.BossId.Value;

            if (bossId > 0)
                __instance.playerPortraitSprites[0] = __instance.bossPortraitSprites[bossId - 1];
        }
    }
}
