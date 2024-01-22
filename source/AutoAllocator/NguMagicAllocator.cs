using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class NguMagicAllocator : BaseAllocator
    {
        private static AllNGUController _allNGU;
        private static Character _character;
        private static NguMagicAllocator Instance = new();
        //private static MagicNGUCalculator[] _calcs;
        
        private static Dictionary<difficulty, List<long>> _ratios = new();

        internal NguMagicAllocator() : base(7)
        {
            Allocators.Magic.Add(Allocators.Feature.NGU_Magic, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.NGU.magicSkills[id].magic += amount;
            _allNGU.NGUMagic[id].updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var currentLevel = _allNGU.NGUMagic[id].CurrentLevel();
            var cap = (CalcCapForLevel(id, currentLevel + 1) * _character.settings.nguCapModifier).CeilToLong();
            //var cap = _character.NGUController.magicNGUCapAmount(id);
            var current = _character.NGU.magicSkills[id].magic;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _allNGU.NGUMagic[id].HitTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllNGUController), "Start")]
        private static void AllNGUController_Start_postfix(AllNGUController __instance)
        {
            _allNGU = __instance;
            _character = __instance.character;
            //_calcs = new MagicNGUCalculator[_allNGU.NGUMagic.Length];

            _allNGU.NGUMagic.Do((c, i) =>
            {
                Instance.TextComponents[i] = c.transform.parent.parent.Find("+ Button/Text").GetComponent<Text>();
                //_calcs[i] = new MagicNGUCalculator(c);
            });

            var dividers = __instance.normalMagicNGUDividers;
            _ratios[difficulty.normal] = dividers.Select(d => (long)(d / dividers[0])).ToList();

            dividers = __instance.evilMagicNGUDividers;
            _ratios[difficulty.evil] = dividers.Select(d => (long)(d / dividers[0])).ToList();

            dividers = __instance.sadisticMagicNGUDividers;
            _ratios[difficulty.sadistic] = dividers.Select(d => (long)(d / dividers[0])).ToList();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUMagicController), "add")]
        private static bool NGUMagicController_add_prefix(NGUMagicController __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                    Enumerable.Range(0, 7).Do(i => Instance[i] = !Instance[i]);
                else
                    Instance[id] = !Instance[id];

                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Options.Allocators.OverCapAllocatorEnabled.Value == true)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    Instance.DisableAll();
                    Enumerable.Range(0, 7).Do(i => OverCap(i));
                }
                else
                {
                    Instance[id] = false;
                    OverCap(id);
                }

                return false;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Options.Allocators.RatioSplitAllocatorEnabled.Value == true)
            {
                Instance.DisableAll();
                SplitMagic();

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "remove")]
        private static void NGUMagicController_remove_postfix(NGUMagicController __instance)
        {
            Instance[__instance.id] = false;

            if (Input.GetKey(KeyCode.LeftShift))
                __instance.removeAll();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "updateText")]
        private static void NGUMagicController_updateText_postfix(NGUMagicController __instance)
        {
            var character = __instance.character;

            if (!NguEnergyAllocator._showRatios
                || !character.InMenu(Menu.NGU_Magic)
                || (character.challenges.blindChallenge.inChallenge && character.allChallenges.blindChallenge.completions() >= 4))
                return;

            __instance.energyMagicText.text = $"[{_ratios[character.settings.nguLevelTrack][__instance.id]:#,###}:1]";
        }

        private static long CalcCapForLevel(int id, long level)
        {
            //_calcs[id].UpdateModifier();
            //return _calcs[id].MagicFromLevel(level);
            return Calculators.NGU_MagicCalculators[id].ResourceFromLevel(level);
        }

        private static void OverCap(int id)
        {
            var ngu = _allNGU.NGUMagic[id];
            var t = ngu.GetTarget();
            if (t == -1)
                return;

            var currentLevel = (ulong)ngu.CurrentLevel();
            var targetLevel = (ulong)t;
            if (targetLevel > 0 && currentLevel >= targetLevel)
                return;

            if (targetLevel == 0)
            {
                var targetRbSeconds = (ulong)_character.input.energyMagicInput * 60;
                var curRebirthSeconds = (ulong)_character.rebirthTime.totalseconds;
                if (targetRbSeconds <= curRebirthSeconds)
                    return;

                var gainLevels = (targetRbSeconds - curRebirthSeconds) * 50;
                targetLevel = currentLevel + gainLevels;
                if (targetLevel > long.MaxValue)
                    targetLevel = long.MaxValue;
            }

            _character.magic.idleMagic += _character.NGU.magicSkills[id].magic;
            _character.NGU.magicSkills[id].magic = 0;

            var cap = CalcCapForLevel(id, (long)targetLevel);
            if (cap > _character.magic.idleMagic)
                cap = _character.magic.idleMagic;

            Instance.Allocate(id, cap);
            _character.magic.idleMagic -= cap;
        }

        private static void SplitMagic()
        {
            var runnableControllers = _allNGU.NGUMagic.Where(c => !c.HitTarget());
            if (!runnableControllers.Any())
                return;

            var ratios = _ratios[_character.settings.nguLevelTrack];

            Dictionary<int, double> parts = new();
            foreach (var ngu in runnableControllers)
            {
                parts[ngu.id] = ngu.CurrentLevel() * ratios[ngu.id];
            }

            _character.NGUController.removeAllMagic();
            var idleMagic = _character.magic.idleMagic;
            var sumOfParts = parts.Values.Sum(d => d);

            foreach (var ngu in runnableControllers)
            {
                var amount = (long)((parts[ngu.id] / sumOfParts) * idleMagic);
                _character.NGU.magicSkills[ngu.id].magic += amount;
                _character.magic.idleMagic -= amount;

                ngu.updateText();
            }
        }
    }
}
