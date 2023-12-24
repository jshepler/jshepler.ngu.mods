using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FixTargets
    {
        // removes "target.text = character.advancedTraining.levelTarget[id].ToString();" from AdvancedTrainingController.updateText()
        // because it's called every time bar completes, interrupting user trying to enter a number

        // I wanted to remove from the first ldrg.0 to the callvirt at the end, but the first ldarg.0 has
        // a label that's used in a branch earlier in the code, so need to keep it, but can delete the ldarg.0 following callvirt
        //
        // ...
        //  ldarg.0 // has label used by a branch earlier in the code, can't remove this instruction without dealing with the label
        //  ldfld     class [UnityEngine.UI] UnityEngine.UI.InputField AdvancedTrainingController::target
        //  ldarg.0
        //  ldfld     class Character AdvancedTrainingController::character
        //  ldfld     class AdvancedTraining Character::advancedTraining
        //  ldfld     int64[] AdvancedTraining::levelTarget
        //  ldarg.0
        //  ldfld     int32 AdvancedTrainingController::id
        //  ldelema   [mscorlib]System.Int64
        //  call      instance string[mscorlib] System.Int64::ToString()
        //  callvirt  instance void[UnityEngine.UI] UnityEngine.UI.InputField::set_text(string)
        //  ldarg.0 // this starts the next line of c# code, one option is to move the label above to here to the branch skips over what we don't want executed
        // ...

        [HarmonyTranspiler, HarmonyPatch(typeof(AdvancedTrainingController), "updateText")]
        private static IEnumerable<CodeInstruction> AdvancedTrainingController_updateText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetField = typeof(AdvancedTrainingController).GetField("target");
            var setTextMethod = typeof(InputField).GetMethod("set_text");

            // some examples of some of the ways this could be done...

            // example #1: use CodeMatcher to determine the start and end indexes of the block to remove
            //var cm = new CodeMatcher(instructions);

            //var start = cm
            //    .MatchForward(true
            //        , new CodeMatch(OpCodes.Ldarg_0)
            //        , new CodeMatch(OpCodes.Ldfld, targetField))
            //    .Pos;

            //var end = cm
            //    .MatchForward(true
            //        , new CodeMatch(OpCodes.Callvirt, setTextMethod)
            //        , new CodeMatch(OpCodes.Ldarg_0))
            //    .Pos;

            //return cm
            //    .RemoveInstructionsInRange(start, end)
            //    .InstructionEnumeration();


            // example #2: use a List, pretty much same as CodeMatcher - find start/end indexes of the block to remove
            //var codes = new List<CodeInstruction>(instructions);
            //var start = -1;
            //var end = -1;

            //for (var x = 0; x < codes.Count; x++)
            //{

            //    if (codes[x].LoadsField(targetField)) start = x;
            //    if (codes[x].Calls(setTextMethod))
            //    {
            //        end = x + 1; // include the ldarg.0 that follows
            //        break;
            //    }
            //}

            //codes.RemoveRange(start, end - start + 1); // +1 because, for ex. if start is 1 and end is 10, that's 10 codes to remove, not 9
            //return codes;

            // example #3: yield the instructions, skipping the ones we want to "remove";
            // I chose to use a kind of state machine to keep track of when to yield or skip
            //var state = 0;
            //foreach (var i in instructions)
            //{
            //    switch (state)
            //    {
            //        case 0: // yield instructions until ldfld, skipping ldfld
            //            if (i.LoadsField(targetField))
            //            {
            //                state++;
            //                continue;
            //            }
            //            break;

            //        case 1: // skip instructions until (and including) the callvirt
            //            if (i.Calls(setTextMethod)) state++;
            //            continue;

            //        case 2: // and skip the ldarg.0 following callvirt, then yield remaining instructions
            //            if (i.IsLdarg(0)) state++;
            //            continue;
            //    }

            //    yield return i;
            //}

            // example #4: instead of removing any code, we can move the label from that first ldarg.0 to the one after callvirt,
            // which would result in the earlier branch to basically skip over the line of code removed in the earlier examples.
            // the line of code remains but execution jumps over it
            var codes = instructions.ToList();
            CodeInstruction ldarg0 = null;

            for (var x = 0; x < codes.Count; x++)
            {
                if (ldarg0 == null && codes[x].LoadsField(targetField))
                {
                    ldarg0 = codes[x - 1];
                }
                else if (ldarg0 != null && codes[x].Calls(setTextMethod))
                {
                    ldarg0.MoveLabelsTo(codes[x + 1]);
                    break;
                }
            }

            return codes;
        }

        // the transpiler patch above removes/skips the code that sets the text of the target input box, which results
        // in the values not being set when save is loaded; we can do it in refresh() without issue
        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "refresh")]
        private static void AdvancedTrainingController_refresh_postfix(AdvancedTrainingController __instance)
        {
            if (!__instance.character.InMenu(Menu.AdvancedTraining)) return;

            if ((__instance.id == 3 || __instance.id == 4) && !__instance.character.settings.wandoos98On) return;

            __instance.target.text = __instance.character.advancedTraining.levelTarget[__instance.id].ToString();
        }
    }
}
