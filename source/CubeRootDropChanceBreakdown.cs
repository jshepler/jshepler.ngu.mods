using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CubeRootDropChanceBreakdown
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure")]
        private static void StatsDisplay_displayMiscAdventure_postfix(StatsDisplay __instance)
        {
            var character = __instance.character;

            __instance.statsBreakdown.text += $"\n<b>(cube root):</b> ";
            __instance.statValue.text += $"\n  {character.display(character.lootFactorRooted() * 100)}%";
        }
    }
}
