using System;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RightClickQuirkCostInTooltip
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "showTooltip")]
        private static void BeastQuestPerkController_showTooltip_postfix(int id, BeastQuestPerkController __instance, ref string ___message)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Quirks) || id < 0 || id > character.beastQuest.quirkLevel.Count)
                return;

            var quirkLevel = character.beastQuest.quirkLevel[id];
            var maxLevel = __instance.maxLevel[id];
            if (quirkLevel >= maxLevel || (maxLevel - quirkLevel) < 2)
                return;

            var qp = character.beastQuest.quirkPoints;
            var cost = __instance.cost[id];
            if (qp < cost * 2)
                return;

            var maxLevelsCanBuy = qp / cost;
            var buyLevels = Math.Min(maxLevelsCanBuy, maxLevel - quirkLevel);
            var buyCost = buyLevels * cost;

            ___message += $"\n\nRight-Click: <b>{buyCost} QP, +{buyLevels} Level{(buyLevels > 1 ? "s" : "")} = Level {quirkLevel + buyLevels}</b>";
            __instance.tooltip.showTooltip(___message);
        }
    }
}
