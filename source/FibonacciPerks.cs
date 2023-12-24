using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FibonacciPerks
    {
        private static Dictionary<int, string> _unlocks = new()
            {
                { 1, "+10% Energy and Magic Power" },
                { 2, "+10% Energy Cap" },
                { 3, "+10% Magic Cap" },
                { 5, "+5% Energy NGU Speed" },
                { 8, "+5% Magic NGU Speed" },
                { 13, "+5% PP Earnings" },
                { 21, "+10% Energy and Magic Bars" },
                { 34, "+13% Adventure Stats" },
                { 55, "+5% Daycare Speed" },
                { 89, "+2% Bonus to AP Earnings" },
                { 144, "+5% Chance for +1 level on Loot!" },
                { 233, "+10% QP Rewards" },
                { 377, "377% Attack/Def Multiplier" },
                { 610, "No More Quest Assignment RNG!" },
                { 987, "+5% Bonus EXP Gains" },
                { 1597, "FIBONACCI KITTY ART" }
            };
        
        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "fibPerkUnlocks")]
        private static void ItopodPerkController_fibPerkUnlocks_postfix(ItopodPerkController __instance, ref string __result)
        {
            var perkLevel = __instance.character.adventure.itopod.perkLevel[94];
            var color = (int i) => perkLevel < i ? "red" : "green";

            __result = "\n\n<b>Fibonacci Perk Unlocks:</b>\n"
                + _unlocks.Join(kv => $"<b>Level {kv.Key}:</b> <color={color(kv.Key)}>{kv.Value}</color>", "\n");
        }
    }
}
