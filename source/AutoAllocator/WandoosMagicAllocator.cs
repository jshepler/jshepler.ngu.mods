using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class WandoosMagicAllocator : BaseAllocator
    {
        private static Character _character;
        private static Wandoos98Controller _controller;
        private static WandoosMagicAllocator Instance = new();

        internal WandoosMagicAllocator() : base(1)
        {
            Allocators.Magic.Add(Allocators.Feature.Wandoos_Magic, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.wandoos98.wandoosMagic += amount;
            _controller.updateText();
        }

        internal override long CalcCapDelta(int id)
        {
            var cap = _controller.capAmountMagic();
            var current = _character.wandoos98.wandoosMagic;

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
            Instance.TextComponents[0] = __instance.transform.parent.Find("Magic Panel/Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "addMagic")]
        private static bool Wandoos98Controller_addMagic_prefix(Wandoos98Controller __instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[0] = !Instance[0];
                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Wandoos98Controller), "removeMagic")]
        private static void Wandoos98Controller_removeMagic_postfix(Wandoos98Controller __instance)
        {
            Instance[0] = false;
        }
    }
}
