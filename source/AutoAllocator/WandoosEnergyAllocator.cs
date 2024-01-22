using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class WandoosEnergyAllocator : BaseAllocator
    {
        private static Character _character;
        private static Wandoos98Controller _controller;
        private static WandoosEnergyAllocator Instance = new();

        internal WandoosEnergyAllocator() : base(1)
        {
            Allocators.Energy.Add(Allocators.Feature.Wandoos_Energy, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.wandoos98.wandoosEnergy += amount;
            _controller.updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var cap = _controller.capAmountEnergy();
            var current = _character.wandoos98.wandoosEnergy;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Wandoos98Controller), "Start")]
        private static void Wandoos98Controller_Start_postfix(Wandoos98Controller __instance)
        {
            _controller = __instance;
            _character = __instance.character;
            Instance.TextComponents[0] = __instance.transform.parent.Find("Energy Panel/Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "addEnergy")]
        private static bool Wandoos98Controller_addEnergy_prefix(Wandoos98Controller __instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[0] = !Instance[0];

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    var magic = Allocators.Magic[Allocators.Feature.Wandoos_Magic];
                    magic[0] = !magic[0];
                }

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Wandoos98Controller), "removeEnergy")]
        private static void Wandoos98Controller_removeEnergy_postfix(Wandoos98Controller __instance)
        {
            Instance[0] = false;

            if (Input.GetKey(KeyCode.LeftShift))
                __instance.removeAllEnergy();
        }
    }
}
