using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackResourcesGained
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

        private static long _poopLastRB
        {
            get => ModSave.Data.PoopGainedLastRB;
            set => ModSave.Data.PoopGainedLastRB = value;
        }

        private static long _poopThisRB
        {
            get => ModSave.Data.PoopGainedThisRB;
            set => ModSave.Data.PoopGainedThisRB = value;
        }

        private static long _curSeeds => Plugin.Character.yggdrasil.seeds;
        private static long _curPoop => Plugin.Character.arbitrary.poop1Count;

        private static long _lastSeedCount;
        private static long _lastPoopCount;

        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Fart _fart;

        // too many methods that add seeds/poop that I'd rather check current seed/poop count every update
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
            _poopThisRB = 0L;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "seedInfo")]
        private static void SeedIconHover_seedInfo_postfix(SeedIconHover __instance)
        {
            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\n<b>Seeds gained this rebirth:</b> {character.display(_seedsThisRB)}"
                + $"\n<b>Seeds gained last rebirth:</b> {character.display(_seedsLastRB)}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "poopInfo")]
        private static void SeedIconHover_poopInfo_postfix(SeedIconHover __instance)
        {
            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\n<b>Poop gained this rebirth:</b> {character.display(_poopThisRB)}"
                + $"\n<b>Poop gained last rebirth:</b> {character.display(_poopLastRB)}";
        }
    }
}
