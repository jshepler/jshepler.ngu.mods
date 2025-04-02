using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackQPGained
    {
        internal const int SOURCE_COUNT = 5;
        private static bool _altIsDown = false;

        // there isn't a method used to add qp, so need to compare before/after the various methods that add qp
        private static long _qpBefore = 0;
        private static int _lastQuestSource;

        internal static long QPGainedLastRB
        {
            get => ModSave.Data.QPGainedLastRB;
            set => ModSave.Data.QPGainedLastRB = value;
        }

        private static long QPGainedThisRB
        {
            get => ModSave.Data.QPGainedThisRB;
            set => ModSave.Data.QPGainedThisRB = value;
        }

        private static long[] _sourcesThisRB
        {
            get => ModSave.Data.QPSourcesThisRB;
            set => ModSave.Data.QPSourcesThisRB = value;
        }

        private static long[] _sourcesLastRB
        {
            get => ModSave.Data.QPSourcesLastRB;
            set => ModSave.Data.QPSourcesLastRB = value;
        }

        private static long _curQP => Plugin.Character.beastQuest.quirkPoints;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                var count = _sourcesThisRB.Length;
                if (count < SOURCE_COUNT)
                    _sourcesThisRB = _sourcesThisRB.Concat(Enumerable.Range(0, SOURCE_COUNT - count).Select(i => 0L)).ToArray();

                count = _sourcesLastRB.Length;
                if (count < SOURCE_COUNT)
                    _sourcesLastRB = _sourcesLastRB.Concat(Enumerable.Range(0, SOURCE_COUNT - count).Select(i => 0L)).ToArray();

                //while (_sourcesThisRB.Length < SOURCE_COUNT)
                //    _sourcesThisRB.Append(0L);

                //while (_sourcesLastRB.Length < SOURCE_COUNT)
                //    _sourcesLastRB.Append(0L);
            };

            Plugin.OnUpdate += (o, e) =>
            {
                _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            QPGainedLastRB = QPGainedThisRB;
            QPGainedThisRB = 0L;

            _sourcesLastRB = _sourcesThisRB;
            _sourcesThisRB = new long[SOURCE_COUNT];
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "giveRewardsAndClear", [typeof(bool)])]
        private static void quests_before()
        {
            _qpBefore = _curQP;
            _lastQuestSource = Plugin.Character.beastQuest.reducedRewards ? QPSource.MinorQuests : QPSource.MajorQuests;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "giveRewardsAndClear", [typeof(bool)])]
        private static void quests_after()
        {
            var gained = _curQP - _qpBefore;
            _sourcesThisRB[_lastQuestSource] += gained;
            QPGainedThisRB += gained;
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(Character), "adventureOfflineProgress"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop")]
        private static void titans_before()
        {
            _qpBefore = _curQP;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(Character), "adventureOfflineProgress"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop")]
        private static void titans_after()
        {
            var gained = _curQP - _qpBefore;
            _sourcesThisRB[QPSource.Titans] += gained;
            QPGainedThisRB += gained;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "consumeQPFruit")]
        private static void fruit_before()
        {
            _qpBefore = _curQP;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(FruitController), "consumeQPFruit")]
        private static void fruit_after()
        {
            var gained = _curQP - _qpBefore;
            _sourcesThisRB[QPSource.Fruit] += gained;
            QPGainedThisRB += gained;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "Start")]
        private static void BeastQuestPerkController_Start_postfix(BeastQuestPerkController __instance)
        {
            var image = GameObject.Find("Canvas/Beast Quirks Canvas/Beast Quirks Menu/Top Panel/Current QP/Image").GetComponent<Image>();

            image.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(StartShowTooltip)
                .OnPointerExit(StopShowTooltip);

            image.raycastTarget = true;
        }

        private static Coroutine _cor;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);

        private static void StartShowTooltip(PointerEventData e)
        {
            StopShowTooltip(e);
            _cor = Plugin.BeginCoroutine(ShowTooltip());
        }

        private static void StopShowTooltip(PointerEventData e)
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
            var text = string.Empty;

            while (true)
            {
                if (!_altIsDown)
                    text = $"<b>QP gained this rebirth:</b> {character.display(QPGainedThisRB)}"
                        + $"\n<b>QP gained last rebirth:</b> {character.display(QPGainedLastRB)}";

                else
                {
                    var sumThisRB = _sourcesThisRB.Sum();
                    var sumLastRB = _sourcesLastRB.Sum();

                    List<(string, long, float)> dataThisRB = [];
                    List<(string, long, float)> dataLastRB = [];

                    for (var source = 0; source < SOURCE_COUNT; source++)
                    {
                        var sourceThisRB = _sourcesThisRB[source];
                        if (sourceThisRB > 0)
                            dataThisRB.Add((QPSource.Name(source), sourceThisRB, QPGainedThisRB == 0 ? 0f : (float)sourceThisRB / QPGainedThisRB));

                        var sourceLastRB = _sourcesLastRB[source];
                        if (sourceLastRB > 0)
                            dataLastRB.Add((QPSource.Name(source), sourceLastRB, QPGainedLastRB == 0 ? 0f : (float)sourceLastRB / QPGainedLastRB));
                    }

                    var otherThisRB = QPGainedThisRB - sumThisRB;
                    if (otherThisRB > 0)
                        dataThisRB.Add(("Other", otherThisRB, QPGainedThisRB == 0 ? 0f : (float)otherThisRB / QPGainedThisRB));

                    var otherLastRB = QPGainedLastRB - sumLastRB;
                    if (otherLastRB > 0)
                        dataLastRB.Add(("Other", otherLastRB, QPGainedLastRB == 0 ? 0f : (float)otherLastRB / QPGainedLastRB));

                    dataThisRB.Sort(sorter);
                    dataLastRB.Sort(sorter);

                    var sourcesThisRB = dataThisRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");
                    var sourcesLastRB = dataLastRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");

                    text = $"<b>QP gained this rebirth:</b> {character.display(QPGainedThisRB)}\n{sourcesThisRB}"
                        + $"\n\n<b>QP gained last rebirth:</b> {character.display(QPGainedLastRB)}\n{sourcesLastRB}";
                }

                Plugin.ShowTooltip(text);
                yield return _wait;
            }
        }

        private static int sorter((string s, long l, float f) a, (string s, long l, float f) b) => b.f.CompareTo(a.f);

        internal static class QPSource
        {
            internal static int Quests = 0;
            internal static int Titans = 1;
            internal static int Fruit = 2;
            internal static int MinorQuests = 3;
            internal static int MajorQuests = 4;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "Quests",
                1 => "Titans",
                2 => "Fruit",
                3 => "Minor Quests",
                4 => "Major Quests",
                _ => $"??? {i}"
            };
        }
    }
}
