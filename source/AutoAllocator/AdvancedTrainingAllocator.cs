using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class AdvancedTrainingAllocator : BaseAllocator
    {
        private static AllAdvancedTraining _allAT;
        private static Character _character;
        private static AdvancedTrainingController[] _controllers;
        private static AdvancedTrainingAllocator Instance = new();

        //private static AdvancedTrainingController _controller(int id) =>
        //    id switch
        //    {
        //        0 => _allAT.defense,
        //        1 => _allAT.attack,
        //        2 => _allAT.block,
        //        3 => _allAT.wandoosEnergy,
        //        4 => _allAT.wandoosMagic,
        //        _ => null
        //    };

        internal AdvancedTrainingAllocator() : base(5)
        {
            Allocators.Energy.Add(Allocators.Feature.AdvancedTraining, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.advancedTraining.energy[id] += amount;
            _controllers[id].updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            if (_character.wishes.wishes[190].level >= 1) return 0;

            var cap = CalcCapForLevel(id, _character.advancedTraining.level[id] + 1);
            var current = _character.advancedTraining.energy[id];

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _controllers[id].HitTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "Start")]
        private static void AdvantedTrainingController_Start_postfix(AdvancedTrainingController __instance)
        {
            if (_allAT == null)
            {
                _character = __instance.character;
                _allAT = _character.advancedTrainingController;
                _controllers = new[]
                {
                    _allAT.defense,
                    _allAT.attack,
                    _allAT.block,
                    _allAT.wandoosEnergy,
                    _allAT.wandoosMagic
                };

                for(var x = 0; x < 5; x++)
                    Instance.TextComponents[x] = _controllers[x].transform.parent.Find("Add/Text").GetComponent<Text>();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdvancedTrainingController), "addEnergy")]
        private static bool AdvancedTrainingController_addEnergy_prefix(AdvancedTrainingController __instance)
        {
            if (_character.wishes.wishes[190].level >= 1)
                return true;

            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                Instance[id] = !Instance[id];
                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                Instance[id] = false;
                OverCap(id);

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

        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "removeEnergy")]
        private static void AdvancedTrainingController_removeEnergy_postfix(AdvancedTrainingController __instance)
        {
            Instance[__instance.id] = false;
        }

        private static long CalcCapForLevel(int id, long level)
        {
            var power = (double)_character.totalEnergyPower();
            var speedBonus = (double)_character.totalAdvancedTrainingSpeedBonus();
            var cap = (50 * _controllers[id].baseTime * level * Math.Sqrt(power) / (speedBonus * power));

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return ((long)cap) + 1;
        }

        private static void OverCap(int id)
        {
            if (_character.advancedTraining.levelTarget[id] == -1)
                return;

            var currentLevel = (ulong)_character.advancedTraining.level[id];
            var targetLevel = (ulong)_character.advancedTraining.levelTarget[id];
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

            _character.idleEnergy += _character.advancedTraining.energy[id];
            _character.advancedTraining.energy[id] = 0;

            var cap = CalcCapForLevel(id, (long)targetLevel);
            if (cap > _character.idleEnergy)
                cap = _character.idleEnergy;

            Instance.Allocate(id, cap);
            _character.idleEnergy -= cap;
        }

        private static void SplitEnergy()
        {
            var runnableControllers = _controllers.Where(c => !c.HitTarget());
            if (!runnableControllers.Any())
                return;

            var normalized = _controllers.Select(c => c.baseTime / _controllers[0].baseTime).ToList();
            var parts = new Dictionary<int, double>();
            foreach (var c in runnableControllers)
                parts[c.id] = c.CurrentLevel() * normalized[c.id];

            var sumOfParts = parts.Sum(p => p.Value);

            _character.advancedTrainingController.removeAllEnergy();

            var idleEnergy = _character.idleEnergy;
            foreach (var c in runnableControllers)
            {
                var amount = (long)((parts[c.id] / sumOfParts) * idleEnergy);
                _character.advancedTraining.energy[c.id] = amount;
                _character.idleEnergy -= amount;

                c.updateText();
            }
        }
    }
}
