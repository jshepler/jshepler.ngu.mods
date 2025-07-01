using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
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

        internal override void OnTargetReached(int id)
        {
            base.OnTargetReached(id);

            if (!Plugin.Character.NGU.autoAdvance)
                return;

            var count = base.Length;
            for (var x = 0; x < count; x++)
            {
                var nextId = (id + x) % count;
                if (IsTargetReached(nextId) || this[nextId])
                    continue;

                this[nextId] = true;
                break;
            }
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
                var isAltDown = Plugin.AltIsDown;
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

            if (Plugin.ShiftIsDown)
            {
                if (Plugin.AltIsDown)
                    Enumerable.Range(0, 9).Do(i =>
                    {
                        if (Instance[i] || !Instance.IsTargetReached(i))
                            Instance[i] = !Instance[i];
                    });
                else
                    Instance[id] = !Instance[id];

                return false;
            }

            if (Plugin.ControlIsDown)
            {
                if (Plugin.AltIsDown)
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

            if (Plugin.AltIsDown)
            {
                Instance.DisableAll();
                SplitEnergy();

                return false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "remove")]
        private static bool NGUMagicController_remove_prefix(NGUController __instance)
        {
            if (Plugin.ControlIsDown)
            {
                if (Plugin.AltIsDown)
                    Enumerable.Range(0, 9).Do(setTimeTarget);
                else
                    setTimeTarget(__instance.id);

                return false;
            }

            Instance[__instance.id] = false;

            if (Plugin.ShiftIsDown)
                __instance.removeAll();

            return true;
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

        private static double CalcCapForLevel(int id, long level)
        {
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

            var cap = CalcCapForLevel(id, (long)targetLevel).CeilToLong();
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

        private static void setTimeTarget(int id)
        {
            var controller = _allNGU.NGU[id];
            var character = controller.character;
            var calc = Calculators.NGU_EnergyCalculators[id];
            var resource = character.NGU.skills[id].energy;
            if (resource == 0)
                resource = character.totalCapEnergy();

            var runTimeSeconds = character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = controller.CurrentLevel();

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            controller.target.text = targetLevel.ToString();
            controller.setTarget();
        }
    }
}
