using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ToggleAdventureAdvancer
    {
        private static bool _disabled
        {
            get => Data.AdventureAdvancerDisabled;
            set => Data.AdventureAdvancerDisabled = value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "setNewMaxZone")]
        private static bool AdventureController_setNewMaxZone_prefix(AdventureController __instance)
        {
            if (!Plugin.ShiftIsDown)
                return true;

            _disabled = !_disabled;
            __instance.updateMenu();

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "updateMenu")]
        private static void AdventureController_updateMenu_postfix(AdventureController __instance)
        {
            var button = __instance.advAdvancerSetter;
            if (!button.isActiveAndEnabled)
                return;

            button.image.color = _disabled ? Plugin.ButtonColor_Red : Color.white;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "Update")]
        private static IEnumerable<CodeInstruction> AdventureController_Update_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var goToMaxZone = typeof(ZoneForwardClick).GetMethod("goToMaxZone", [typeof(int)]);

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, goToMaxZone))
                .Advance(-6)
                .RemoveInstructions(7)
                .Insert(Transpilers.EmitDelegate(gotoAdvancerZone));

            return cm.InstructionEnumeration();
        }

        private static void gotoAdvancerZone()
        {
            if (_disabled)
                return;

            Plugin.Character.adventureController.zoneForward.goToMaxZone(Plugin.Character.arbitrary.advAdvancerZone);
        }
    }
}
