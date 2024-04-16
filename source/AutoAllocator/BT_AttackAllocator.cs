using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class BT_AttackAllocator : BaseAllocator
    {
        private static Character _character;
        private static OffenseTraining[] _controllers;
        private static BT_AttackAllocator Instance = new();
        private static Toggle _autoAdvanceToggle;

        public BT_AttackAllocator() : base(6)
        {
            Allocators.Energy.Add(Allocators.Feature.BT_Attack, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.training.attackEnergy[id] += amount;
            _controllers[id].updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var cap = _character.training.attackCaps[id];
            var current = _character.training.attackEnergy[id];

            if (_character.purchases.hasAutoAdvance && _autoAdvanceToggle.isOn)
            {
                cap = 0;
                current = 0;
                for (var x = id; x < 6; x++)
                {
                    cap += _character.training.attackCaps[x];
                    current += _character.training.attackEnergy[x];
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
                return _character.training.attackEnergy.Sum() >= _character.training.attackCaps.Sum();

            return _character.training.attackEnergy[id] >= _character.training.attackCaps[id];
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllOffenseTraining), "Start")]
        private static void AllOffenseTraining_Start_postfix(AllOffenseTraining __instance)
        {
            _character = __instance.character;
            _controllers = __instance.trains;
            _autoAdvanceToggle = __instance.autoAdvance;

            for (var x = 0; x < 6; x++)
                Instance.TextComponents[x] = _controllers[x].transform.Find("Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OffenseTraining), "addEnergy", [])]
        private static bool OffenseTraining_addEnergy_prefix(OffenseTraining __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[id] = !Instance[id];

                if (_character.settings.syncTraining)
                    Allocators.Energy[Allocators.Feature.BT_Defense][id] = Instance[id];

                return false;
            }

            return true;
        }

        internal static bool IgnoreRemoveEnergy = false;

        [HarmonyPostfix
            , HarmonyPatch(typeof(OffenseTraining), "removeEnergy", [])
            , HarmonyPatch(typeof(OffenseTraining), "removeEnergy", [typeof(long)])]
        private static void OffenseTraining_removeEnergy_postfix(OffenseTraining __instance)
        {
            if (IgnoreRemoveEnergy)
                return;

            Instance[__instance.id] = false;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                IgnoreRemoveEnergy = true;
                __instance.removeEnergy(long.MaxValue);

                if (Plugin.Character.settings.syncTraining)
                    Plugin.Character.allDefenseController.trains[__instance.id].removeEnergy(long.MaxValue);

                IgnoreRemoveEnergy = false;
            }
        }
    }
}
