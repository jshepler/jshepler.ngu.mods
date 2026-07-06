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
                .Insert(Transpilers.EmitDelegate(AddQuestsPerDay))

                // adding an extra line break so the text is more clean
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<b>\nThis Quest is currently worth "))
                .SetOperandAndAdvance("<b>\n\nThis Quest is currently worth ")

                // removing "a hard worker and " shortens the string enough to not wrap and saves a line, offsetting that exta line break
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "% rewards because you're a hard worker and haven't used Idle Mode!</b>"))
                .SetOperandAndAdvance("% rewards because you haven't used Idle Mode!</b>");

            return cm.InstructionEnumeration();
        }

        private static string AddQuestsPerDay(string text)
        {
            var character = Plugin.Character;
            var secondsPerQuest = (float)character.beastQuestController.timerThreshold();
            var questsPerDay = 86400f / secondsPerQuest;

            var maxMajors = character.beastQuestController.maxBankedQuests();
            var curMajors = character.beastQuest.curBankedQuests;
            var secondsUntilMax = curMajors >= maxMajors ? 0f : (maxMajors - curMajors) * secondsPerQuest - character.beastQuest.dailyQuestTimer.totalseconds;

            text += $"\n<b>Time per Major Quest:</b> {NumberOutput.timeOutput(secondsPerQuest)}"
                + $"\n<b>Major Quests per Day:</b> {questsPerDay}"
                + $"\n<b>Time to Full Bank:</b> {NumberOutput.timeOutput(secondsUntilMax)}";

            var banked = character.beastQuest.curBankedQuests;
            if (banked > 0)
            {
                var respawnTime = character.adventureController.respawnTime();
                var idleAttackSpeed = character.adventure.attackSpeed;
                var secondsPerKill = respawnTime + idleAttackSpeed;
                var avgItems = character.adventure.itopod.perkLevel[94] >= 610 ? 50 : 55;
                var dropChance = character.beastQuestController.questDropChance();
                var bankedSeconds = (banked * avgItems / dropChance) * secondsPerKill;

                text += $"\n<b>Est. Time to Complete Cur Bank:</b> {NumberOutput.timeOutput(bankedSeconds)}";
            }

            return text;
        }
    }
}
