using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackPPGained
    {
        private static int _nextSource = -1;
        private static bool _altIsDown = false;

        private static long _ppLastRB
        {
            get => ModSave.Data.PPGainedLastRB;
            set => ModSave.Data.PPGainedLastRB = value;
        }

        private static long _ppThisRB
        {
            get => ModSave.Data.PPGainedThisRB;
            set => ModSave.Data.PPGainedThisRB = value;
        }

        private static long[] _sourcesThisRB
        {
            get => ModSave.Data.PPSourcesThisRB;
            set => ModSave.Data.PPSourcesThisRB = value;
        }

        private static long[] _sourcesLastRB
        {
            get => ModSave.Data.PPSourcesLastRB;
            set => ModSave.Data.PPSourcesLastRB = value;
        }

        private static long _curPP => Plugin.Character.adventure.itopod.perkPoints;

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
            _ppLastRB = _ppThisRB;
            _ppThisRB = 0L;

            _sourcesLastRB = _sourcesThisRB;
            _sourcesThisRB = [0L, 0L, 0L];
        }

        // awardHighestLevelPP doesn't call addProgress, so do a before/after comparison
        private static long _ppBefore;

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "awardHighestLevelPP")]
        private static void ItopodPerkController_awardHighestLevelPP_prefix(ItopodPerkController __instance)
        {
            _ppBefore = _curPP;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "awardHighestLevelPP")]
        private static void ItopodPerkController_awardHighestLevelPP_postfix()
        {
            var gained = _curPP - _ppBefore;
            _sourcesThisRB[PPSource.ITOPOD] += gained;
            _ppThisRB += gained;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static void setITOPOD(AdventureController __instance)
        {
            if (__instance.zone == 1000)
                _nextSource = PPSource.ITOPOD;
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
        private static void setTitans()
        {
            _nextSource = PPSource.Titans;
        }

        // adventure offline progress does both titans and itopod - titans first, so the above HarmonyPrefix sets that
        // after the titans are done, need to switch source to ITOPOD - this does that at the "if (settings.buffedKillsOn" line
        [HarmonyTranspiler, HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> Character_adventureOfflineProgress_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var itopodOn = typeof(PlayerSettings).GetField("itopodOn");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, itopodOn))
                .Insert(Transpilers.EmitDelegate(() => { _nextSource = PPSource.ITOPOD; }));

            return cm.InstructionEnumeration();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "consumePPFruit")]
        private static void setFruit_prefix()
        {
            _nextSource = PPSource.Fruit;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(AdventureController), "enemyDeath"),
            HarmonyPatch(typeof(Character), "adventureOfflineProgress"),
            HarmonyPatch(typeof(LootDrop), "zone19Drop"),
            HarmonyPatch(typeof(LootDrop), "zone23Drop"),
            HarmonyPatch(typeof(LootDrop), "zone26Drop"),
            HarmonyPatch(typeof(LootDrop), "zone30Drop"),
            HarmonyPatch(typeof(LootDrop), "zone34Drop"),
            HarmonyPatch(typeof(LootDrop), "zone38Drop"),
            HarmonyPatch(typeof(LootDrop), "zone42Drop"),
            HarmonyPatch(typeof(FruitController), "consumePPFruit")]
        private static void setNone_postfix()
        {
            _nextSource = -1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "addProgress")]
        private static void ItopodPerkController_addProgress_postfix(long __result)
        {
            if (_nextSource >= 0)
                _sourcesThisRB[_nextSource] += __result;

            _ppThisRB += __result;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "Start")]
        private static void ItopodPerkController_Start_postfix(ItopodPerkController __instance)
        {
            var image = GameObject.Find("Canvas/ITOPOD Perks Canvas/Item List Page 1 Menu/Top Panel/PP Panel (lol peepee)/Current PP/Image").GetComponent<Image>();

            image.gameObject
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(StartShowTooltip)
                .OnPointerExit(StopTooltip);

            image.raycastTarget = true;
        }

        private static Coroutine _cor;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);

        private static void StartShowTooltip(PointerEventData e)
        {
            StopTooltip(e);
            _cor = Plugin.BeginCoroutine(ShowTooltip());
        }

        private static void StopTooltip(PointerEventData e)
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
                    text = $"<b>PP gained this rebirth:</b> {character.display(_ppThisRB)}"
                        + $"\n<b>PP gained last rebirth:</b> {character.display(_ppLastRB)}";

                else
                {
                    var sumThisRB = _sourcesThisRB.Sum();
                    var sumLastRB = _sourcesLastRB.Sum();

                    List<(string, long, float)> dataThisRB = [];
                    List<(string, long, float)> dataLastRB = [];

                    for (var source = 0; source < 3; source++)
                    {
                        var sourceThisRB = _sourcesThisRB[source];
                        if (sourceThisRB > 0)
                            dataThisRB.Add((PPSource.Name(source), sourceThisRB, _ppThisRB == 0 ? 0f : (float)sourceThisRB / _ppThisRB));

                        var sourceLastRB = _sourcesLastRB[source];
                        if (sourceLastRB > 0)
                            dataLastRB.Add((PPSource.Name(source), sourceLastRB, _ppLastRB == 0 ? 0f : (float)sourceLastRB / _ppLastRB));
                    }

                    var otherThisRB = _ppThisRB - sumThisRB;
                    if (otherThisRB > 0)
                        dataThisRB.Add(("Other", otherThisRB, _ppThisRB == 0 ? 0f : (float)otherThisRB / _ppThisRB));

                    var otherLastRB = _ppLastRB - sumLastRB;
                    if (otherLastRB > 0)
                        dataLastRB.Add(("Other", otherLastRB, _ppLastRB == 0 ? 0f : (float)otherLastRB / _ppLastRB));

                    dataThisRB.Sort(sorter);
                    dataLastRB.Sort(sorter);

                    var sourcesThisRB = dataThisRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");
                    var sourcesLastRB = dataLastRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");

                    text = $"<b>PP gained this rebirth:</b> {character.display(_ppThisRB)}\n{sourcesThisRB}"
                        + $"\n\n<b>PP gained last rebirth:</b> {character.display(_ppLastRB)}\n{sourcesLastRB}";
                }

                Plugin.ShowTooltip(text);
                yield return _wait;
            }
        }

#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
        private static int sorter((string s, long l, float f) a, (string s, long l, float f) b) => b.f.CompareTo(a.f);
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified

        internal static class PPSource
        {
            internal static int ITOPOD = 0;
            internal static int Titans = 1;
            internal static int Fruit = 2;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "ITOPOD",
                1 => "Titans",
                2 => "Fruit",
                _ => string.Empty
            };
        }
    }
}
