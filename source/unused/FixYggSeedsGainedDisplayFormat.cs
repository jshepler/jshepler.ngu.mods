using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch(typeof(FruitController))]
    internal class FixYggSeedsGainedDisplayFormat
    {
        private static MethodInfo _characterDisplay = typeof(Character).GetMethod("display", new[] { typeof(double) });
        private static FieldInfo _characterField = typeof(FruitController).GetField("character");

        [HarmonyTranspiler, HarmonyPatch("harvest", typeof(int))]
        private static IEnumerable<CodeInstruction> harvest(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You gained "))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay))
                .InstructionEnumeration();

            return newInstructions;
        }

        [HarmonyTranspiler, HarmonyPatch("consumeGoldFruit")]
        private static IEnumerable<CodeInstruction> consumeGoldFruit(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, " Gold and "))
                .Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay))
                .InstructionEnumeration();

            return newInstructions;
        }

        [HarmonyTranspiler, HarmonyPatch("consumePowerFruit")]
        private static IEnumerable<CodeInstruction> consumePowerFruit(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "%</b>.You've also gained "))
                .Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay))
                .InstructionEnumeration();

            return newInstructions;
        }

        [HarmonyTranspiler, HarmonyPatch("consumeAPFruit")]
        private static IEnumerable<CodeInstruction> consumeAPFruit(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, " AP and "))
                .Advance(-5)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay))
                .InstructionEnumeration();

            return newInstructions;
        }
    }
}
