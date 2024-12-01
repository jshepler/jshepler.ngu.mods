using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ExploderHelper
    {
        private static int _lastSeconds;
        private static bool _saveLoaded = false;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnOfflineProgressionComplete += (o, e) => _saveLoaded = true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EnemyAI), "Update")]
        private static void EnemyAI_Update_prefix(EnemyAI __instance, float ___enemyAttackTimer, bool ___firstStrike)
        {
            if (Hardcore.IsHardcoreGame)
                return;

            var currentEnemy = __instance.ac.currentEnemy;
            if (currentEnemy == null || currentEnemy.AI != AI.exploder)
            {
                if (_saveLoaded)
                    _saveLoaded = false;

                return;
            }

            var ar = currentEnemy.attackRate * (___firstStrike ? 1.5f : 1f);

            if (_saveLoaded)
            {
                _lastSeconds = Mathf.CeilToInt(ar - ___enemyAttackTimer);
                _saveLoaded = false;
            }

            if (___enemyAttackTimer == 0)
                _lastSeconds = Mathf.CeilToInt(ar);

            var secondsLeft = Mathf.CeilToInt(ar - ___enemyAttackTimer);
            if (secondsLeft < _lastSeconds)
            {
                _lastSeconds = secondsLeft;

                if(secondsLeft < 3)
                    __instance.log.AddEvent($"Explodes in: {secondsLeft}", 3);
            }
        }
    }
}
