using System.Linq;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class BloodRitualAllocator : BaseAllocator
    {
        private static AllBloodMagicController _allBM;
        private static Character character;
        private static BloodRitualAllocator Instance = new();

        internal BloodRitualAllocator() : base(8)
        {
            Allocators.Magic.Add(Allocators.Feature.BloodRitual, this);
        }

        internal override void Allocate(int id, long amount)
        {
            character.bloodMagic.ritual[id].magic += amount;
            _allBM.bloodMagics[id].updateBloodMagicText();
        }

        internal override long CalcCapDelta(int id)
        {
            if (id > character.bloodMagicController.ritualsUnlocked())
                return 0;

            var cap = _allBM.bloodMagics[id].capValue();
            var current = character.bloodMagic.ritual[id].magic;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BloodMagicController), "Start")]
        private static void BloodMagicController_Start_postfix(BloodMagicController __instance)
        {
            if (_allBM == null)
            {
                character = __instance.character;
                _allBM = character.bloodMagicController;

                _allBM.bloodMagics.Do((c, i) =>
                    Instance.TextComponents[i] = c.transform.parent.Find("Add Magic/Text").GetComponent<Text>());
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BloodMagicController), "add")]
        private static bool BloodMagicController_add_prefix(BloodMagicController __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                var rit8Unlocked = character.allChallenges.trollChallenge.completions() >= 6;

                if (Input.GetKey(KeyCode.LeftAlt))
                    Enumerable.Range(0, rit8Unlocked ? 8 : 7).Do(i => Instance[i] = !Instance[i]);

                else if (id == 7 && !rit8Unlocked)
                    return true;

                else
                    Instance[id] = !Instance[id];
                
                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BloodMagicController), "removeMagic")]
        private static void BloodMagicController_removeMagic_postfix(BloodMagicController __instance)
        {
            Instance[__instance.id] = false;
        }
    }
}
