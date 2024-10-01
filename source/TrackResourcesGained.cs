using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private static long _curSeeds => Plugin.Character.yggdrasil.seeds;
        private static long _curPoop => Plugin.Character.arbitrary.poop1Count;
        private static long _curAP => Plugin.Character.arbitrary.curArbitraryPoints;

        private static long _lastSeedCount;
        private static long _lastPoopCount;
        private static long _lastAPCount;

        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

        // too many methods that add seeds/poop that I'd rather check current seed/poop count every update
        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastSeedCount = _curSeeds;
                _lastPoopCount = _curPoop;
                _lastAPCount = _curAP;
            };

            Plugin.OnLateUpdate += (o, e) =>
            {
                var seeds = _curSeeds;
                if (seeds > _lastSeedCount)
                    _seedsThisRB += (seeds - _lastSeedCount);

                var poop = _curPoop;
                if (poop > _lastPoopCount)
                    _poopThisRB += (poop - _lastPoopCount);

                var ap = _curAP;
                if (ap > _lastAPCount)
                    _apThisRB += (ap - _lastAPCount);

                _lastSeedCount = seeds;
                _lastPoopCount = poop;
                _lastAPCount = ap;
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            _seedsLastRB = _seedsThisRB;
            _seedsThisRB = 0L;

            _poopLastRB = _poopThisRB;
            _poopThisRB = 0L;

            _apLastRB = _apThisRB;
            _apThisRB = 0L;
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

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showPotionTimer")]
        private static void ButtonShower_showPotionTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            var character = Plugin.Character;

            ___message += $"\n\n<b>AP gained this rebirth:</b> {character.display(_apThisRB)}"
                + $"\n<b>AP gained last rebirth:</b> {character.display(_apLastRB)}";

            __instance.tooltip.showTooltip(___message);
        }

        //private static Coroutine _crTooltip;
        //private static WaitForSeconds _wait1 = new WaitForSeconds(1f);

        //private static void StopTooltip(PointerEventData e)
        //{
        //    if (_crTooltip != null)
        //    {
        //        Plugin.Character.StopCoroutine(_crTooltip);
        //        _crTooltip = null;
        //    }

        //    Plugin.Character.adventureController.tooltip.hideTooltip();
        //}
    }
}
