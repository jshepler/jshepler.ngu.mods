using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Perk34Fix
    {
        private static FieldInfo _perkLevel = typeof(ITOPOD).GetField("perkLevel");

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "zone6Drop"),
            HarmonyPatch(typeof(LootDrop), "zone8Drop"),
            HarmonyPatch(typeof(LootDrop), "zone11Drop"),
            HarmonyPatch(typeof(LootDrop), "zone14Drop"),
            HarmonyPatch(typeof(LootDrop), "zone16Drop"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop")]
        private static IEnumerable<CodeInstruction> LootDrop_zoneDrop_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _perkLevel))
                .MatchForward(false, new CodeMatch(OpCodes.Bge))
                .SetOpcodeAndAdvance(OpCodes.Bgt);

            return cm.InstructionEnumeration();
        }
    }
}
