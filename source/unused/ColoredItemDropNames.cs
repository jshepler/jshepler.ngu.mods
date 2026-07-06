using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ColoredItemDropNames
    {
        [HarmonyPostfix,
            HarmonyPatch(typeof(ItemNameDesc), "makeLoot", [typeof(int)]),
            HarmonyPatch(typeof(ItemNameDesc), "makeLevelledLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLevelledLoot")]
        private static void ItemNameDesc_makeLoot_postfix(int id, ref string __result)
        {
            var index = 0;

            // when LootDrop spits out quest item names, it does a Substring(40) to skip over "<b><color=blue>[QUEST ITEM]</color></b>\n"
            if ((id >= 278 && id <= 287))
                index = 40;

            var newResult = __result.Insert(index, $"<b><color={Options.Colors.LootItemNames.Value}>") + " </color></b>";
            __result = newResult;
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "dropMacguffin"),
            HarmonyPatch(typeof(LootDrop), "dropRandomMacguffin", [typeof(string), typeof(int)])]
        private static IEnumerable<CodeInstruction> LootDrop_dropMacguffin_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var match1 = " also dropped ";
            var replace1 = $" also dropped <b><color={Options.Colors.LootItemNames.Value}>";

            var match2 = " Power Macguffin Fragment";
            var replace2 = " Power Macguffin Fragment</color></b>";

            var match3 = " Cap Macguffin Fragment";
            var replace3 = " Cap Macguffin Fragment</color></b>";

            var match4 = " Bar Macguffin Fragment";
            var replace4 = " Bar Macguffin Fragment</color></b>";

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1))
                .SetOperandAndAdvance(replace1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match2))
                .SetOperandAndAdvance(replace2)

                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1))
                .SetOperandAndAdvance(replace1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match3))
                .SetOperandAndAdvance(replace3)

                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1))
                .SetOperandAndAdvance(replace1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, match4))
                .SetOperandAndAdvance(replace4);

            return cm.InstructionEnumeration();
        }
    }
}
