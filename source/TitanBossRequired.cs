using System.Collections.Generic;
using System.Linq;
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
            var bosses = GameData.TitanAK.Requirements.Select(req => req.effectiveBossId).Distinct().ToArray();
            var cm = new CodeMatcher(instructions)
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)

                // T1 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[0]), Transpilers.EmitDelegate(insertBoss))

                // T1 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[0]), Transpilers.EmitDelegate(insertBoss))

                // T2 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[1]), Transpilers.EmitDelegate(insertBoss))

                // T2 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[1]), Transpilers.EmitDelegate(insertBoss))

                // T3 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[2]), Transpilers.EmitDelegate(insertBoss))

                // T3 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[2]), Transpilers.EmitDelegate(insertBoss))

                // T4 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[3]), Transpilers.EmitDelegate(insertBoss))

                // T4 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[3]), Transpilers.EmitDelegate(insertBoss))

                // T5 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[4]), Transpilers.EmitDelegate(insertBoss))

                // T5 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[4]), Transpilers.EmitDelegate(insertBoss))

                // T6 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[5]), Transpilers.EmitDelegate(insertBoss))

                // T6 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[5]), Transpilers.EmitDelegate(insertBoss))

                // T7 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[6]), Transpilers.EmitDelegate(insertBoss))

                // T7 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[6]), Transpilers.EmitDelegate(insertBoss))

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
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[7]), Transpilers.EmitDelegate(insertBoss))

                // T8 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[7]), Transpilers.EmitDelegate(insertBoss))

                // T9 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[8]), Transpilers.EmitDelegate(insertBoss))

                // T9 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[8]), Transpilers.EmitDelegate(insertBoss))

                // T10 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[9]), Transpilers.EmitDelegate(insertBoss))

                // T10 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[9]), Transpilers.EmitDelegate(insertBoss))

                // T11 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[10]), Transpilers.EmitDelegate(insertBoss))

                // T11 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[10]), Transpilers.EmitDelegate(insertBoss))

                // fixes vanilla bug that was showing T12 starting at boss 246 instead of 248
                .SearchForward(i => i.opcode == OpCodes.Ldc_I4 && (int)i.operand == 848)
                .SetOperandAndAdvance(bosses[11])

                // T12 - ready
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[11]), Transpilers.EmitDelegate(insertBoss))

                // T12 - time until
                .SearchForward(i => i.opcode == OpCodes.Ldstr)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, bosses[11]), Transpilers.EmitDelegate(insertBoss))

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
