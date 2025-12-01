using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// in vanilla, online titan AKs aren't counted for "Titans Defeated" on the misc stats screen
namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class IncreaseTitansDefeatedForOnlineAK
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "manageFight")]
        private static IEnumerable<CodeInstruction> AdventureController_manageFight_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var addKills = typeof(BestiaryController).GetMethod("addKills");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, addKills))
                .Repeat(m => m.Advance(1).Insert(Transpilers.EmitDelegate(incTitansDefeated)));

            return cm.InstructionEnumeration();
        }

        private static void incTitansDefeated()
        {
            Plugin.Character.stats.titansDefeated++;
        }
    }
}
