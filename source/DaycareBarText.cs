using HarmonyLib;
using UnityEngine.TextCore;

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
            if (id >= character.inventory.daycareTimers.Count || !character.InMenu(Menu.Inventory)) return;
            
            var item = character.inventory.daycare[id];
            if (item.id == 0 || (item.level >= 100 && item.type != part.MacGuffin)) return;

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

            var r = (double)__instance.daycareRate(item);
            var c = character.inventory.daycareTimers[id].totalseconds;
            var b = (double)character.allDiggers.totalDaycareBonus();

            var secondsPerLevel = r / b;
            var secondsRemainingThisLevel = (r - c % r) / b;

            var totalSecondsTo100 = (secondsPerLevel * (99 - (item.level + gained)) ) + secondsRemainingThisLevel;
            __instance.daycareText.text = $"<b><size=12>Level: {item.level + gained} ({NumberOutput.timeOutput(totalSecondsTo100)})</size></b>";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DaycareItemController), "timeLeftMessage")]
        private static void DaycareItemController_timeLeftMessage_postfix(ref string __result, DaycareItemController __instance)
        {
            var character = __instance.character;
            var item = character.inventory.daycare[__instance.id];

            var r = (double)__instance.daycareRate(item);
            var b = (double)character.allDiggers.totalDaycareBonus();
            var secondsPerLevel = r / b;
            
            __result += $"\n<b>Time Per Level:</b> {NumberOutput.timeOutput(secondsPerLevel)}";
        }
    }
}
