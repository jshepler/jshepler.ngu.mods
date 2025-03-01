using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TotalDiggerLevels
    {
        private const string TEXT = "This bonus is applied to the effects of all active Gold Diggers, and is based on the sum of your Gold Diggers' max levels - even the inactive ones! It's really good!";

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "Start")]
        private static void AllGoldDiggerController_Start_postfix(AllGoldDiggerController __instance)
        {
            __instance.levelBonus.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(e => Plugin.ShowTooltip($"{TEXT}\n\n<b>Total Digger Levels:</b> {__instance.sumOfAllLevels()}"));
        }
    }
}
