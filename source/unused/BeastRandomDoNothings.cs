using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BeastRandomDoNothings
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "doAura")]
        private static IEnumerable<CodeInstruction> EnemyAI_doAura_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Switch))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Switch))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    Transpilers.EmitDelegate(randomDoNothing));

            return cm.InstructionEnumeration();
        }

        private static void randomDoNothing(EnemyAI ai)
        {
            var text = string.Format(_messages[UnityEngine.Random.Range(0, _messages.Count)], ai.ac.currentEnemy.name);
            ai.log.AddEvent(text, 2);
        }

        private static List<string> _messages =
        [
            "{0} forgot what skill it was going to use. It's your turn now.",
            "{0} tried to poison you with a toxic fart, but it failed to do so.",
            "{0} has run out of mana so no spell is cast.",
            "{0} forgot whatever move it was going to do.",
            "{0} performs a distraction dance. It's not very effective.",
            "{0} heard a fly around its ear and attacked it for 0 damage!",
            "{0} is depressed and doesn't want to use a special move at this time.",
            "{0} is an unpaid worker and goes on strike.",
            "{0} recalled its favorite childhood show and got emotional.",
            "{0} sneezed for 0 damage!",
            "{0} is busy watching TikTok, try again later.",
            "{0} says, \"Check back in 10 moves for a new special attack\"."
        ];
    }
}
