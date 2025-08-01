using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackAPGained
    {
        private static bool _altIsDown = false;
        private static bool _showTooltip = false;

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

        private static long[] _sourcesThisRB
        {
            get => ModSave.Data.APSourcesThisRB;
            set => ModSave.Data.APSourcesThisRB = value;
        }

        private static long[] _sourcesLastRB
        {
            get => ModSave.Data.APSourcesLastRB;
            set => ModSave.Data.APSourcesLastRB = value;
        }

        private static long _curAP => Plugin.Character.arbitrary.curArbitraryPoints;
        private static long _lastAPCount;
        internal static Action Reset => () => _lastAPCount = 0L;

        private static MethodInfo _addAP32 = typeof(Character).GetMethod("addAP", [typeof(int)]);
        private static MethodInfo _addAP64 = typeof(Character).GetMethod("addAP", [typeof(long)]);
        private static FieldInfo _curArbitraryPoints = typeof(Arbitrary).GetField("curArbitraryPoints");

        internal static long TrackGain(long amount, int source)
        {
            if (source >= 0 && source < SOURCE_COUNT)
                _sourcesThisRB[source] += amount;

            return amount;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastAPCount = _curAP;
            };

            Plugin.OnUpdate += (o, e) =>
            {
                var altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                if (altIsDown != _altIsDown)
                {
                    _altIsDown = altIsDown;
                    if(_showTooltip)
                        Plugin.Character.buttons.showPotionTimer();
                }
            };

            Plugin.OnLateUpdate += (o, e) => lateUpate();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "zone6Drop"),
            HarmonyPatch(typeof(LootDrop), "zone8Drop"),
            HarmonyPatch(typeof(LootDrop), "zone11Drop"),
            HarmonyPatch(typeof(LootDrop), "zone14Drop"),
            HarmonyPatch(typeof(LootDrop), "zone16Drop"),
            HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> titans_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64))
                .Repeat(m =>
                {
                    m.Advance(1)
                    .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Titans)));
                });

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "itopodDrop"),
            HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> itopod_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _curArbitraryPoints))
                .Advance(-1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.ITOPOD)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static IEnumerable<CodeInstruction> bosses_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP32))
                .Advance(1)
                .RemoveInstructions(1)
                .Insert(
                    Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Bosses)),
                    Transpilers.EmitDelegate((long l) => Plugin.Character.adventureController.log.AddEvent($"You also gained {l} AP for killing 10 bosses!", 3)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(DailyRewardController), "tier0Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier1Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier2Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier3Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier4Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier5Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier6Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier7Reward")]
        private static IEnumerable<CodeInstruction> dailyspin_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP32))
                .Repeat(m =>
                {
                    m.Advance(1)
                    .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.DailySpin)));
                });

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "giveRewardsAndClear", typeof(bool))]
        private static IEnumerable<CodeInstruction> quests_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64))
                .Repeat(m =>
                {
                    m.Advance(1)
                    .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Quests)));
                });

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumeAPFruit")]
        private static IEnumerable<CodeInstruction> fruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Fruit)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(PitController), "oneTossReward")]
        private static IEnumerable<CodeInstruction> pit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.MoneyPit)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(Rebirth), "awardAP")]
        private static IEnumerable<CodeInstruction> rebirth_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Rebirth)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(OpenFileDialog), "startSaveStandalone")]
        private static IEnumerable<CodeInstruction> dailySave_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP32))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.DailySave)));

            return cm.InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            // the ap gained for rebirth needs to be added to _apThisRB before it gets copied to _apLastRB
            lateUpate();

            _apLastRB = _apThisRB;
            _apThisRB = 0L;

            _sourcesLastRB = _sourcesThisRB;
            _sourcesThisRB = new long[SOURCE_COUNT];
        }

        private static void lateUpate()
        {
            var cur = _curAP;
            var gained = cur - _lastAPCount;

            if (gained > 0)
                _apThisRB += gained;

            _lastAPCount = cur;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(ButtonShower), "showPotionTimers")]
        private static void ButtonShower_showPotionTimers_prefix()
        {
            _showTooltip = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "hideTooltip")]
        private static void ButtonShower_hideTooltip_postfix()
        {
            _showTooltip = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showPotionTimer")]
        private static void ButtonShower_showPotionTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            var display = Plugin.Character.display;

            if (_altIsDown)
            {
                var sumThisRB = _sourcesThisRB.Sum();
                var sumLastRB = _sourcesLastRB.Sum();

                List<(string, long, float)> dataThisRB = [];
                List<(string, long, float)> dataLastRB = [];

                for (var source = 0; source < SOURCE_COUNT; source++)
                {
                    var sourceThisRB = _sourcesThisRB[source];
                    if (sourceThisRB > 0)
                        dataThisRB.Add((APSource.Name(source), sourceThisRB, _apThisRB == 0 ? 0f : (float)sourceThisRB / _apThisRB));

                    var sourceLastRB = _sourcesLastRB[source];
                    if (sourceLastRB > 0)
                        dataLastRB.Add((APSource.Name(source), sourceLastRB, _apLastRB == 0 ? 0f : (float)sourceLastRB / _apLastRB));
                }

                var otherThisRB = _apThisRB - sumThisRB;
                if (otherThisRB > 0)
                    dataThisRB.Add(("Other", otherThisRB, _apThisRB == 0 ? 0f : (float)otherThisRB / _apThisRB));

                var otherLastRB = _apLastRB - sumLastRB;
                if (otherLastRB > 0)
                    dataLastRB.Add(("Other", otherLastRB, _apLastRB == 0 ? 0f : (float)otherLastRB / _apLastRB));

                dataThisRB.Sort(sorter);
                dataLastRB.Sort(sorter);

                var sourcesThisRB = dataThisRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");
                var sourcesLastRB = dataLastRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");

                ___message += $"\n\n<b>AP gained this rebirth:</b> {display(_apThisRB)}\n{sourcesThisRB}"
                    + $"\n\n<b>AP gained last rebirth:</b> {display(_apLastRB)}\n{sourcesLastRB}";
            }

            else
                ___message += $"\n\n<b>AP gained this rebirth:</b> {display(_apThisRB)}"
                    + $"\n<b>AP gained last rebirth:</b> {display(_apLastRB)}";

            __instance.tooltip.showTooltip(___message);
        }



        // show AP gain on rebirth
        //[HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerDisplay), "Update")]
        // moved to ImprovedNumberBreakdown.cs
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

        private static int sorter((string s, long l, float f) a, (string s, long l, float f) b) => b.f.CompareTo(a.f);

        internal const int SOURCE_COUNT = 9;
        internal static class APSource
        {
            internal static int ITOPOD = 0;
            internal static int Titans = 1;
            internal static int Bosses = 2;
            internal static int Fruit = 3;
            internal static int DailySpin = 4;
            internal static int Quests = 5;
            internal static int MoneyPit = 6;
            internal static int Rebirth = 7;
            internal static int DailySave = 8;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "ITOPOD",
                1 => "Titans",
                2 => "Adv Bosses",
                3 => "Fruit",
                4 => "Daily Spin",
                5 => "Quests",
                6 => "Money Pit",
                7 => "Rebirth",
                8 => "Daily Save",
                _ => $"{i}??"
            };
        }
    }
}
