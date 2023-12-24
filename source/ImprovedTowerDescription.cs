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
        private static float _lastTime = 0f;
        private static Queue<float> _last5KillTimes = new();

        private static string _ppProgressText;
        private static string _killsToNextPPText;
        private static string _killsToNextAPText;
        private static string _killsToNextGuffText;
        private static string _floorsText;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static void AdventureController_enemyDeath_postfix(AdventureController __instance)
        {
            if (__instance.zone != 1000)
            {
                _last5KillTimes.Clear();
                return;
            }

            while (_last5KillTimes.Count > 4)
                _last5KillTimes.Dequeue();

            var time = Time.time;
            if(_lastTime > 0f)
                _last5KillTimes.Enqueue(time - _lastTime);

            _lastTime = time;

            var rollingAvgSecondsPerKill = _last5KillTimes.Count == 0 ? 0 : _last5KillTimes.Average();

            var character = __instance.character;
            var ppProgress = (float)character.adventure.itopod.pointProgress;
            var pointsPerPP = (float)character.adventureController.itopod.pointThreshold();
            _ppProgressText = $"\n\n<b>PP Progress:</b> {ppProgress:#,##0} / {pointsPerPP:#,##0} ({ppProgress / pointsPerPP * 100f:##0.00}%)";

            var optimalFloor = character.calculateBestItopodLevel();
            var maxFloor = character.adventure.highestItopodLevel;
            var currentFloor = __instance.itopodLevel;
            var progressPerKill = Plugin.Character.adventureController.itopod.progressGained(currentFloor);
            //var killsPerPP = Mathf.CeilToInt(pointsPerPP / progressPerKill);
            var killsRemaining = Mathf.CeilToInt((pointsPerPP - ppProgress) / progressPerKill);

            if (currentFloor <= optimalFloor)
            {
                var respawnTime = Plugin.Character.adventureController.respawnTime();
                var idleAttackSpeed = Plugin.Character.adventure.attackSpeed;
                var secondsPerKill = respawnTime + idleAttackSpeed;
                var secondsRemaining = (pointsPerPP - ppProgress) / progressPerKill * secondsPerKill;
                _killsToNextPPText = $"\n\n<b>Kills to next PP:</b> {killsRemaining} in {NumberOutput.timeOutput(secondsRemaining)} ({(currentFloor < optimalFloor ? "sub-optimal" : "optimal")})";
            }
            else if (_last5KillTimes.Count == 0)
            {
                _killsToNextPPText = $"\n\n<b>Kills to next PP:</b> {killsRemaining} in ??? (estimated)";
            }
            else
            {
                var estimatedSecondsRemaining = killsRemaining * rollingAvgSecondsPerKill;
                _killsToNextPPText = $"\n\n<b>Kills to next PP:</b> {killsRemaining} in {NumberOutput.timeOutput(estimatedSecondsRemaining)} (estimated)";
            }


            var tier = character.adventureController.lootDrop.itopodTier(currentFloor);
            var killsToNextAP = __instance.lootDrop.killsUntilAP(currentFloor);
            _killsToNextAPText = $"\n\n<b>Kills to next AP{(tier > 3 ? "/EXP" : string.Empty)}:</b> {killsToNextAP}";


            _killsToNextGuffText = string.Empty;
            if (character.achievements.achievementComplete[145] && character.adventure.itopod.perkLevel[68] >= 1)
            {
                var killsToNextGuff = __instance.lootDrop.killsUntilMacguffin();
                _killsToNextGuffText = $"\n<b>Kills to next MacGuffin:</b> {killsToNextGuff}";
            }

            _floorsText = $"\n\n<b>Max Floor: </b> {maxFloor}"
                + $"\n<b>Optimal Floor:</b> {optimalFloor}";
                //+ $"\n\n<b>Total ITOPOD Kills:</b> {character.display(character.adventure.itopod.enemiesKilled)}";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "zoneDescriptions")]
        private static bool AdventureController_zoneDescriptions_prefix(AdventureController __instance, ref string ___message)
        {
            if (__instance.zone != 1000)
                return true;

            ___message = $"<b>{__instance.zoneName(1000)}</b>" + _ppProgressText + _killsToNextPPText + _killsToNextAPText + _killsToNextGuffText + _floorsText;
            __instance.tooltip.showTooltip(___message);

            return false;
        }

        private static Coroutine _cr;
        private static WaitForSeconds _delay = new WaitForSeconds(.1f);

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "repeatShowTooltip")]
        private static bool AdventureController_repeatShowTooltip_prefix(AdventureController __instance)
        {
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
    }
}
