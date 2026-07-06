using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShowHighestBossNumberInTimeMachine
    {
        private static Character _character;

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            _character = __instance.character;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(TimeMachineController), "updateMachineText")]
        private static IEnumerable<CodeInstruction> TimeMachineController_updateMachineText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var step = 0;
            var skipInstructions = false;

            foreach (var i in instructions)
            {
                switch (step)
                {
                    case 0:
                        if (i.opcode == OpCodes.Ldstr && (string)i.operand == "<b>Highest Boss Multiplier:</b> ")
                        {
                            step++;
                        }
                        break;

                    case 1:
                        if (i.opcode == OpCodes.Ldarg_0)
                        {
                            skipInstructions = true;
                            step++;
                        }
                        break;

                    case 2:
                        if (i.opcode == OpCodes.Stelem_Ref)
                        {
                            yield return Transpilers.EmitDelegate(() => $"{_character.machineBossMulti()} (boss #{_character.highestBoss})");
                            skipInstructions = false;
                            step++;
                        }
                        break;
                }

                if (skipInstructions) continue;

                yield return i;
            }
        }
    }
}

/*
    I did this mostly to learn about using a transpiler patch to inject a delegate call. The TimeMachineController.updateMachineText() method
is where the "Highest Boss Multiplier" is displayed, but it's in the middle of many strings being created and displayed so a simple postfix patch
isn't very practical - would have to copy/paste the entire method, changing the piece that I want. The objective was to append the boss number to
the end of highest boss multiplier because the multiplier doesn't match the highest boss and instead of always having to go look at stats to see
the max boss number, just wanted to show it here.

    Anyway, it seemed like a good opportunity for another transpiler patch. My thought was to change the string concatenation below and replace
the call to character.machineBossMulti() with an anonymous method that returns the string "{multiplier} (boss #{highestBoss})". Looking at the CIL
below, I decided to replace IL_011f to IL_012a with the call to my anonymous method, using HarmonyX's Transpilers.EmitDelegate() [https://github.com/BepInEx/HarmonyX/wiki/Transpiler-helpers#emitdelegate]

    My plan was to emit all instructions until the first ldarg.0 after ldstr "<b>Highest Boss Multiplier:</b> ", skip instructions until stelem.ref,
emit the delegate, emit the stelem.ref and the remaining instructions.

    ...

	// text2 = "<b>Highest Boss Multiplier:</b> " + character.machineBossMulti() + "\n<b>Gold Multiplier:</b> " + character.display(1 + character.machine.levelGoldMulti);
	IL_010f: ldc.i4.4
	IL_0110: newarr [mscorlib]System.Object
	IL_0115: dup
	IL_0116: ldc.i4.0
	IL_0117: ldstr "<b>Highest Boss Multiplier:</b> "
	IL_011c: stelem.ref
	IL_011d: dup
	IL_011e: ldc.i4.1
--------------------------
	IL_011f: ldarg.0
	IL_0120: ldfld class Character TimeMachineController::character
	IL_0125: callvirt instance int64 Character::machineBossMulti()
	IL_012a: box [mscorlib]System.Int64
--------------------------
	IL_012f: stelem.ref
	IL_0130: dup
	IL_0131: ldc.i4.2
	IL_0132: ldstr "\n<b>Gold Multiplier:</b> "
	IL_0137: stelem.ref
	IL_0138: dup
	IL_0139: ldc.i4.3
	IL_013a: ldarg.0
	IL_013b: ldfld class Character TimeMachineController::character
	IL_0140: ldc.i4.1
	IL_0141: conv.i8
	IL_0142: ldarg.0
	IL_0143: ldfld class Character TimeMachineController::character
	IL_0148: ldfld class TimeMachine Character::machine
	IL_014d: ldfld int64 TimeMachine::levelGoldMulti
	IL_0152: add
	IL_0153: conv.r8
	IL_0154: callvirt instance string Character::display(float64)
	IL_0159: stelem.ref
	IL_015a: call string [mscorlib]System.String::Concat(object[])
	IL_015f: stloc.1

    ...

 */