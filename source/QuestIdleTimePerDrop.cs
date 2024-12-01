using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestIdleTimePerDrop
    {
        private static bool _altIsDown = true;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.Character.InMenu(Menu.Quests))
                    return;

                _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "showIdleModeTooltip")]
        private static IEnumerable<CodeInstruction> BeastQuestController_showIdleModeTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var stringConcat3 = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string)]);
            var stringConcat4 = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string), typeof(string)]);

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n\nTime until next drop is added to Quest: "))
                .InsertAndAdvance(Transpilers.EmitDelegate(getTimePerDrop))
                .SetOperandAndAdvance("\nTime until next drop is added to Quest: ")
                .MatchForward(false, new CodeMatch(OpCodes.Call, stringConcat3))
                .SetOperandAndAdvance(stringConcat4);

            return cm.InstructionEnumeration();
        }

        private static string getTimePerDrop()
        {
            var character = Plugin.Character;
            var ppt = character.beastQuestController.idleProgressPerTick();

            // should never be 0, but to be safe...
            if (ppt == 0)
                return string.Empty;

            var secondsPerDrop = 1f / ppt / 50f;
            var secondsForQuest = secondsPerDrop * character.beastQuest.targetDrops;
            var avgDropsPerQuest = character.adventure.itopod.perkLevel[94] >= 610 ? 50 : 55;
            var avgQuestsPerDay = 86400f / (secondsPerDrop * avgDropsPerQuest);

            var timePerDrop = NumberOutput.timeOutput(secondsPerDrop);
            var totalTime = NumberOutput.timeOutput(secondsForQuest);

            var speed = character.inventory.itemList.redLiquidComplete ? 0.8f : 1.0f;
            var respawn = character.adventureController.respawnTime();
            var questDC = character.beastQuestController.questDropChance();
            var idleDF = character.beastQuestController.idleDropFactor();
            var seconds = (speed + respawn) / questDC * idleDF;

            var altText = !_altIsDown ? string.Empty :
                $"\n   idle attack speed: {speed}"
                + $"\n   respawn: {respawn}"
                + $"\n   quest DC: {questDC}"
                + $"\n   idle speed divider (perks): {idleDF}"
                + $"\n   seconds per drop"
                + $"\n      = (speed + respawn) / questDC * divider"
                + $"\n      = {seconds} ({NumberOutput.timeOutput(seconds)})\n";

            var text = $"\n\nTime per drop: {timePerDrop}"
                + altText
                + $"\nTotal quest time: {totalTime}"
                + $"\nAverage quests per day: {avgQuestsPerDay:0.#}\n";

            return text;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "timeToNextFill")]
        private static void BeastQuestController_timeToNextFill_postfix(BeastQuestController __instance, ref string __result)
        {
            var character = __instance.character;
            var ppt = character.beastQuestController.idleProgressPerTick();

            // should never be 0, but to be safe...
            if (ppt == 0f)
                return;

            var secondsPerDrop = 1f / ppt / 50f;
            var dropsLeft = character.beastQuest.targetDrops - character.beastQuest.curDrops;
            var secondsLeft = dropsLeft * secondsPerDrop;

            var curProgress = character.beastQuest.idleProgress;
            if (curProgress > 0)
                secondsLeft -= curProgress * secondsPerDrop;

            __result += $"\nTime to completed quest: {NumberOutput.timeOutput(secondsLeft)}";
        }
    }
}
