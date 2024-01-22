using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedTowerDescription
    {
        private const float MAXPROGRESS = 1000000f;

        private static float _lastTime = 0f;
        private static Queue<float> _last5KillTimes = new();

        private static string _tooltipText;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static void AdventureController_enemyDeath_postfix(AdventureController __instance)
        {
            if (__instance.zone != 1000)
            {
                _last5KillTimes.Clear();
                _lastTime = 0f;

                return;
            }

            while (_last5KillTimes.Count > 4)
                _last5KillTimes.Dequeue();

            var time = Time.time;
            if (_lastTime > 0f)
                _last5KillTimes.Enqueue(time - _lastTime);

            _lastTime = time;

            UpdateTooltip();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "zoneDescriptions")]
        private static bool AdventureController_zoneDescriptions_prefix(AdventureController __instance, ref string ___message)
        {
            if (__instance.zone != 1000)
                return true;

            ___message = $"<b>{__instance.zoneName(1000)}</b>" + _tooltipText;
            __instance.tooltip.showTooltip(___message);

            return false;
        }

        private static Coroutine _cr;
        private static WaitForSeconds _delay = new WaitForSeconds(.1f);

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "repeatShowTooltip")]
        private static bool AdventureController_repeatShowTooltip_prefix(AdventureController __instance)
        {
            UpdateTooltip();
            _cr = __instance.StartCoroutine(ShowTooltip());

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "tooltipHide")]
        private static bool AdventureController_tooltipHide_prefix(AdventureController __instance)
        {
            if (_cr != null)
            {
                __instance.StopCoroutine(_cr);
                _cr = null;
            }

            Plugin.Character.adventureController.tooltip.hideTooltip();
            return false;
        }

        private static IEnumerator ShowTooltip()
        {
            while (true)
            {
                Plugin.Character.adventureController.zoneDescriptions();
                yield return _delay;
            }
        }

        private static void UpdateTooltip()
        {
            var character = Plugin.Character;
            var controller = character.adventureController;

            var optimalFloor = character.calculateBestItopodLevel();
            var maxFloor = character.adventure.highestItopodLevel;
            var currentFloor = controller.itopodLevel;

            var progressPerKill = controller.itopod.progressGained(currentFloor);
            var killsPerPP = Mathf.CeilToInt(MAXPROGRESS / progressPerKill);

            var currentProgress = (float)character.adventure.itopod.pointProgress;
            var killsRemaining = Mathf.CeilToInt((MAXPROGRESS - currentProgress) / progressPerKill);

            var secondsPerKill = _last5KillTimes.Count == 0 ? 0 : _last5KillTimes.Average();
            var isEstimated = currentFloor > optimalFloor;

            if (!isEstimated)
            {
                var respawnTime = Plugin.Character.adventureController.respawnTime();
                var idleAttackSpeed = Plugin.Character.adventure.attackSpeed;
                secondsPerKill = respawnTime + idleAttackSpeed;
            }

            var secondsPerPP = killsPerPP * secondsPerKill;
            var secondsRemaining = killsRemaining * secondsPerKill; //secondsPerKill == 0 ? 0 : (MAXPROGRESS - currentProgress) / progressPerKill * secondsPerKill;

            var ppPerKill = (float)progressPerKill / MAXPROGRESS;
            var ppPerHour = secondsPerKill == 0 ? 0 : (60 * 60 / secondsPerKill) * ppPerKill;

            _tooltipText = $"\n\n<b>PP Progress:</b> {currentProgress:#,##0} / {MAXPROGRESS:#,##0} ({currentProgress / MAXPROGRESS * 100f:##0.00}%)"
                + $"\n\n<b>Seconds per kill:</b> {(secondsPerKill == 0.0 ? "????" : NumberOutput.timeOutput(secondsPerKill))} ({(isEstimated ? "estimated" : currentFloor < optimalFloor ? "sub-optimal" : "optimal")})"
                + $"\n<b>Kills to next PP:</b> {killsRemaining} in {(secondsRemaining == 0.0 ? "????" : NumberOutput.timeOutput(secondsRemaining))}"
                + $"\n\n<b>Kills per PP:</b> {killsPerPP} taking {(secondsPerPP == 0 ? "????" : NumberOutput.timeOutput(secondsPerPP))}"
                + $"\n<b>PP per hour:</b> {ppPerHour:#,##0.##}";


            var tier = character.adventureController.lootDrop.itopodTier(currentFloor);
            var killsToNextAP = controller.lootDrop.killsUntilAP(currentFloor);
            _tooltipText += $"\n\n<b>Kills to next AP{(tier > 3 ? "/EXP" : string.Empty)}:</b> {killsToNextAP} in {(secondsPerKill == 0.0 ? "???" : NumberOutput.timeOutput(killsToNextAP * secondsPerKill))}";


            if (character.achievements.achievementComplete[145] && character.adventure.itopod.perkLevel[68] >= 1)
            {
                var killsToNextGuff = controller.lootDrop.killsUntilMacguffin();
                _tooltipText += $"\n<b>Kills to next MacGuffin:</b> {killsToNextGuff} in {(secondsPerKill == 0.0 ? "???" : NumberOutput.timeOutput(killsToNextGuff * secondsPerKill))}";
            }


            _tooltipText += $"\n\n<b>Max Floor: </b> {maxFloor}"
                + $"\n<b>Optimal Floor:</b> {optimalFloor}";
        }
    }
}
