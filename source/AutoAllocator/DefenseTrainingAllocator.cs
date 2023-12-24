using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class DefenseTrainingAllocator : BaseAllocator
    {
        private static Character _character;
        private static DefenseTraining[] _controllers;
        private static DefenseTrainingAllocator Instance = new();

        public DefenseTrainingAllocator() : base(6)
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

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _character.training.defenseEnergy[id] >= _character.training.defenseCaps[id];
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllDefenseTraining), "Start")]
        private static void AllDefenseTraining_Start_postfix(AllDefenseTraining __instance)
        {
            _character = __instance.character;
            _controllers = __instance.trains;

            for (var x = 0; x < 6; x++)
                Instance.TextComponents[x] = _controllers[x].transform.parent.Find("Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DefenseTraining), "addEnergy", new Type[0])]
        private static bool DefenseTraining_addEnergy_prefix(DefenseTraining __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                Instance[id] = !Instance[id];

                if (_character.settings.syncTraining)
                    Allocators.Energy[Allocators.Feature.BT_Attack][id] = Instance[id];

                return false;
            }

            return true;
        }

        [HarmonyPostfix
            , HarmonyPatch(typeof(DefenseTraining), "removeEnergy", new Type[0])
            , HarmonyPatch(typeof(DefenseTraining), "removeEnergy", new[] { typeof(long) })]
        private static void DefenseTraining_removeEnergy_postfix(DefenseTraining __instance)
        {
            Instance[__instance.id] = false;
        }
    }
}
