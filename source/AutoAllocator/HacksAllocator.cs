using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class HacksAllocator : BaseAllocator
    {
        private const int NUMBER_OF_HACKS = 16;
        private const int NUMBER_OF_PODS = 8;

        private static HacksAllocator Instance = new();
        private static Character _character;
        private static HacksController _hacksController = null;
        
        private static int _currentPage => _hacksController.curPage;

        internal HacksAllocator() : base(NUMBER_OF_HACKS, NUMBER_OF_PODS)
        {
            Allocators.Res3.Add(Allocators.Feature.Hacks, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.hacks.hacks[id].res3 += amount;

            if (_character.InMenu(Menu.Hacks) && (id / NUMBER_OF_PODS) == _currentPage)
            {
                _hacksController.pods[id % NUMBER_OF_PODS].updateRes3Allocated();
            }
        }

        internal override long CalcCapDelta(int id)
        {
            var p = _character.totalRes3Power();
            var s = _character.hacksController.totalHackSpeedBonus();
            var d = _character.hacksController.properties[id].baseDivider;

            var targetLevel = _character.hacks.hacks[id].level + 1;
            var v = (float)Math.Pow(1.0078, (targetLevel - 1)) * targetLevel;

            var cap = (long)(v * d / (p * s));
            var current = _character.hacks.hacks[id].res3;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _character.hacksController.hitTarget(id);
        }

        protected override Text GetTextComponent(int id)
        {
            if (id / NUMBER_OF_PODS != _currentPage)
                return null;

            return base.TextComponents[id % NUMBER_OF_PODS];
        }

        internal override void OnTargetReached(int id)
        {
            base.OnTargetReached(id);

            if (!_hacksController.character.hacks.autoAdvance)
                return;

            int nextId;
            for (var x = 0; x < NUMBER_OF_HACKS; x++)
            {
                nextId = (id + x) % NUMBER_OF_HACKS;
                if (!IsTargetReached(nextId))
                {
                    if (!this[nextId])
                        this[nextId] = true;

                    break;
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _hacksController = __instance;
            _character = __instance.character;

            __instance.pods.Do((pod, index) =>
                Instance.TextComponents[index] = pod.transform.parent.parent.Find("+ Button/Text").GetComponent<Text>());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "changePage")]
        private static void HacksController_changePage_postfix()
        {
            for (var x = 0; x < NUMBER_OF_PODS; x++)
            {
                Instance.UpdateText(_hacksController.pods[x].id);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HackUIController), "add")]
        private static bool HackUIController_add_prefix(HackUIController __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[id] = !Instance[id];
                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HackUIController), "remove")]
        private static void HackUIController_remove_postfix(HackUIController __instance)
        {
            Instance[__instance.id] = false;
        }
    }
}
