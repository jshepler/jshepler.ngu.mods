using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackExpGained
    {
        private static int _nextSource = -1;
        private static bool _altIsDown = false;

        private static long _expLastRB
        {
            get => ModSave.Data.ExpGainedLastRB;
            set => ModSave.Data.ExpGainedLastRB = value;
        }

        private static long _expThisRB
        {
            get => ModSave.Data.ExpGainedThisRB;
            set => ModSave.Data.ExpGainedThisRB = value;
        }

        private static long[] _sourcesThisRB
        {
            get => ModSave.Data.ExpSourcesThisRB;
            set => ModSave.Data.ExpSourcesThisRB = value;
        }

        private static long[] _sourcesLastRB
        {
            get => ModSave.Data.ExpSourcesLastRB;
            set => ModSave.Data.ExpSourcesLastRB = value;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            _expLastRB = _expThisRB;
            _expThisRB = 0L;

            _sourcesLastRB = _sourcesThisRB;
            _sourcesThisRB = [0L, 0L, 0L, 0L];
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "rewardExp")]
        private static void BossController_rewardExp_prefix()
        {
            _nextSource = ExpSource.Bosses;
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(Character), "adventureOfflineProgress"),
            HarmonyPatch(typeof(LootDrop), "zone6Drop"),
            HarmonyPatch(typeof(LootDrop), "zone8Drop"),
            HarmonyPatch(typeof(LootDrop), "zone11Drop"),
            HarmonyPatch(typeof(LootDrop), "zone14Drop"),
            HarmonyPatch(typeof(LootDrop), "zone16Drop"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop")]
        private static void LootDrop_prefix()
        {
            _nextSource = ExpSource.Titans;
        }

        // adventure offline progress does both titans and itopod - titans first, so the above HarmonyPrefix sets that
        // after the titans are done, need to switch source to ITOPOD - this does that at the "if (settings.itopodOn" line
        [HarmonyTranspiler, HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> Character_adventureOfflineProgress_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var itopodOn = typeof(PlayerSettings).GetField("itopodOn");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, itopodOn))
                .Insert(Transpilers.EmitDelegate(() => { _nextSource = ExpSource.ITOPOD; }));

            return cm.InstructionEnumeration();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LootDrop), "itopodDrop")]
        private static void LootDrop_itopodDrop_prefix()
        {
            _nextSource = ExpSource.ITOPOD;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "consumeKnowledgeFruit")]
        private static void FruitController_consumeKnowledgeFruit_prefix()
        {
            _nextSource = ExpSource.Fruit;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(Character), "adventureOfflineProgress"),
            HarmonyPatch(typeof(LootDrop), "zone6Drop"),
            HarmonyPatch(typeof(LootDrop), "zone8Drop"),
            HarmonyPatch(typeof(LootDrop), "zone11Drop"),
            HarmonyPatch(typeof(LootDrop), "zone14Drop"),
            HarmonyPatch(typeof(LootDrop), "zone16Drop"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop"),
            HarmonyPatch(typeof(LootDrop), "itopodDrop"),
            HarmonyPatch(typeof(FruitController), "consumeKnowledgeFruit"),
            HarmonyPatch(typeof(BossController), "rewardExp")]
        private static void LootDrop_postfix()
        {
            _nextSource = -1;
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(Character), "addExp", [typeof(long)]),
            HarmonyPatch(typeof(Character), "addExp", [typeof(float)])]
        private static IEnumerable<CodeInstruction> Character_addExp_long_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var realExp = typeof(Character).GetField("realExp");

            var cm = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, realExp),
                    new CodeMatch(OpCodes.Ldloc_1))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(addExp));

            return cm.InstructionEnumeration();
        }

        private static long addExp(long exp)
        {
            if (_nextSource >= 0)
                _sourcesThisRB[_nextSource] += exp;

            _expThisRB += exp;
            return exp;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SpendExpTooltipTrigger), "OnPointerEnter")]
        private static bool SpendExpTooltipTrigger_OnPointerEnter_postfix()
        {
            StartShowTooltip();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SpendExpTooltipTrigger), "OnPointerExit")]
        private static bool SpendExpTooltipTrigger_OnPointerExit_postfix()
        {
            StopShowTooltip();

            return false;
        }

        private static Coroutine _cor;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);

        private static void StartShowTooltip()
        {
            StopShowTooltip();
            _cor = Plugin.BeginCoroutine(ShowTooltip());
        }

        private static void StopShowTooltip()
        {
            if (_cor == null)
                return;

            Plugin.HideTooltip();
            Plugin.EndCoroutine(_cor);
            _cor = null;
        }

        private static IEnumerator ShowTooltip()
        {
            var character = Plugin.Character;
            var display = (double d) => character.display(d);

            while (true)
            {
                var text = "This will open up a magical menu to allow you to spend EXP on a huge variety of bonuses! Most of these bonuses are permanent, too!"
                    + $"\n\nYou have <b>{character.realExp:###,##0}</b> EXP to spend.";

                if (!_altIsDown)
                    text += $"\n\n<b>EXP gained this rebirth:</b> {display(_expThisRB)}"
                       + $"\n<b>EXP gained last rebirth:</b> {display(_expLastRB)}";

                else
                {
                    var sumThisRB = _sourcesThisRB.Sum();
                    var sumLastRB = _sourcesLastRB.Sum();

                    List<(string, long, float)> dataThisRB = [];
                    List<(string, long, float)> dataLastRB = [];

                    for (var source = 0; source < 4; source++)
                    {
                        var sourceThisRB = _sourcesThisRB[source];
                        if (sourceThisRB > 0)
                            dataThisRB.Add((ExpSource.Name(source), sourceThisRB, _expThisRB == 0 ? 0f : (float)sourceThisRB / _expThisRB));

                        var sourceLastRB = _sourcesLastRB[source];
                        if (sourceLastRB > 0)
                            dataLastRB.Add((ExpSource.Name(source), sourceLastRB, _expLastRB == 0 ? 0f : (float)sourceLastRB / _expLastRB));
                    }

                    var otherThisRB = _expThisRB - sumThisRB;
                    if (otherThisRB > 0)
                        dataThisRB.Add(("Other", otherThisRB, _expThisRB == 0 ? 0f : (float)otherThisRB / _expThisRB));

                    var otherLastRB = _expLastRB - sumLastRB;
                    if (otherLastRB > 0)
                        dataLastRB.Add(("Other", otherLastRB, _expLastRB == 0 ? 0f : (float)otherLastRB / _expLastRB));

                    dataThisRB.Sort(sorter);
                    dataLastRB.Sort(sorter);

                    var sourcesThisRB = dataThisRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");
                    var sourcesLastRB = dataLastRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");

                    text += $"\n\n<b>EXP gained this rebirth:</b> {display(_expThisRB)}\n{sourcesThisRB}"
                       + $"\n\n<b>EXP gained last rebirth:</b> {display(_expLastRB)}\n{sourcesLastRB}";
                }

                Plugin.ShowTooltip(text);
                yield return _wait;
            }
        }

