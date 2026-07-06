using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// changes the displayed current values to be the (reduced) effective values,
// like what happens in normal when an item is reduced due to being on low bosses
namespace jshepler.ngu.mods.ItemTooltips
{
    [HarmonyPatch]
    internal class Weap2EffectiveValues
    {
        private static bool _isWeap2 = false;

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(int)])]
        private static void InventoryController_itemTooltipText_prefix(int id)
        {
            _isWeap2 = id == -6;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(int)])]
        private static void InventoryController_itemTooltipText_postfix()
        {
            _isWeap2 = false;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(Equipment)])]
        private static IEnumerable<CodeInstruction> InventoryController_itemTooltipText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var toe = typeof(Equipment);
            var curAttack = toe.GetField("curAttack");
            var curDefense = toe.GetField("curDefense");
            var spec1Cur = toe.GetField("spec1Cur");
            var spec2Cur = toe.GetField("spec2Cur");
            var spec3Cur = toe.GetField("spec3Cur");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curAttack))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curDefense))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec1Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec2Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec3Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value));

            return cm.InstructionEnumeration();
        }

        private static float effectiveWeap2Value(float value)
        {
            var effectiveness = Plugin.Character.inventoryController.weapon2Factor();
            return _isWeap2 ? value * effectiveness : value;
        }
    }
}
