using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class JakeLocustsWarning
    {
        // the code makes it look like the swarm attack is supposed to start the turn after the warning,
        // but it's actually starting the next frame - this also fixes it so that it will start on the next attack
        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "jakeAI")]
        private static IEnumerable<CodeInstruction> EnemyAI_jakeAI_transpiler(IEnumerable<CodeInstruction> instructinos)
        {
            var enemyAttackTimerField = typeof(EnemyAI).GetField("enemyAttackTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            var acField = typeof(EnemyAI).GetField("ac");
            var currentEnemyField = typeof(AdventureController).GetField("currentEnemy");
            var attackRateField = typeof(Enemy).GetField("attackRate");

            var cm = new CodeMatcher(instructinos)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_0))
                .Advance(1);

            var bge = cm.Instruction;

            cm.Advance(1)
                .Insert(new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, enemyAttackTimerField)
                    , new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, acField)
                    , new CodeInstruction(OpCodes.Ldfld, currentEnemyField)
                    , new CodeInstruction(OpCodes.Ldfld, attackRateField)
                    , new CodeInstruction(OpCodes.Ble_Un, bge.operand)
                    , new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldc_R4, 0.0f)
                    , new CodeInstruction(OpCodes.Stfld, enemyAttackTimerField))

                // sets warning text to blue
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, " opens his mouth unnaturally wide and shoots out 100,000 FREAKING LOCUSTS! INCOMING!!!"))
                .Advance(2)
                //.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_3));
                .SetInstruction(Transpilers.EmitDelegate(warningColor));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static int warningColor()
        {
            return Hardcore.IsHardcoreGame ? 2 : 3;
        }
    }
}

/*
    // if (enemyAttackTimer > ac.currentEneym.attackRate)
	IL_00e2: ldarg.0
	IL_00e3: ldfld float32 EnemyAI::enemyAttackTimer
	IL_00e8: ldarg.0
	IL_00e9: ldfld class AdventureController EnemyAI::ac
	IL_00ee: ldfld class Enemy AdventureController::currentEnemy
	IL_00f3: ldfld float32 Enemy::attackRate

	// enemyAttackTimer = 0f;
	IL_0082: ldarg.0
	IL_0083: ldc.r4 0.0
	IL_0088: stfld float32 EnemyAI::enemyAttackTimer
 */