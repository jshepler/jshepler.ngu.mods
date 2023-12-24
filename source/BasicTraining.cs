using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BasicTraining
    {
        [HarmonyPrefix, HarmonyPatch(typeof(OffenseTraining), "cap", new Type[0])]
        private static bool OffenseTraining_cap_prefix(OffenseTraining __instance)
        {
            if (!Plugin.Character.settings.syncTraining)
                return true;

            var id = __instance.id;
            
            CapPair(id);
            __instance.updateText();
            __instance.character.allDefenseController.trains[id].updateText();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DefenseTraining), "cap", new Type[0])]
        private static bool DefenseTraining_cap_prefix(DefenseTraining __instance)
        {
            if (!Plugin.Character.settings.syncTraining)
                return true;

            var id = __instance.id;

            CapPair(id);
            __instance.updateText();
            __instance.character.allOffenseController.trains[id].updateText();

            return false;
        }

        private static void CapPair(int id)
        {
            var attackEnergy = Plugin.Character.training.attackEnergy;
            var attackCaps = Plugin.Character.training.attackCaps;
            var defenseEnergy = Plugin.Character.training.defenseEnergy;
            var defenseCaps = Plugin.Character.training.defenseCaps;

            Plugin.Character.idleEnergy += attackEnergy[id] + defenseEnergy[id];

            var amountNeeded = attackCaps[id] + defenseCaps[id];
            if (amountNeeded <= Plugin.Character.idleEnergy)
            {
                attackEnergy[id] = attackCaps[id];
                defenseEnergy[id] = defenseCaps[id];
            }
            else
            {
                var half = Plugin.Character.idleEnergy / 2f;
                attackEnergy[id] = Mathf.CeilToInt(attackCaps[id] / (float)Mathf.CeilToInt(attackCaps[id] / half));
                defenseEnergy[id] = Mathf.CeilToInt(defenseCaps[id] / (float)Mathf.CeilToInt(defenseCaps[id] / half));
            }

            Plugin.Character.idleEnergy -= attackEnergy[id] + defenseEnergy[id];
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OffenseTraining), "addSum")]
        private static bool OffenseTraining_addSum_prefix(OffenseTraining __instance)
        {
            var c = __instance.character;

            if (c.settings.syncTraining) CapAllBasicTraining(c);
            else CapAllOffsenseBasicTraining(c);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DefenseTraining), "addSum")]
        private static bool DefenseTraining_addSum_prefix(OffenseTraining __instance)
        {
            var c = __instance.character;

            if (c.settings.syncTraining) CapAllBasicTraining(c);
            else CapAllDefenseBasicTraining(c);

            return false;
        }

        private static void CapAllOffsenseBasicTraining(Character character)
        {
            var offenseAmount = CalcAmountToAddOffense(character.training);
            character.allOffenseController.trains[0].addEnergy(offenseAmount);
        }

        private static void CapAllDefenseBasicTraining(Character character)
        {
            var defenseAmount = CalcAmountToAddDefense(character.training);
            character.allDefenseController.trains[0].addEnergy(defenseAmount);
        }

        private static void CapAllBasicTraining(Character character)
        {
            var offenseAmount = CalcAmountToAddOffense(character.training);
            var defenseAmount = CalcAmountToAddDefense(character.training);

            if (character.idleEnergy < (offenseAmount + defenseAmount))
            {
                offenseAmount = defenseAmount = character.idleEnergy / 2;
            }

            character.allOffenseController.trains[0].addEnergy(offenseAmount);
            character.allDefenseController.trains[0].addEnergy(defenseAmount);
        }

        private static long CalcAmountToAddOffense(Training training)
        {
            var amount = 0L;
            for (var x = 0; x < training.attackCaps.Length; x++)
            {
                amount += (training.attackCaps[x] - training.attackEnergy[x]);
            }

            return amount;
        }

        private static long CalcAmountToAddDefense(Training training)
        {
            var amount = 0L;
            for (var x = 0; x < training.attackCaps.Length; x++)
            {
                amount += (training.defenseCaps[x] - training.defenseEnergy[x]);
            }

            return amount;
        }

        private static int _showTooltipId = -1;
        private static HoverTooltip _tooltip = null;
        private static Coroutine _showTooltip = null;
        private const string BT_ATK = "Levels gained in this Attack skill will decrease the amount of Energy needed to cap leveling speed upon Rebirth.";
        private const string BT_DEF = "Levels gained in this Defense skill will decrease the amount of Energy needed to cap leveling speed upon Rebirth.";

        [HarmonyPostfix, HarmonyPatch(typeof(OffenseTraining), "Start")]
        private static void OffenseTraining_Start_postfix(OffenseTraining __instance)
        {
            _tooltip = __instance.tooltip;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OffenseTraining), "OnPointerEnter")]
        private static bool OffenseTraining_OnPointerEnter_prefix(OffenseTraining __instance)
        {
            if (_showTooltip != null)
                Plugin.Character.StopCoroutine(_showTooltip);

            _showTooltipId = __instance.id;
            _showTooltip = Plugin.Character.StartCoroutine(ShowTooltip());

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DefenseTraining), "OnPointerEnter")]
        private static bool DefenseTraining_OnPointerEnter_prefix(DefenseTraining __instance)
        {
            if (_showTooltip != null)
                Plugin.Character.StopCoroutine(_showTooltip);

            _showTooltipId = __instance.id + 6;
            _showTooltip = Plugin.Character.StartCoroutine(ShowTooltip());

            return false;
        }

        [HarmonyPrefix
            , HarmonyPatch(typeof(OffenseTraining), "OnPointerExit")
            , HarmonyPatch(typeof(DefenseTraining), "OnPointerExit")]
        private static bool OffenseTraining_OnPointerExit_prefix()
        {
            _tooltip.hideTooltip();
            _showTooltipId = -1;

            if (_showTooltip != null)
                Plugin.Character.StopCoroutine(_showTooltip);

            return false;
        }

        private static IEnumerator ShowTooltip()
        {
            var wait = new WaitForSeconds(.1f);

            while (_showTooltipId >= 0)
            {
                var str = _showTooltipId > 5 ? BuildDefenseTooltip(_showTooltipId - 6) : BuildOffenseTooltip(_showTooltipId);
                _tooltip.showTooltip(str);

                yield return wait;// new WaitForSeconds(.1f);
            }
        }

        // modeled on OffenseTraining.OnPointerEnter()
        private static string BuildOffenseTooltip(int id)
        {
            var character = Plugin.Character;
            var curLevel = character.training.attackTraining[id];
            var curCap = character.training.attackCaps[id];
            var maxReduction = curCap / 10 + 1;
            var capReduction = 0L;

            if (curLevel > 0)
            {
                capReduction = (long)(1f + Mathf.Pow(curLevel - 500f * id, 1.2f) / 500f * (curCap / 1000f));
                if (capReduction < 1)
                    capReduction = 1;
            }

            if (curCap - maxReduction < 1)
                maxReduction = curCap - 1;

            if (capReduction > maxReduction)
                capReduction = maxReduction;

            // L = ((((r-1)/(c/1000))*500)^(1/1.2))+(500*i), thanks JC
            var maxReductionLevel = maxReduction < 1 ? 0L : (long)(Mathf.Pow((maxReduction - 1f) / (curCap / 1000f) * 500f, 1f / 1.2f) + (500f * id));
            var levelsLeft = curLevel >= maxReductionLevel ? 0 : maxReductionLevel - curLevel;
            
            var attackEnergy = character.training.attackEnergy[id];
            var ppt = attackEnergy == 0 ? 0f : (float)attackEnergy / curCap;
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var ticks = levelsLeft * tpb;
            var seconds = ticks / 50f;

            var capAfterRB = curCap - capReduction;
            var maxCapAfterRB = curCap - maxReduction;
            var color = capAfterRB == maxCapAfterRB ? "green" : "red";

            return BT_ATK
                + $"\n\n<b>Current Speed Cap:</b> {curCap}"
                + $"\n<b>   ... after rebirth:</b> <color={color}>{capAfterRB} / {maxCapAfterRB}</color>"
                + $"\n\n<b>Max reduction at level:</b> {maxReductionLevel}"
                + $"\n<b>   ... time remaining:</b> {NumberOutput.timeOutput(seconds)}"
                + $"\n\nBase Attack received per level: {character.display(character.training.trainFactor[id])}";
        }

        private static string BuildDefenseTooltip(int id)
        {
            var character = Plugin.Character;
            var curLevel = character.training.defenseTraining[id];
            var curCap = character.training.defenseCaps[id];
            var maxReduction = curCap / 10 + 1;
            var capReduction = 0L;

            if (curLevel > 0)
            {
                capReduction = (long)(1f + Mathf.Pow(curLevel - 500f * id, 1.2f) / 500f * (curCap / 1000f));
                if (capReduction < 1)
                    capReduction = 1;
            }

            if (curCap - maxReduction < 1)
                maxReduction = curCap - 1;

            if (capReduction > maxReduction)
                capReduction = maxReduction;

            var maxReductionLevel = maxReduction < 1 ? 0L : (long)(Mathf.Pow((maxReduction - 1f) / (curCap / 1000f) * 500f, 1f / 1.2f) + (500f * id));
            var levelsLeft = curLevel >= maxReductionLevel ? 0 : maxReductionLevel - curLevel;

            var defenseEnergy = character.training.defenseEnergy[id];
            var ppt = defenseEnergy == 0 ? 0f : (float)defenseEnergy / curCap;
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var ticks = levelsLeft * tpb;
            var seconds = ticks / 50f;

            var capAfterRB = curCap - capReduction;
            var maxCapAfterRB = curCap - maxReduction;
            var color = capAfterRB == maxCapAfterRB ? "green" : "red";

            return BT_DEF
                + $"\n\n<b>Current Speed Cap:</b> {curCap}"
                + $"\n<b>   ... after rebirth:</b> <color={color}>{capAfterRB} / {maxCapAfterRB}</color>"
                + $"\n\n<b>Max reduction at level:</b> {maxReductionLevel}"
                + $"\n<b>   ... time remaining:</b> {NumberOutput.timeOutput(seconds)}"
                + $"\n\nBase Defense received per level: {character.display(character.training.trainFactor[id])}";
        }
    }
}
