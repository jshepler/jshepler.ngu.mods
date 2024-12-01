using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TraitorHelper
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "displayEnemyStats")]
        private static void AdventureController_updateAdventureStats_postfix(AdventureController __instance)
        {
            if (Hardcore.IsHardcoreGame)
                return;

            var ai = __instance.enemyAI;
            if (!__instance.character.InMenu(Menu.Adventure)
                || __instance.currentEnemy == null
                || !ai.isTraitor())
                return;

            var color = ai.invincibleCount > 0 ? "#d82e3f" : "#28cc2d";
            __instance.enemyStats.text += $"\n<b><color={color}>IC: {ai.invincibleCount}</color> | <color=blue>GR: {ai.growRate}</color> | <color=blue>GC: {ai.growCount}</color></b>";
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "Start")]
        private static IEnumerable<CodeInstruction> AdventureController_Start_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.5f))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.5f))
                .SetOperandAndAdvance(0.2f);

            return cm.InstructionEnumeration();
        }
    }
}
