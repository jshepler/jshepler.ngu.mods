using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WalderpDoubleExplosionBugfix
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "takeDamage")]
        private static IEnumerable<CodeInstruction> EnemyAI_takeDamage_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var explode = typeof(EnemyAI).GetMethod("explode", BindingFlags.NonPublic | BindingFlags.Instance);
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, explode))
                .Advance(1)
                .RemoveInstruction();

            return cm.InstructionEnumeration();//.DumpToLog();
        }
    }
}

/*
the bug is when doing the wrong attack, the flag that indicates player being in the walderp says state isn't reset so
on walderp's next attack, the code will do the explosion as if player didn't do any attack in time

this block:

if (inWaldoSaysLoop)
{
	if (!didCorrectWaldoMove())
	{
		if (waldoSays)
		{
			log.AddEvent("The demented hiker cackles, 'YOU DIDN'T DO WHAT WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
			log.AddEvent("He pulls out a big red button and presses it.");
		}
		else
		{
			log.AddEvent("The demented hiker cackles, 'I DIDN'T SAY WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
			log.AddEvent("He pulls out a big red button and presses it.");
		}
		explode();
	}
	else
	{
		waldoAttackID = 0;
		lastAttackID = 0;
		inWaldoSaysLoop = false;
	}
}


becomes:

if (inWaldoSaysLoop)
{
	if (!didCorrectWaldoMove())
	{
		if (waldoSays)
		{
			log.AddEvent("The demented hiker cackles, 'YOU DIDN'T DO WHAT WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
			log.AddEvent("He pulls out a big red button and presses it.");
		}
		else
		{
			log.AddEvent("The demented hiker cackles, 'I DIDN'T SAY WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
			log.AddEvent("He pulls out a big red button and presses it.");
		}
		explode();
	}

// removed the else
	waldoAttackID = 0;
	lastAttackID = 0;
	inWaldoSaysLoop = false;
}


the IL:
	// if (!didCorrectWaldoMove())
	IL_0076: ldarg.0
	IL_0077: call instance bool EnemyAI::didCorrectWaldoMove()
	IL_007c: brtrue.s IL_00d0

	// if (waldoSays)
	IL_007e: ldarg.0
	IL_007f: ldfld bool EnemyAI::waldoSays
	IL_0084: brfalse.s IL_00a8

	// log.AddEvent("The demented hiker cackles, 'YOU DIDN'T DO WHAT WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
	IL_0086: ldarg.0
	IL_0087: ldfld class PlayerLog EnemyAI::log
	IL_008c: ldstr "The demented hiker cackles, 'YOU DIDN'T DO WHAT WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'"
	IL_0091: callvirt instance void PlayerLog::AddEvent(string)

	// log.AddEvent("He pulls out a big red button and presses it.");
	IL_0096: ldarg.0
	IL_0097: ldfld class PlayerLog EnemyAI::log
	IL_009c: ldstr "He pulls out a big red button and presses it."
	IL_00a1: callvirt instance void PlayerLog::AddEvent(string)
	IL_00a6: br.s IL_00c8

	// log.AddEvent("The demented hiker cackles, 'I DIDN'T SAY WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'");
	IL_00a8: ldarg.0
	IL_00a9: ldfld class PlayerLog EnemyAI::log
	IL_00ae: ldstr "The demented hiker cackles, 'I DIDN'T SAY WALDERP SAYS, SUCKER! TIME TO DIEEEEEEEEE!'"
	IL_00b3: callvirt instance void PlayerLog::AddEvent(string)

	// log.AddEvent("He pulls out a big red button and presses it.");
	IL_00b8: ldarg.0
	IL_00b9: ldfld class PlayerLog EnemyAI::log
	IL_00be: ldstr "He pulls out a big red button and presses it."
	IL_00c3: callvirt instance void PlayerLog::AddEvent(string)

	// explode();
	IL_00c8: ldarg.0
	IL_00c9: call instance void EnemyAI::explode()

	IL_00ce: br.s IL_00e5	<-- removed this branch

	// waldoAttackID = 0;
	IL_00d0: ldarg.0
	IL_00d1: ldc.i4.0
	IL_00d2: stfld int32 EnemyAI::waldoAttackID

	// lastAttackID = 0;
	IL_00d7: ldarg.0
	IL_00d8: ldc.i4.0
	IL_00d9: stfld int32 EnemyAI::lastAttackID

	// inWaldoSaysLoop = false;
	IL_00de: ldarg.0
	IL_00df: ldc.i4.0
	IL_00e0: stfld bool EnemyAI::inWaldoSaysLoop

	// if (isBeast() && auraID == 5)
	IL_00e5: ldarg.0
	IL_00e6: call instance bool EnemyAI::isBeast()

 */