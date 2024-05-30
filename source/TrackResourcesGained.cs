using System.Collections;
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

        private static long _qpLastRB
        {
            get => ModSave.Data.QPGainedLastRB;
            set => ModSave.Data.QPGainedLastRB = value;
        }

        private static long _qpThisRB
        {
            get => ModSave.Data.QPGainedThisRB;
            set => ModSave.Data.QPGainedThisRB = value;
        }

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

        private static long _curSeeds => Plugin.Character.yggdrasil.seeds;
        private static long _curPoop => Plugin.Character.arbitrary.poop1Count;
        private static long _curAP => Plugin.Character.arbitrary.curArbitraryPoints;
        private static long _curQP => Plugin.Character.beastQuest.quirkPoints;
        private static long _curPP => Plugin.Character.adventure.itopod.perkPoints;

        private static long _lastSeedCount;
        private static long _lastPoopCount;
        private static long _lastAPCount;
        private static long _lastQPCount;
        private static long _lastPPCount;

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
                _lastQPCount = _curQP;
                _lastPPCount = _curPP;
            };

            Plugin.OnUpdate += (o, e) =>
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

                var qp = _curQP;
                if (qp > _lastQPCount)
                    _qpThisRB += (qp - _lastQPCount);

                var pp = _curPP;
                if (pp > _lastPPCount)
                    _ppThisRB += (pp - _lastPPCount);

                _lastSeedCount = seeds;
                _lastPoopCount = poop;
                _lastAPCount = ap;
                _lastQPCount = qp;
                _lastPPCount = pp;
            };

            var go = GameObject.Find("Canvas/Adventure Menu Canvas/Adventure Menu/ITOPOD Perks")
                .AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(e => StartPerksTooltip())
                .OnPointerExit(e => StopPerksTooltip());
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

            _qpLastRB = _qpThisRB;
            _qpThisRB = 0L;

            _ppLastRB = _ppThisRB;
            _ppThisRB = 0L;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "seedInfo")]
        private static void SeedIconHover_seedInfo_postfix(SeedIconHover __instance)
        {
            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\nSeeds gained this rebirth: {character.display(_seedsThisRB)}\nSeeds gained last rebirth: {character.display(_seedsLastRB)}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SeedIconHover), "poopInfo")]
        private static void SeedIconHover_poopInfo_postfix(SeedIconHover __instance)
        {
            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\nPoop gained this rebirth: {character.display(_poopThisRB)}\nPoop gained last rebirth: {character.display(_poopLastRB)}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showPotionTimer")]
        private static void ButtonShower_showPotionTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            var character = Plugin.Character;

            ___message += $"\n\nAP gained this rebirth: {character.display(_apThisRB)}\nAP gained last rebirth: {character.display(_apLastRB)}";
            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showQuestStatus")]
        private static void ButtonShower_showQuestStatus_postfix(ButtonShower __instance)
        {
            if (!__instance.beast.interactable)
                return;

            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\nQP gained this rebirth: {character.display(_qpThisRB)}\nQP gained last rebirth: {character.display(_qpLastRB)}";
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showTitanTimer"), HarmonyPriority(1)]
        private static void ButtonShower_showTitanTimer_postfix(ButtonShower __instance)
        {
            if (!__instance.adventure.interactable)
                return;

            var character = Plugin.Character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\nPP gained this rebirth: {character.display(_ppThisRB)}\nPP gained last rebirth: {character.display(_ppLastRB)}";
        }

        private static Coroutine _crPerks;
        private static WaitForSeconds _wait1 = new WaitForSeconds(1f);

        private static void StartPerksTooltip()
        {
            if (_crPerks != null)
                Plugin.Character.StopCoroutine(_crPerks);

            _crPerks = Plugin.Character.StartCoroutine(ShowPerksTooltip());
        }

        private static void StopPerksTooltip()
        {
            if (_crPerks != null)
            {
                Plugin.Character.StopCoroutine(_crPerks);
                _crPerks = null;
            }

            Plugin.Character.adventureController.tooltip.hideTooltip();
        }

        private static IEnumerator ShowPerksTooltip()
        {
            var character = Plugin.Character;
            var tt = character.adventureController.tooltip;

            while (true)
            {
                var text = $"PP gained this rebirth: {character.display(_ppThisRB)}\nPP gained last rebirth: {character.display(_ppLastRB)}";
                tt.showTooltip(text);

                yield return _wait1;
            }
        }
    }
}
