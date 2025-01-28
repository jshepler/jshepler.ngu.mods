using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TitanBossRequired
    {
        [HarmonyTranspiler, HarmonyPriority(1), HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        private static IEnumerable<CodeInstruction> ButtonShower_showTitanTimer_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)

                // T1 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 58), Transpilers.EmitDelegate(insertBoss))

                // T1 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 58), Transpilers.EmitDelegate(insertBoss))

                // T2 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 66), Transpilers.EmitDelegate(insertBoss))

                // T2 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 66), Transpilers.EmitDelegate(insertBoss))

                // T3 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 82), Transpilers.EmitDelegate(insertBoss))

                // T3 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 82), Transpilers.EmitDelegate(insertBoss))

                // T4 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 100), Transpilers.EmitDelegate(insertBoss))

                // T4 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 100), Transpilers.EmitDelegate(insertBoss))

                // T5 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 116), Transpilers.EmitDelegate(insertBoss))

                // T5 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 116), Transpilers.EmitDelegate(insertBoss))

                // T6 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 132), Transpilers.EmitDelegate(insertBoss))

                // T6 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 132), Transpilers.EmitDelegate(insertBoss))

                // T7 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 426), Transpilers.EmitDelegate(insertBoss))

                // T7 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 426), Transpilers.EmitDelegate(insertBoss))

                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)

                // T8 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 467), Transpilers.EmitDelegate(insertBoss))

                // T8 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 467), Transpilers.EmitDelegate(insertBoss))

                // T9 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 491), Transpilers.EmitDelegate(insertBoss))

                // T9 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 491), Transpilers.EmitDelegate(insertBoss))

                // T10 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 777), Transpilers.EmitDelegate(insertBoss))

                // T10 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 777), Transpilers.EmitDelegate(insertBoss))

                // T11 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 826), Transpilers.EmitDelegate(insertBoss))

                // T11 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 826), Transpilers.EmitDelegate(insertBoss))

                // T12 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 848), Transpilers.EmitDelegate(insertBoss))

                // T12 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 848), Transpilers.EmitDelegate(insertBoss))

                // T13 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 897), Transpilers.EmitDelegate(insertBoss))

                // T13 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 897), Transpilers.EmitDelegate(insertBoss))

                // T14 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 902), Transpilers.EmitDelegate(insertBoss))

                // T14 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 902), Transpilers.EmitDelegate(insertBoss));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static string insertBoss(string s, int effectiveBossId)
        {
            var bossNumber = effectiveBossId;
            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    if (effectiveBossId > 301)
                        return s;
                    break;

                case difficulty.evil:
                    if (effectiveBossId > 602 || effectiveBossId <= 301)
                        return s;

                    bossNumber = effectiveBossId - 301;
                    break;

                case difficulty.sadistic:
                    if (effectiveBossId <= 602)
                        return s;

                    bossNumber = effectiveBossId - 602;
                    break;
            }

            var unlocked = Plugin.Character.effectiveBossID() >= effectiveBossId;
            var pos = s.IndexOf('<');
            s = s.Insert(pos, $"<color={(unlocked ? "green" : "red")}>{bossNumber}</color>: ");

            return s;
        }
    }
}
