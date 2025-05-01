using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareBarText
    {
        [HarmonyPostfix, HarmonyPatch(typeof(DaycareItemController), "updateDaycareTimer")]
        private static void DaycareItemController_updateDaycareTimer_postfix(DaycareItemController __instance)
        {
            var id = __instance.id;
            var character = __instance.character;
            if (id >= character.inventory.daycareTimers.Count || !character.InMenu(Menu.Inventory))
                return;
            
            var item = character.inventory.daycare[id];
            if (item.id == 0 || (item.level >= 100 && item.type != part.MacGuffin))
                return;

            var gained = __instance.levelsAdded();
            if (item.type == part.MacGuffin || item.level + gained >= 100)
            {
                __instance.daycareText.text = $"<b>Level: {item.level + gained}</b>";
                return;
            }

            // based on DaycareItemController.timeLeftMessage()
            //  r = dacareRate(Equipment)
            //  c = current total seconds
            //  b = digger daycare bonus
            // time left = (r - c % r) / b
            // so when c = 0, seconds per level = r / b

            //var r = (double)__instance.daycareRate(item);
            //var c = character.inventory.daycareTimers[id].totalseconds;
            //var b = (double)character.allDiggers.totalDaycareBonus();

            //var secondsPerLevel = r / b;
            //var secondsRemainingThisLevel = (r - c % r) / b;

            //var totalSecondsTo100 = (secondsPerLevel * (99 - (item.level + gained)) ) + secondsRemainingThisLevel;
            var totalSecondsTo100 = TimeToMaxLevel(__instance);
            __instance.daycareText.text = $"<b><size=12>Level: {item.level + gained} ({NumberOutput.timeOutput(totalSecondsTo100)})</size></b>";
        }

        internal static double TimeToMaxLevel(DaycareItemController controller, int levelOffset = 0)
        {
            var id = controller.id;
            var character = controller.character;
            var item = character.inventory.daycare[id];
            var gained = controller.levelsAdded();

            // based on DaycareItemController.timeLeftMessage()
            //  r = dacareRate(Equipment)
            //  c = current total seconds
            //  b = digger daycare bonus
            // time left = (r - c % r) / b
            // so when c = 0, seconds per level = r / b

            var r = (double)controller.daycareRate(item);
            var c = character.inventory.daycareTimers[id].totalseconds;
            var b = (double)character.allDiggers.totalDaycareBonus();

            var secondsPerLevel = r / b;
            var secondsRemainingThisLevel = (r - c % r) / b;

            var currentLevel = item.level + gained + levelOffset;
            var totalSecondsTo100 = (secondsPerLevel * (99 - (currentLevel))) + secondsRemainingThisLevel;

            return totalSecondsTo100;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DaycareItemController), "timeLeftMessage")]
        private static void DaycareItemController_timeLeftMessage_postfix(ref string __result, DaycareItemController __instance)
        {
            var character = __instance.character;
            var item = character.inventory.daycare[__instance.id];

            var r = (double)__instance.daycareRate(item);
            var b = (double)character.allDiggers.totalDaycareBonus();
            var secondsPerLevel = r / b;
            var baseTime = character.itemInfo.daycareRate[item.id];

            __result += $"\n<b>Time per level:</b> {NumberOutput.timeOutput(secondsPerLevel)}";

            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                __result += $"\n   base: {NumberOutput.timeOutput(baseTime)}"
                    + $"\n   time modifier: x {timeModifier():#,##0.0###} ({NumberOutput.timeOutput(r)})"
                    + $"\n   speed divider: / {b:#,##0.0###} ({NumberOutput.timeOutput(secondsPerLevel)})";
        }

        private static float timeModifier()
        {
            var character = Plugin.Character;
            var totalModifier = 1f;

            var blindCompletions = character.allChallenges.blindChallenge.completions();
            if (blindCompletions > 0)
            {
                var blindModifier = 1f - 0.05f - blindCompletions * 0.01f;
                totalModifier *= blindModifier;
            }

            var perk27 = character.adventure.itopod.perkLevel[27];
            var perk28 = character.adventure.itopod.perkLevel[28];
            if (perk27 > 0 || perk28 > 0)
            {
                var perkModifier = 1f - perk27 * character.adventureController.itopod.effectPerLevel[27];
                perkModifier *= 1f - perk28 * character.adventureController.itopod.effectPerLevel[28];
                totalModifier *= perkModifier;
            }

            if (character.arbitrary.hasDaycareSpeed)
                totalModifier *= 0.9f;

            return totalModifier;
        }
    }
}
