using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class AttackTrainingAllocator : BaseAllocator
    {
        private static Character _character;
        private static OffenseTraining[] _controllers;
        private static AttackTrainingAllocator Instance = new();

        public AttackTrainingAllocator() : base(6)
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

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _character.training.attackEnergy[id] >= _character.training.attackCaps[id];
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllOffenseTraining), "Start")]
        private static void AllOffenseTraining_Start_postfix(AllOffenseTraining __instance)
        {
            _character = __instance.character;
            _controllers = __instance.trains;

            for (var x = 0; x < 6; x++)
                Instance.TextComponents[x] = _controllers[x].transform.Find("Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OffenseTraining), "addEnergy", new Type[0])]
        private static bool OffenseTraining_addEnergy_prefix(OffenseTraining __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                Instance[id] = !Instance[id];

                if (_character.settings.syncTraining)
                    Allocators.Energy[Allocators.Feature.BT_Defense][id] = Instance[id];

                return false;
            }

            return true;
        }

        [HarmonyPostfix
            , HarmonyPatch(typeof(OffenseTraining), "removeEnergy", new Type[0])
            , HarmonyPatch(typeof(OffenseTraining), "removeEnergy", new[] { typeof(long) })]
        private static void OffenseTraining_removeEnergy_postfix(OffenseTraining __instance)
        {
            Instance[__instance.id] = false;
        }
    }
}
