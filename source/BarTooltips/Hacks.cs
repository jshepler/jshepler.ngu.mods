using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class Hacks
    {
        private static CapCalculators.HacksCalculator[] _calc;

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _calc = __instance.character.hacks.hacks.Select((h, i) => new CapCalculators.HacksCalculator(__instance, i)).ToArray();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "showTooltip", typeof(int))]
        private static void HacksController_showTooltip_postfix(int id, HacksController __instance)
        {
            if (id == 15)
                return;

            var character = __instance.character;
            var props = __instance.properties[id];
            var text = $"<b>{props.hackName}</b>\n{props.hackDesc}";

            var currentBonus = __instance.hackBonus(id);
            text += $"\n\n<b>Current Effect:</b> {character.display(currentBonus * 100f, 1)}%";

            var nextLevelBonus = __instance.hackBonus(id, 1);
            text += $"\n<b>Next Level:</b> {character.display(nextLevelBonus * 100f, 1)}%";

            var timeUntilNextLevel = __instance.timeLeft(id);
            text += $"\n<b>Time Until Next Level:</b> {timeUntilNextLevel}";

            var hack = character.hacks.hacks[id];
            if (hack.level < hack.target)
            {
                var r3 = hack.res3 > 0 ? hack.res3 : character.totalCapRes3();
                var secondsToTarget = GetSecondsToTarget(id);
                text += $"\n<b>Time to Target:</b> {NumberOutput.timeOutput(secondsToTarget)}";
            }

            var msGained = __instance.numMilestonesReached(id);
            text += $"\n\n<b>Milestones reached:</b> {msGained}";

            var reducerCount = GetReducerCount(character, id);
            var maxReducerCount = GetMaxReducerCount(character, id);
            text += $" (with {reducerCount}/{maxReducerCount} reducers)";

            var levelsToNext = __instance.levelsToNextMilestone(id);
            text += $"\n<b>Next Milestone in:</b> {levelsToNext} levels";

            var baseEffect = __instance.properties[id].baseEffectPerLevel;
            var levelEffect = 1f + hack.level * baseEffect;
            text += $"\n\n<b>Hack Effect (base):</b> {character.display(baseEffect * 100f, 1, 3)}%"
                + $"\n<b>Hack Effect (leveled):</b> {character.display(levelEffect * 100f, 1, 3)}%";

            var msBaseBonus = props.milestoneEffect;
            var msTotalBonus = Mathf.Pow(msBaseBonus, msGained);
            text += $"\n\n<b>Milestone bonus (base):</b> {character.display(msBaseBonus * 100f, 1)}%";
            text += $"\n<b>Milestone bonus (total):</b> {character.display(msTotalBonus * 100f, 1)}%";

            if (Input.GetKey(KeyCode.LeftAlt) == false)
            {
                __instance.tooltip.showTooltip(text);
                return;
            }

            var msThreshold = props.milestoneThreshold;

            for (var x = 0; x <= maxReducerCount; x++)
            {
                var lastGained = Mathf.FloorToInt(hack.level / (msThreshold - x));
                var lastBonus = Mathf.Pow(msBaseBonus, lastGained);
                text += $"\n   <b>@{x} reducers:</b> {lastGained} => {character.display(lastBonus * 100f, 1)}%";
            }

            __instance.tooltip.showTooltip(text);
        }

        internal static double GetSecondsToTarget(int id)
        {
            var character = Plugin.Character;
            var hack = character.hacks.hacks[id];
            var r3 = character.totalCapRes3();
            var seconds = _calc[id].TimeToTarget(hack.level, hack.target, r3, hack.progress);

            return seconds;
        }

        internal static long GetReducerCount(Character character, int id)
        {
            var qc = character.beastQuest;
            var pc = character.adventure.itopod;
            var wc = character.wishes;

            return id switch
            {
                0 => qc.quirkLevel[57],
                1 => pc.perkLevel[113],
                2 => qc.quirkLevel[175],
                3 => pc.perkLevel[217],
                4 => pc.perkLevel[218],
                5 => qc.quirkLevel[174],
                6 => pc.perkLevel[219],
                7 => pc.perkLevel[114],
                8 => wc.wishes[76].level,
                9 => pc.perkLevel[115],
                10 => qc.quirkLevel[59],
                11 => wc.wishes[77].level,
                12 => qc.quirkLevel[58],
                13 => wc.wishes[78].level,
                14 => qc.quirkLevel[60],
                _ => 0
            };
        }

        private static long GetMaxReducerCount(Character character, int id)
        {
            var qc = character.beastQuestPerkController;
            var pc = character.adventureController.itopod;
            var wc = character.wishesController;

            return id switch
            {
                0 => qc.maxLevel[57],
                1 => pc.maxLevel[113],
                2 => qc.maxLevel[175],
                3 => pc.maxLevel[217],
                4 => pc.maxLevel[218],
                5 => qc.maxLevel[174],
                6 => pc.maxLevel[219],
                7 => pc.maxLevel[114],
                8 => wc.properties[76].maxLevel,
                9 => pc.maxLevel[115],
                10 => qc.maxLevel[59],
                11 => wc.properties[77].maxLevel,
                12 => qc.maxLevel[58],
                13 => wc.properties[78].maxLevel,
                14 => qc.maxLevel[60],
                _ => 0
            };
        }
    }
}