#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
        private static int sorter((string s, long l, float f) a, (string s, long l, float f) b) => b.f.CompareTo(a.f);
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified

        internal static class ExpSource
        {
            internal static int Bosses = 0;
            internal static int Titans = 1;
            internal static int ITOPOD = 2;
            internal static int Fruit = 3;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "Bosses",
                1 => "Titans",
                2 => "ITOPOD",
                3 => "Fruit",
                _ => string.Empty
            };
        }
    }
}

/*

basically changes code from:
	if (!((double)realExp + (double)num2 >= 9.2233720368547758E+18))
	{
		realExp += num2;
		stats.totalExp += num2;
	}

to something like (but not exactly like):
	if (!((double)realExp + (double)num2 >= 9.2233720368547758E+18))
	{
		realExp += num2;
        addExp(num2);
		stats.totalExp += num2;
	}


changes IL from:
	// realExp += num2;
	IL_00e0: ldarg.0
	IL_00e1: ldarg.0
	IL_00e2: ldfld int64 Character::realExp
	IL_00e7: ldloc.1
	IL_00e8: add
	IL_00e9: stfld int64 Character::realExp

to:
	// realExp += num2;
	IL_00e0: ldarg.0
	IL_00e1: ldarg.0
	IL_00e2: ldfld int64 Character::realExp
	IL_00e7: ldloc.1
            call addExp(long)
	IL_00e8: add
	IL_00e9: stfld int64 Character::realExp


addExp takes a long and retuns the same value passed, so the add instruction will still work as expected,
but there is no C# code that would compile to this since I'm inserting the call between setting up the args
for the add instruction and the add instruction itself

 */