using System;
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

        private static float _atpNeeded(float power) => AdvancedTrainingTitanAK.GetNeededAT(new(power, 0f)).Power;

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

            var optimalFloor = CalculateOptimalFloor();
            var maxFloor = character.adventure.highestItopodLevel;
            var currentFloor = controller.itopodLevel;

            var progressPerKill = controller.itopod.progressGained(currentFloor);
            var killsPerPP = Mathf.CeilToInt(MAXPROGRESS / progressPerKill);

            var currentProgress = (float)character.adventure.itopod.pointProgress;
            var killsRemaining = Mathf.CeilToInt((MAXPROGRESS - currentProgress) / progressPerKill);

            var isEstimated = currentFloor > optimalFloor || !character.adventure.autoattacking;
            var secondsPerKill = isEstimated ? _last5KillTimes.Count == 0 ? 0 : _last5KillTimes.Average() : 0f;

            if (!isEstimated)
            {
                var respawnTime = Plugin.Character.adventureController.respawnTime();
                var idleAttackSpeed = Plugin.Character.adventure.attackSpeed;
                secondsPerKill = respawnTime + idleAttackSpeed;
            }

            var killsPerHour = 3600f / secondsPerKill;
            var killsPerDay = 86400f / secondsPerKill;

            _tooltipText = $"\n\n<b>PP Progress:</b> {currentProgress:#,##0} / {MAXPROGRESS:#,##0} ({currentProgress / MAXPROGRESS * 100f:##0.00}%)"
                + $"\n\n<b>Seconds per kill:</b> {(secondsPerKill == 0f ? "????" : NumberOutput.timeOutput(secondsPerKill))} ({(isEstimated ? "estimated" : currentFloor < optimalFloor ? "sub-optimal" : "optimal")})"
                + $"\n<b>Kills per hour:</b> {killsPerHour:#,##0.##}"
                + $"\n<b>Kills per day:</b> {killsPerDay:#,##0.##}";


            var secondsPerPP = killsPerPP * secondsPerKill;
            var secondsRemaining = killsRemaining * secondsPerKill;
            var ppPerKill = (float)progressPerKill / MAXPROGRESS;
            var ppPerHour = secondsPerKill == 0f ? 0 : killsPerHour * ppPerKill;
            var ppPerDay = secondsPerKill == 0f ? 0 : killsPerDay * ppPerKill;

            if (killsPerPP == 1)
                _tooltipText += $"\n\n<b>PP per kill:</b> {ppPerKill:#,##0.00}";
            else
                _tooltipText += $"\n\n<b>Kills per PP:</b> {killsPerPP} taking {(secondsPerPP == 0f ? "????" : NumberOutput.timeOutput(secondsPerPP))}"
                    + $"\n<b>Kills to next PP:</b> {killsRemaining} in {(secondsRemaining == 0f ? "????" : NumberOutput.timeOutput(secondsRemaining))}";

            //_tooltipText += killsPerPP == 1
            //    ? $"\n\n<b>PP per kill:</b> {ppPerKill:#,##0.00}"
            //    : $"\n\n<b>Kills per PP:</b> {killsPerPP} taking {(secondsPerPP == 0f ? "????" : NumberOutput.timeOutput(secondsPerPP))}";

            //if (killsPerPP > 1)
            //    _tooltipText += $"\n<b>Kills to next PP:</b> {killsRemaining} in {(secondsRemaining == 0f ? "????" : NumberOutput.timeOutput(secondsRemaining))}";

            _tooltipText += $"\n<b>PP per hour:</b> {ppPerHour:#,##0.##}"
                + $"\n<b>PP per day:</b> {ppPerDay:#,##0.##}";


            var tier = character.adventureController.lootDrop.itopodTier(currentFloor);
            var killsPerEXP = controller.lootDrop.killsPerEXP(tier);
            var killsToNextAP = controller.lootDrop.killsUntilAP(currentFloor);
            var baseExpPerGroup = controller.lootDrop.itopodEXPAwarded(tier);
            var expPerGroup = character.checkExpAdded(baseExpPerGroup);
            var secondsPerExpGroup = killsPerEXP * secondsPerKill;
            var expPerDay = secondsPerKill == 0f ? 0L : (long)(60 * 60 * 24 / secondsPerExpGroup) * expPerGroup;
            var apPerDay = secondsPerKill == 0f ? 0L : (long)(60 * 60 * 24 / secondsPerExpGroup);

            _tooltipText += $"\n\n<b>Kills per EXP/AP drop:</b> {killsPerEXP} taking {(secondsPerExpGroup == 0f ? "????" : NumberOutput.timeOutput(secondsPerExpGroup))}"
                + $"\n<b>Kills to next EXP/AP:</b> {killsToNextAP} in {(secondsPerKill == 0f ? "???" : NumberOutput.timeOutput(killsToNextAP * secondsPerKill))}"
                + $"\n<b>EXP per drop:</b> {character.display(expPerGroup)} ({baseExpPerGroup} base)"
                + $"\n<b>EXP per day:</b> {character.display(expPerDay)}"
                + $"\n<b>AP per day:</b> {character.display(apPerDay)}";


            if (character.adventure.itopod.perkLevel[30] >= 1)
            {
                var killsPerPoop = character.adventureController.itopod.poopThreshold();
                var killsToNextPoop = killsPerPoop - character.adventure.itopod.poopProgress;
                var dcPoop = character.adventureController.itopod.effectPerLevel[30];
                var avgPoopPerDay = (killsPerDay / killsPerPoop) + (killsPerDay * dcPoop);
                _tooltipText += $"\n\n<b>Kills per Poop:</b> {killsPerPoop} taking {NumberOutput.timeOutput(killsPerPoop * secondsPerKill)}"
                    + $"\n<b>Kills to next Poop:</b> {killsToNextPoop} in {(secondsPerKill == 0f ? "???" : NumberOutput.timeOutput(killsToNextPoop * secondsPerKill))}"
                    + $"\n<b>DC per kill:</b> {dcPoop * 100f:0.####}%"
                    + $"\n<b>Avg Poop per day:</b> ~{avgPoopPerDay:#,##0.##}";
            }


            if (character.achievements.achievementComplete[145] && character.adventure.itopod.perkLevel[68] >= 1)
            {
                var killsPerGuff = controller.lootDrop.killsPerMacguffin();
                var killsToNextGuff = controller.lootDrop.killsUntilMacguffin();
                var guffsPerDay = killsPerDay / killsPerGuff;
                _tooltipText += $"\n\n<b>Kills per MacGuffin:</b> {killsPerGuff} taking {NumberOutput.timeOutput(killsPerGuff * secondsPerKill)}"
                    + $"\n<b>Kills to next MacGuffin:</b> {killsToNextGuff} in {(secondsPerKill == 0f ? "???" : NumberOutput.timeOutput(killsToNextGuff * secondsPerKill))}"
                    + $"\n<b>MacGuffins per day:</b> {character.display(guffsPerDay)}";
            }

            _tooltipText += $"\n\n<b>Max Floor: </b> {maxFloor - 1}"
                + $"\n<b>Optimal Floor:</b> {optimalFloor}";

            var currentATP = character.advancedTraining.level[1];
            _tooltipText += $"\n\n<b>Current AT Power:</b> {character.display(currentATP)}";

            var nextOptimalFloorPower = getPowForOpt(optimalFloor + 1);
            var nextOptimalATP = _atpNeeded(nextOptimalFloorPower);
            if (optimalFloor < 1599)
                _tooltipText += $"\n<b>ATP for next opt:</b> {character.display(nextOptimalATP)}";

            var next50Floor = (Mathf.FloorToInt(optimalFloor / 50f) + 1) * 50;
            var next50FloorPower = next50Floor < 1600 ? getPowForOpt(next50Floor) : 0f;
            var next50FloorATP = _atpNeeded(next50FloorPower);
            if (next50Floor < 1600)
                _tooltipText += $"\n<b>  ... next 50th ({next50Floor}):</b> {character.display(next50FloorATP)}";

            var nextBoostFloor = _boostFloors.FirstOrDefault(f => f > optimalFloor);
            var nextBoostFloorPower = nextBoostFloor == 0 ? 0 : getPowForOpt(nextBoostFloor);
            var nextBoostATP = _atpNeeded(nextBoostFloorPower);
            if(nextBoostFloor > 0)
                _tooltipText += $"\n<b>  ... next boost ({nextBoostFloor}):</b> {character.display(nextBoostATP)}";
        }

        private static int[] _boostFloors = [0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 700, 850, 1150];

        private static int CalculateOptimalFloor()
        {
            var character = Plugin.Character;

            var totalAdvAttack = character.totalAdvAttack();
            if (totalAdvAttack < 700f)
                return 0;

            var optimalAttack = totalAdvAttack / 765f * character.idleAttackPower();

            int floor = Convert.ToInt32(Math.Floor(Math.Log(optimalAttack, 1.05)));

            if (floor < 1)
                return 1;

            return floor;
        }

        private static float getPowForOpt(int floor)
        {
            var iap = Plugin.Character.idleAttackPower();
            return Mathf.Pow(1.05f, floor) * 765 / iap;
        }

        private static int getOptForPow(float pow)
        {
            var iap = Plugin.Character.idleAttackPower();
            return Convert.ToInt32(Math.Floor(Math.Log(pow / 765.0 * iap, 1.05)));
        }
    }
}
