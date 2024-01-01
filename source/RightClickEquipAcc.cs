using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RightClickEquipAcc
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(ItemController), "autoEquip")]
        private static IEnumerable<CodeInstruction> ItemController_autoEquip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var item1 = typeof(Inventory).GetField("item1");

            var cm = new CodeMatcher(instructions)
                .End()
                .MatchBack(false
                    , new CodeMatch(OpCodes.Ldc_I4, 10000)
                    , new CodeMatch(OpCodes.Stfld, item1))
                .SetInstruction(Transpilers.EmitDelegate(FirstEmptyAcc));

            return cm.InstructionEnumeration();
        }

        private static int FirstEmptyAcc()
        {
            var accs = Plugin.Character.inventory.accs;

            for (var x = 0; x < accs.Count; x++)
            {
                if (accs[x].id == 0)
                    return x + 10000;
            }

            return 10000;
        }
    }
}
