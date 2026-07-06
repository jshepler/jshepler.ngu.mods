using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestManualTimeToComplete
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(ButtonShower), "showQuestStatus")]
        private static IEnumerable<CodeInstruction> ButtonShower_showQuestStatus_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_5))
                .SetOpcodeAndAdvance(OpCodes.Ldc_I4_6)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_5))
                .Advance(-2)
                .Insert(new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldc_I4_5),
                    Transpilers.EmitDelegate(buildTimeToCompletion),
                    new CodeInstruction(OpCodes.Stelem_Ref));

            return cm.InstructionEnumeration();
        }

        private static string buildTimeToCompletion()
        {
            var character = Plugin.Character;
            var quest = character.beastQuest;

            if (!quest.inQuest || quest.idleMode)
                return "\n";

            var respawnTime = character.adventureController.respawnTime();
            var idleAttackSpeed = character.adventure.attackSpeed;
            var secondsPerKill = respawnTime + idleAttackSpeed;
            var dropRemaining = quest.targetDrops - quest.curDrops;
            var dropChance = character.beastQuestController.questDropChance();
            var secondsRemaining = (dropRemaining / dropChance) * secondsPerKill;
            var text = $"\n<b>  ... est. time remaining:</b> {NumberOutput.timeOutput(secondsRemaining)}";

            var banked = character.beastQuest.curBankedQuests;
            if (banked > 0)
            {
                var avgItems = character.adventure.itopod.perkLevel[94] >= 610 ? 50 : 55;
                var bankedSeconds = (banked * avgItems / dropChance) * secondsPerKill;
                text += $"\n<b>  ... with rest of bank:</b> {NumberOutput.timeOutput(secondsRemaining + bankedSeconds)}";
            }

            return $"{text}\n";
        }
    }
}
