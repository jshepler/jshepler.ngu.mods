using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedQuirkTooltip
    {
        private static BeastQuestPerkController _quirkController;
        private static List<int> _noQuirkBonusStrings;

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
        private static void BeastQuestPerkController_Start_postfix(BeastQuestPerkController __instance)
        {
            _quirkController = __instance;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "showTooltip")]
        private static void BeastQuestPerkController_showTooltip_postfix(int id, BeastQuestPerkController __instance, ref string ___message)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Quirks) || id < 0 || id > character.beastQuest.quirkLevel.Count)
                return;

            var display = (double d) => Plugin.Character.display(d);
            var currentLevel = character.beastQuest.quirkLevel[id];
            var maxLevel = __instance.maxLevel[id];
            if (currentLevel >= maxLevel)
                return;

            var qp = character.beastQuest.quirkPoints;
            var costPerLevel = __instance.cost[id];
            var maxLevelsCanBuy = qp / costPerLevel;
            var buyLevels = (int)Math.Min(maxLevelsCanBuy, maxLevel - currentLevel);

            var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
            var ppGainedLastRB = TrackQPGained.QPGainedLastRB;
            var gainedPerSec = secondsLastRB == 0.0 ? 0.0 : ppGainedLastRB / secondsLastRB;

            var qpToNextLevel = costPerLevel - qp;
            if (qpToNextLevel > 0)
            {
                ___message += $"\n\n<b>QP to next level: {display(costPerLevel - qp)}</b>";

                if (gainedPerSec > 0)
                    ___message += $"\n ... est. days: {display(Math.Floor(qpToNextLevel / gainedPerSec / 86400.0))}";
            }

            else //if (buyLevels > 1)
            {
                var buyCost = buyLevels * costPerLevel;
                var newLevel = currentLevel + buyLevels;

                ___message += $"\n\n<b>Right-Click:</b> <size=10>(max levels: {maxLevelsCanBuy})</size>"
                    + $"\n   <b>New Level: {(newLevel == maxLevel ? "<color=green>MAX</color>" : $"{newLevel}</b> <size=10>(+{buyLevels})</size><b>")}"
                    + getNewQuirkLevelBonus(id, buyLevels)
                    + $"\n   COST: {display(buyCost)} Quirk Point{(buyCost > 1 ? "s" : string.Empty)}</b>";
            }

            var levelsToMax = maxLevel - currentLevel;
            var qpToMax = costPerLevel * levelsToMax - qp;
            if (levelsToMax > 1 && qpToMax > 0)
            {
                ___message += $"\n\n<b>QP to max level: {display(qpToMax)}</b>";

                if (gainedPerSec > 0)
                    ___message += $"\n ... est. days: {display(Math.Floor(qpToMax / gainedPerSec / 86400.0))}";
            }

            __instance.tooltip.showTooltip(___message);
        }

        private static string getNewQuirkLevelBonus(int quirkId, int offset)
        {
            if (_noQuirkBonusStrings == null)
                _noQuirkBonusStrings = Enumerable.Range(20, 14).Select(i => i).ToList();

            if (!_quirkController.hasStatEffect[quirkId]
                || _noQuirkBonusStrings.Contains(quirkId)
                || _quirkController.character.beastQuest.quirkLevel[quirkId] >= _quirkController.capLevel(quirkId))
                return string.Empty;

            return $"\n   New Level Bonus: {_quirkController.percentEffect(quirkId, offset)}%";
        }
    }
}
