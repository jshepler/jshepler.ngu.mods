using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WalderpHideSeekBugFixes
    {
        // bug: offline progression advances walderp respawn timer even when walderp is currently hiding
        // fix: if hiding, change value passed to boss5Spawn.setTime() to 0
        [HarmonyTranspiler, HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> Character_adventureOfflineProgress_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var boss5Spawn = typeof(Adventure).GetField("boss5Spawn");
            var setTime = typeof(PlayerTime).GetMethod("setTime", [typeof(double)]);

            var cm = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, boss5Spawn),
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Callvirt, setTime))
                .Insert(Transpilers.EmitDelegate(zeroIfHiding));

            return cm.InstructionEnumeration();
        }

        private static double zeroIfHiding(double time)
        {
            if (Plugin.Character.adventure.waldoDefeats > Plugin.Character.adventure.waldoFinds)
                return 0.0;

            return time;
        }

        // bug: walderp is not assigned to a menu when killed until timer % 180 == 0, but timer inits to 0 and is incremented before check
        // fix: change check to if timer % 180 == 1
        [HarmonyTranspiler, HarmonyPatch(typeof(WaldoSaysUnlocker), "updateWaldo")]
        private static IEnumerable<CodeInstruction> WaldoSaysUnlocker_updateWaldo_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Rem))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                .SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            return cm.InstructionEnumeration();
        }

        // bug: when walderp is found, timer isn't reset so that when he's next killed,
        //      time resumes from where it was and would have not caused a menu assignment (i.e. is not in any menu for up to 3 mins)
        // fix: reset the timer to 0
        [HarmonyPostfix, HarmonyPatch(typeof(WaldoSaysUnlocker), "waldoOff")]
        private static void WaldoSaysUnlocker_waldoOff_postfix(WaldoSaysUnlocker __instance)
        {
            __instance.waldoTimer = 0;
        }
    }
}
