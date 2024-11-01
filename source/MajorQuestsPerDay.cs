using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class MajorQuestsPerDay
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static IEnumerable<CodeInstruction> BeastQuestController_updateText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var allActive = typeof(BeastQuest).GetField("allActive");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, allActive))
                .Advance(-4)
                .Insert(Transpilers.EmitDelegate(AddQuestsPerDay));

            return cm.InstructionEnumeration();
        }

        private static string AddQuestsPerDay(string text)
        {
            var secondsPerQuest = (float)Plugin.Character.beastQuestController.timerThreshold();
            var questsPerDay = 86400f / secondsPerQuest;

            return text
                + $"\n<b>Time per Major Quest:</b> {NumberOutput.timeOutput(secondsPerQuest)}"
                + $"\n<b>Major Quests per Day:</b> {questsPerDay}";
        }
    }
}
