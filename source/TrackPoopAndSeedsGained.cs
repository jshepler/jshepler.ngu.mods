using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackPoopAndSeedsGained
    {
        private static long _seedsLastRB
        {
            get => ModSave.Data.SeedsGainedLastRB;
            set => ModSave.Data.SeedsGainedLastRB = value;
        }

        private static long _seedsThisRB
        {
            get => ModSave.Data.SeedsGainedThisRB;
            set => ModSave.Data.SeedsGainedThisRB = value;
        }

        private static int _poopLastRB
        {
            get => ModSave.Data.PoopGainedLastRB;
            set => ModSave.Data.PoopGainedLastRB = value;
        }

        private static int _poopThisRB
        {
            get => ModSave.Data.PoopGainedThisRB;
            set => ModSave.Data.PoopGainedThisRB = value;
        }

        private static long _curSeeds => Plugin.Character.yggdrasil.seeds;
        private static int _curPoop => Plugin.Character.arbitrary.poop1Count;

        private static long _lastSeedCount;
        private static int _lastPoopCount;
        internal static Action Reset => () => _lastSeedCount = _lastPoopCount = 0;

        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Fart _fart;

        [HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "Start")]
        private static void AllYggdrasil_Start_postfix()
        {
            _fart = new()
            {
                fartNoise = Plugin.Character.gameObject.AddComponent<AudioSource>(),
                tooltip = Plugin.Character.tooltip
            };

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastSeedCount = _curSeeds;
                _lastPoopCount = _curPoop;

                // these used to be longs even though poop is an int, dunno why I had these as longs,
                // but now that they're ints, need to get them converted to ints this way because
                // boxed longs cannot be directly cast to ints, they must be cast to long first
                // (boxed value types must be cast to their actual type first to unbox them)
                if (ModSave.Data.Values.ContainsKey("PoopGainedLastRB"))
                {
                    var boxedValue = ModSave.Data.Values["PoopGainedLastRB"];
                    if (boxedValue is long)
                        _poopLastRB = (int)(long)boxedValue;

                    boxedValue = ModSave.Data.Values["PoopGainedThisRB"];
                    if (boxedValue is long)
                        _poopThisRB = (int)(long)boxedValue;
                }
            };

            Plugin.OnLateUpdate += (o, e) =>
            {
                var seeds = _curSeeds;
                if (seeds > _lastSeedCount)
                    _seedsThisRB += (seeds - _lastSeedCount);

                var poop = _curPoop;
                var poopGained = poop - _lastPoopCount;
                if (poopGained > 0)
                {
                    _poopThisRB += poopGained;
                    Plugin.ShowNotification($"poop: +{poopGained} ({_poopThisRB} | {_curPoop})");

                    if (UnityEngine.Random.value < Options.Yggdrasil.PoopAudioChance.Value)
                        _fart.fart();
                }

                _lastSeedCount = seeds;
                _lastPoopCount = poop;

                if (Input.GetKeyDown(KeyCode.P) && !Plugin.InputFieldHasFocus)
                    _fart.fart();
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            _seedsLastRB = _seedsThisRB;
            _seedsThisRB = 0L;

            _poopLastRB = _poopThisRB;
            _poopThisRB = 0;

            _poopSourcesLastRB = _poopSourcesThisRB;
            _poopSourcesThisRB = new int[POOP_SOURCE_COUNT];
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "seedInfo")]
        private static void SeedIconHover_seedInfo_postfix(SeedIconHover __instance)
        {
            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\n<b>Seeds gained this rebirth:</b> {character.display(_seedsThisRB)}"
                + $"\n<b>Seeds gained last rebirth:</b> {character.display(_seedsLastRB)}";
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "poopInfo")]
        //private static void SeedIconHover_poopInfo_postfix(SeedIconHover __instance)
        //{
        //    var character = Plugin.Character;
        //    var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

        //    tooltipText.text += $"\n\n<b>Poop gained this rebirth:</b> {character.display(_poopThisRB)}"
        //        + $"\n<b>Poop gained last rebirth:</b> {character.display(_poopLastRB)}";
        //}

        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static string _basePoopText = "This is your Poop count. Each poop allows one fruit to be harvested or eaten for a +50% bonus to the results!";
        private static Coroutine _poopTooltipCoroutine;

        [HarmonyPrefix, HarmonyPatch(typeof(SeedIconHover), "poopInfo")]
        private static bool SeedIconHover_poopInfo_prefix()
        {
            if (_poopTooltipCoroutine != null)
                Plugin.EndCoroutine(_poopTooltipCoroutine);

            _poopTooltipCoroutine = Plugin.BeginCoroutine(showPoopTooltip());

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "exitTooltip")]
        private static void SeedIconHover_exitTooltip_postfix()
        {
            if (_poopTooltipCoroutine != null)
                Plugin.EndCoroutine(_poopTooltipCoroutine);
        }

        private static IEnumerator showPoopTooltip()
        {
            var character = Plugin.Character;

            while (true)
            {
                var thisRB = $"\n\n<b>Poop gained this rebirth:</b> {character.display(_poopThisRB)}";
                var lastRB = $"\n<b>Poop gained last rebirth:</b> {character.display(_poopLastRB)}";

                if (Plugin.AltIsDown)
                {
                    thisRB += buildPoopSources(_poopSourcesThisRB, _poopThisRB);
                    lastRB = "\n" + lastRB + buildPoopSources(_poopSourcesLastRB, _poopLastRB);
                }

                Plugin.ShowTooltip(_basePoopText + thisRB + lastRB);

                yield return _wait;
            }
        }


        private static int[] _poopSourcesThisRB
        {
            get => ModSave.Data.PoopSourcesThisRB;
            set => ModSave.Data.PoopSourcesThisRB = value;
        }

        private static int[] _poopSourcesLastRB
        {
            get => ModSave.Data.PoopSourcesLastRB;
            set => ModSave.Data.PoopSourcesLastRB = value;
        }

        internal static int TrackPoopGain(int amount, int source)
        {
            if (source >= 0 && source < POOP_SOURCE_COUNT)
                _poopSourcesThisRB[source] += amount;

            return amount;
        }

        private static FieldInfo _poop1Count = typeof(Arbitrary).GetField("poop1Count");

        [HarmonyTranspiler,
            HarmonyPatch(typeof(ItopodPerkController), "addPoopProgress")]
        private static IEnumerable<CodeInstruction> ItopodPerkController_addPoopProgress_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _poop1Count))
                .MatchForward(false, new CodeMatch(OpCodes.Add))
                .Insert(Transpilers.EmitDelegate((int value) => TrackPoopGain(value, PoopSource.ITOPOD_9k)));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(LootDrop), "itopodDrop")]
        private static IEnumerable<CodeInstruction> ItopodPerkController_itopodDrop_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _poop1Count))
                .MatchForward(false, new CodeMatch(OpCodes.Add))
                .Insert(Transpilers.EmitDelegate((int value) => TrackPoopGain(value, PoopSource.ITOPOD_RNG)));

            return cm.InstructionEnumeration();
        }

        private static int _poopBefore;

        [HarmonyPrefix,
            HarmonyPatch(typeof(DailyRewardController), "startNoBullshitSpin"),
            HarmonyPatch(typeof(DailyRewardController), "updateSpinLoop")]
        private static void dailySpin_prefix()
        {
            _poopBefore = _curPoop;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(DailyRewardController), "startNoBullshitSpin"),
            HarmonyPatch(typeof(DailyRewardController), "updateSpinLoop")]
        private static void dailySpin_postfix()
        {
            var gained = _curPoop - _poopBefore;
            if (gained > 0)
                TrackPoopGain(gained, PoopSource.DailySpin);
        }

        private static string buildPoopSources(int[] gainSources, int totalGained)
        {
            var display = Plugin.Character.display;
            var sum = gainSources.Sum();
            List<(string, int, float)> sources = [];

            for (var source = 0; source < POOP_SOURCE_COUNT; source++)
            {
                var sourceThisRB = gainSources[source];
                if (sourceThisRB > 0)
                    sources.Add((PoopSource.Name(source), sourceThisRB, totalGained == 0 ? 0f : (float)sourceThisRB / totalGained));
            }

            var other = totalGained - sum;
            if (other > 0)
                sources.Add(("Other", other, totalGained == 0 ? 0f : (float)other / totalGained));

            sources.Sort(sorter);

            var sourcesString = sources.Join(d => $"\n   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", string.Empty);
            return sourcesString;
        }

        private static int sorter((string s, int l, float f) a, (string s, int l, float f) b) => b.f.CompareTo(a.f);

        internal const int POOP_SOURCE_COUNT = 3;
        internal static class PoopSource
        {
            internal static int ITOPOD_RNG = 0;
            internal static int ITOPOD_9k = 1;
            internal static int DailySpin = 2;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "ITOPOD (RNG)",
                1 => "ITOPOD (9k)",
                2 => "Daily Spin",
                _ => $"{i}??"
            };
        }
    }
}
