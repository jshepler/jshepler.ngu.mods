using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StatsBreakdown_NGU
    {
        // the perks modifiers don't include "Welcome to Sadistic" perk
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayNGU")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayNGU_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var totalEnergyNGUBonus = typeof(ItopodPerkController).GetMethod("totalEnergyNGUBonus");
            var totalMagicNGUBonus = typeof(ItopodPerkController).GetMethod("totalMagicNGUBonus");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, totalEnergyNGUBonus))
                .Advance(-4)
                .RemoveInstructions(5)
                .Insert(new CodeInstruction(Transpilers.EmitDelegate(getEnergyPerkBonus)))
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, totalMagicNGUBonus))
                .Advance(-4)
                .RemoveInstructions(5)
                .Insert(new CodeInstruction(Transpilers.EmitDelegate(getMagicPerkBonus)));

            return cm.InstructionEnumeration();
        }

        private static float getEnergyPerkBonus()
        {
            var bonus = Plugin.Character.adventureController.itopod.totalEnergyNGUBonus();

            if (Plugin.Character.settings.rebirthDifficulty >= difficulty.sadistic
                && Plugin.Character.adventure.itopod.perkLevel[144] >= 1)
            {
                bonus *= 1.2f;
            }

            return bonus;
        }

        private static float getMagicPerkBonus()
        {
            var bonus = Plugin.Character.adventureController.itopod.totalMagicNGUBonus();

            if (Plugin.Character.settings.rebirthDifficulty >= difficulty.sadistic
                && Plugin.Character.adventure.itopod.perkLevel[144] >= 1)
            {
                bonus *= 1.2f;
            }

            return bonus;
        }
    }
}
