using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class NguEnergyAllocator : BaseAllocator
    {
        private static AllNGUController _allNGU;
        private static Character _character;
        private static NguEnergyAllocator Instance = new();
        //private static EnergyNGUCalculator[] _calcs;

        private static Dictionary<difficulty, List<long>> _ratios = new();
        internal static bool _showRatios = false;

        internal NguEnergyAllocator() : base(9)
        {
            Allocators.Energy.Add(Allocators.Feature.NGU_Energy, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.NGU.skills[id].energy += amount;
            _allNGU.NGU[id].updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var currentLevel = _allNGU.NGU[id].CurrentLevel();
            var cap = (CalcCapForLevel(id, currentLevel + 1) * _character.settings.nguCapModifier).CeilToLong();
            //var cap = _character.NGUController.energyNGUCapAmount(id);
            var current = _character.NGU.skills[id].energy;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _allNGU.NGU[id].HitTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllNGUController), "Start")]
        private static void AllNGUController_Start_postfix(AllNGUController __instance)
        {
            _allNGU = __instance;
            _character = __instance.character;
            //_calcs = new EnergyNGUCalculator[_allNGU.NGU.Length];

            _allNGU.NGU.Do((c, i) =>
            {
                Instance.TextComponents[i] = c.transform.parent.parent.Find("+ Button/Text").GetComponent<Text>();
                //_calcs[i] = new EnergyNGUCalculator(c);
            });

            
            var dividers = __instance.normalEnergyNGUDividers;
            _ratios[difficulty.normal] = dividers.Select(d => (long)(d / dividers[0])).ToList();

            dividers = __instance.evilEnergyNGUDividers;
            _ratios[difficulty.evil] = dividers.Select(d => (long)(d / dividers[0])).ToList();

            dividers = __instance.sadisticEnergyNGUDividers;
            _ratios[difficulty.sadistic] = dividers.Select(d => (long)(d / dividers[0])).ToList();

            Plugin.OnUpdate += (o, e) =>
            {
                var isAltDown = Input.GetKey(KeyCode.LeftAlt);
                if (_showRatios != isAltDown)
                {
                    _showRatios = isAltDown;
                    __instance.refreshMenu();
                }
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "add")]
        private static bool NGUMagicController_add_prefix(NGUController __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                    Enumerable.Range(0, 9).Do(i => Instance[i] = !Instance[i]);
                else
                    Instance[id] = !Instance[id];

                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    Instance.DisableAll();
                    Enumerable.Range(0, 9).Do(i => OverCap(i));
                }
                else
                {
                    Instance[id] = false;
                    OverCap(id);
                }

                return false;
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                Instance.DisableAll();
                SplitEnergy();

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "remove")]
        private static void NGUMagicController_remove_postfix(NGUController __instance)
        {
            Instance[__instance.id] = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "updateText")]
        private static void NGUController_updateText_postfix(NGUController __instance)
        {
            if (!_showRatios
                || !_character.InMenu(Menu.NGU_Energy)
                || (_character.challenges.blindChallenge.inChallenge && _character.allChallenges.blindChallenge.completions() >= 4))
                return;

            __instance.energyMagicText.text = $"[{_ratios[_character.settings.nguLevelTrack][__instance.id]:#,###}:1]";
        }

        private static long CalcCapForLevel(int id, long level)
        {
            //_calcs[id].UpdateModifier();
            return Calculators.NGU_EnergyCalculators[id].ResourceFromLevel(level);// _calcs[id].EnergyFromLevel(level);
        }

        private static void OverCap(int id)
        {
            var ngu = _allNGU.NGU[id];
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

            _character.idleEnergy += _character.NGU.skills[id].energy;
            _character.NGU.skills[id].energy = 0;

            var cap = CalcCapForLevel(id, (long)targetLevel);
            if (cap > _character.idleEnergy)
                cap = _character.idleEnergy;

            Instance.Allocate(id, cap);
            _character.idleEnergy -= cap;
        }

        private static void SplitEnergy()
        {
            var runnableControllers = _allNGU.NGU.Where(c => !c.HitTarget());
            if (!runnableControllers.Any())
                return;

            var ratios = _ratios[_character.settings.nguLevelTrack];

            Dictionary<int, double> parts = new();
            foreach (var ngu in runnableControllers)
            {
                parts[ngu.id] = ngu.CurrentLevel() * ratios[ngu.id];
            }

            _character.NGUController.removeAllEnergy();
            var idleEnergy = _character.idleEnergy;
            var sumOfParts = parts.Values.Sum(d => d);

            foreach (var ngu in runnableControllers)
            {
                var amount = (long)((parts[ngu.id] / sumOfParts) * idleEnergy);
                _character.NGU.skills[ngu.id].energy += amount;
                _character.idleEnergy -= amount;

                ngu.updateText();
            }
        }
    }
}
