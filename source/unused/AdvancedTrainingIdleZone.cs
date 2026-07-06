using System;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AdvancedTrainingIdleZone
    {
        static Enemy _enemy;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "zoneDescriptions")]
        private static void AdventureController_zoneDescriptions_postfix(AdventureController __instance, ref string ___message)
        {
            var character = __instance.character;
            var advancedTraining = character.advancedTrainingController;
            var advController = character.adventureController;

            if (__instance.zone == -1 || __instance.zone >= 1000 || Input.GetKey(KeyCode.LeftAlt)) return;

            //var enemies = __instance.enemyList[__instance.zone];
            //if (enemies == null || enemies.Count == 0) return;

            if(advController.fightInProgress && _enemy != advController.currentEnemy)
                _enemy = advController.currentEnemy;

            if (_enemy == null) return;

            var currentATPLevel = character.advancedTraining.level[1];
            var atText = $"\n\nAT Power: {character.display(currentATPLevel)}\nEnemy: {_enemy.name}";

            // taken from PlayerController.idleAttack()
            // (doesn't use def/2 becase def isn't known yet - used below)
            // a random modifier from 0.8 to 1.2 is applied, using 0.8 here to make sure the calculated ATP is enough to cover it
            var totalAdvAttack = character.totalAdvAttack() * character.idleAttackPower() * 0.8f;
            
            // the +1 is to convert from "bonus" to "multiplier"
            var totalPowerWithoutATPowerBonus = totalAdvAttack / (advancedTraining.adventurePowerBonus(0) + 1f);

            for (var numberOfHits = 1; numberOfHits < 6; numberOfHits++)
            {
                var totalPowerNeeded = (_enemy.maxHP / numberOfHits) + (_enemy.defense / 2); // divide by 2 taken from PlayerController.idleAttack()
                var atp_pct = ((totalPowerNeeded / totalPowerWithoutATPowerBonus) - 1) * 100f; // -1 to convert from "multiplier" to "bonus" (for use in following formula)
                var atp = (float)Math.Pow(atp_pct / 10, 2.5); // https://ngu-idle.fandom.com/wiki/Advanced_Training#Formulas
                if (float.IsNaN(atp)) atp = 0f;
                
                atText += $"\n - {numberOfHits}-hit kill: ~{character.display(atp)}";

                if (atp < currentATPLevel) break;
            }

            ___message += atText;
            __instance.tooltip.showTooltip(___message);
        }
    }
}
