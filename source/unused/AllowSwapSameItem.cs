using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AllowSwapSameItem
    {
        private static MethodInfo _alreadyEquipped = typeof(InventoryController).GetMethod("alreadyEquipped", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyTranspiler, HarmonyPatch(typeof(InventoryController), "swapAcc")]
        private static IEnumerable<CodeInstruction> InventoryController_swapAcc_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _alreadyEquipped))
                .SetInstruction(Transpilers.EmitDelegate(alreadyEquppedAllowSameItem));

            return cm.InstructionEnumeration();
        }

        private static bool alreadyEquppedAllowSameItem(InventoryController controller, int invItemId)
        {
            var inventory = controller.character.inventory;
            var accItemId = inventory.accs[controller.accessoryID(inventory.item1)].id;
            
            if (accItemId == invItemId)
                return false;

            return (bool)_alreadyEquipped.Invoke(controller, [invItemId]);
        }
    }
}

/*
	IL_03d6: ldarg.0
	IL_03d7: ldarg.0
	IL_03d8: ldfld class Character InventoryController::character
	IL_03dd: ldfld class Inventory Character::inventory
	IL_03e2: ldfld class [mscorlib]System.Collections.Generic.List`1<class Equipment> Inventory::inventory
	IL_03e7: ldloc.1
	IL_03e8: callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<class Equipment>::get_Item(int32)
	IL_03ed: ldfld int32 Equipment::id

at this point, the stack has a ref to the instance of InventoryController (ldarg.0)
and the item id of the item being swapped in (ldloc.1)

	IL_03f2: call instance bool InventoryController::alreadyEquipped(int32)

replacing the call from Inventory::alreadyEquipped to my static method above makes the InventoryController reference the first parameter

    call static bool jshepler.ngu.mods.AllowSwapSameItem::alreadyEquppedAllowSameItem(InventoryController, int32)
 */