using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DPS
    {
        private static float _startTime;
        private static float _totalDamage;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "spawnEnemy")]
        private static void AdventureController_spawnEnemy_postfix()
        {
            _startTime = Time.realtimeSinceStartup;
            _totalDamage = 0;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(AdventureController), "enemyDeath"),
            HarmonyPatch(typeof(AdventureController), "playerDeath")]
        private static void AdventureController_enemyDeath_postfix(AdventureController __instance)
        {
            var killTime = Time.realtimeSinceStartup - _startTime;
            var dps = _totalDamage / killTime;
            var display = Plugin.Character.display;

            __instance.log.AddEvent($"You did {display(_totalDamage)} damage over {NumberOutput.timeOutput(killTime)} ({display(dps)} DPS)");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "takeDamage")]
        private static IEnumerable<CodeInstruction> EnemyAI_takeDamage_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var curHP = typeof(Enemy).GetField("curHP");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curHP))
                .Advance(2)
                .Insert(Transpilers.EmitDelegate(trackDamage));

            return cm.InstructionEnumeration();
        }

        private static float trackDamage(float damage)
        {
            _totalDamage += damage;
            return damage;
        }
    }
}
