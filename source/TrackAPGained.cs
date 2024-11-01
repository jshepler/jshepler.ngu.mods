using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackAPGained
    {
        private static long _apLastRB
        {
            get => ModSave.Data.APGainedLastRB;
            set => ModSave.Data.APGainedLastRB = value;
        }

        private static long _apThisRB
        {
            get => ModSave.Data.APGainedThisRB;
            set => ModSave.Data.APGainedThisRB = value;
        }

        private static long _curAP => Plugin.Character.arbitrary.curArbitraryPoints;
        private static long _lastAPCount;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastAPCount = _curAP;
            };

            Plugin.OnLateUpdate += (o, e) =>
            {
                var ap = _curAP;
                if (ap > _lastAPCount)
                    _apThisRB += (ap - _lastAPCount);

                _lastAPCount = ap;
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            _apLastRB = _apThisRB;
            _apThisRB = 0L;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showPotionTimer")]
        private static void ButtonShower_showPotionTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            var character = Plugin.Character;

            ___message += $"\n\n<b>AP gained this rebirth:</b> {character.display(_apThisRB)}"
                + $"\n<b>AP gained last rebirth:</b> {character.display(_apLastRB)}";

            __instance.tooltip.showTooltip(___message);
        }

        // show AP gain on rebirth
        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerDisplay), "Update")]
        private static void RebirthPowerDisplay_Update_postfix(RebirthPowerDisplay __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Rebirth)
                || (character.challenges.blindChallenge.inChallenge && character.allChallenges.blindChallenge.completions() >= 4))
                return;

            var time = (long)__instance.character.rebirthTime.totalseconds - 3600;
            if (time < 0)
                time = 0L;

            var ap = character.checkAPAdded(time / 500);
            __instance.rebirthChange.text += $"\nYou will gain {ap} AP if you rebirth now.";
        }

        // show AP gain for current quest
        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static IEnumerable<CodeInstruction> BeastQuestController_updateText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n<b>This Quest is currently worth "))
                .SetInstruction(Transpilers.EmitDelegate(BuildQuestReward));

            return cm.InstructionEnumeration();
        }

        private static string BuildQuestReward()
        {
            var character = Plugin.Character;
            var controller = character.beastQuestController;

            var isMinorQuest = character.beastQuest.reducedRewards;
            var baseAP = isMinorQuest ? controller.minorQuestAPReward() : controller.majorQuestAPReward();

            if (character.beastQuest.allActive)
                baseAP = (long)(baseAP * controller.allActiveModifier());

            var ap = character.checkAPAdded(baseAP);

            return $"\n<b>This Quest is currently worth {ap} AP and ";
        }
    }
}
