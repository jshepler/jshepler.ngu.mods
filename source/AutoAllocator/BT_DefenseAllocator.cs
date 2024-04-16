using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class BT_DefenseAllocator : BaseAllocator
    {
        private static Character _character;
        private static DefenseTraining[] _controllers;
        private static BT_DefenseAllocator Instance = new();
        private static Toggle _autoAdvanceToggle;

        public BT_DefenseAllocator() : base(6)
        {
            Allocators.Energy.Add(Allocators.Feature.BT_Defense, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.training.defenseEnergy[id] += amount;
            _controllers[id].updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var cap = _character.training.defenseCaps[id];
            var current = _character.training.defenseEnergy[id];

            if (_character.purchases.hasAutoAdvance && _autoAdvanceToggle.isOn)
            {
                cap = 0;
                current = 0;
                for (var x = id; x < 6; x++)
                {
                    cap += _character.training.defenseCaps[x];
                    current += _character.training.defenseEnergy[x];
                }
            }

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            if (_character.purchases.hasAutoAdvance && _autoAdvanceToggle.isOn)
                return _character.training.defenseEnergy.Sum() >= _character.training.defenseCaps.Sum();

            return _character.training.defenseEnergy[id] >= _character.training.defenseCaps[id];
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllDefenseTraining), "Start")]
        private static void AllDefenseTraining_Start_postfix(AllDefenseTraining __instance)
        {
            _character = __instance.character;
            _controllers = __instance.trains;
            _autoAdvanceToggle = _character.allOffenseController.autoAdvanceController.autoAdvanceToggle;

            for (var x = 0; x < 6; x++)
                Instance.TextComponents[x] = _controllers[x].transform.parent.Find("Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DefenseTraining), "addEnergy", [])]
        private static bool DefenseTraining_addEnergy_prefix(DefenseTraining __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[id] = !Instance[id];

                if (_character.settings.syncTraining)
                    Allocators.Energy[Allocators.Feature.BT_Attack][id] = Instance[id];

                return false;
            }

            return true;
        }

        [HarmonyPostfix
            , HarmonyPatch(typeof(DefenseTraining), "removeEnergy", [])
            , HarmonyPatch(typeof(DefenseTraining), "removeEnergy", [typeof(long)])]
        private static void DefenseTraining_removeEnergy_postfix(DefenseTraining __instance)
        {
            if (BT_AttackAllocator.IgnoreRemoveEnergy)
                return;

            Instance[__instance.id] = false;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                BT_AttackAllocator.IgnoreRemoveEnergy = true;
                __instance.removeEnergy(long.MaxValue);

                if (Plugin.Character.settings.syncTraining)
                    Plugin.Character.allOffenseController.trains[__instance.id].removeEnergy(long.MaxValue);

                BT_AttackAllocator.IgnoreRemoveEnergy = false;
            }
        }
    }
}
