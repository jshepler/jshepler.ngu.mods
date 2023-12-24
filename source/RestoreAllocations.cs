using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RestoreAllocations
    {
        private static StoredAllocations _allocations;

        private static bool IsControlR => Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl);
        private static bool IsAltR => Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftAlt);

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            _allocations = new();

            Plugin.OnUpdate += (o, e) =>
            {
                if (IsAltR)
                    _allocations.Store(e.Character);
                    
                else if(IsControlR)
                    _allocations.Restore(e.Character);
            };
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "removeMostEnergy")]
        [HarmonyPatch(typeof(Character), "removeMostMagic")]
        [HarmonyPatch(typeof(Character), "removeAllEnergy")]
        [HarmonyPatch(typeof(Character), "removeAllMagic")]
        [HarmonyPatch(typeof(Character), "removeAllRes3")]
        [HarmonyPatch(typeof(Character), "removeAllEnergyAndMagic")]
        private static bool postfix(Character __instance, MethodBase __originalMethod)
        {
            if (IsControlR || IsAltR)
                return false;

            _allocations.Store(__instance);
            return true;
        }
    }

    internal class StoredAllocations
    {
        public long[] BT_AttackEnergy = new long[6];
        public long[] BT_DefenseEnergy = new long[6];
        public long Wandoos_Energy = 0;
        public long Wandoos_Magic = 0;
        public long[] Augs_Energy = new long[7];
        public long[] Augs_Upgrade_Energy = new long[7];
        public long TM_Energy = 0;
        public long TM_Magic = 0;
        public long[] BM_Magic = new long[8];
        public long[] AT_Energy = new long[5];
        public long[] NGU_Energy = new long[9];
        public long[] NGU_Magic = new long[7];
        public long[] Hacks_Res3 = new long[16];
        public long[] Wishes_Energy = new long[231];
        public long[] Wishes_Magic = new long[231];
        public long[] Wishes_Res3 = new long[231];

        internal void Store(Character c)
        {
            for (var x = 0; x < 6; x++)
            {
                BT_AttackEnergy[x] = c.training.attackEnergy[x];
                BT_DefenseEnergy[x] = c.training.defenseEnergy[x];
            }

            Wandoos_Energy = c.wandoos98.wandoosEnergy;
            Wandoos_Magic = c.wandoos98.wandoosMagic;

            for (var x = 0; x < 7; x++)
            {
                Augs_Energy[x] = c.augments.augs[x].augEnergy;
                Augs_Upgrade_Energy[x] = c.augments.augs[x].upgradeEnergy;
            }

            TM_Energy = c.machine.speedEnergy;
            TM_Magic = c.machine.goldMultiMagic;

            for (var x = 0; x < 8; x++)
            {
                BM_Magic[x] = c.bloodMagic.ritual[x].magic;
            }

            for (var x = 0; x < 5; x++)
            {
                AT_Energy[x] = c.advancedTraining.energy[x];
            }

            for (var x = 0; x < 9; x++)
            {
                NGU_Energy[x] = c.NGU.skills[x].energy;

                if (x < 7)
                    NGU_Magic[x] = c.NGU.magicSkills[x].magic;
            }

            for (var x = 0; x < 16; x++)
            {
                Hacks_Res3[x] = c.hacks.hacks[x].res3;
            }

            for (var x = 0; x < 231; x++)
            {
                Wishes_Energy[x] = c.wishes.wishes[x].energy;
                Wishes_Magic[x] = c.wishes.wishes[x].magic;
                Wishes_Res3[x] = c.wishes.wishes[x].res3;
            }
        }

        internal void Restore(Character c)
        {
            for (var x = 0; x < 6; x++)
            {
                RestoreValue(ref c.training.attackEnergy[x], ref c.idleEnergy, BT_AttackEnergy[x]);
                RestoreValue(ref c.training.defenseEnergy[x], ref c.idleEnergy, BT_DefenseEnergy[x]);
            }

            RestoreValue(ref c.wandoos98.wandoosEnergy, ref c.idleEnergy, Wandoos_Energy);
            RestoreValue(ref c.wandoos98.wandoosMagic, ref c.magic.idleMagic, Wandoos_Magic);

            for (var x = 0; x < 7; x++)
            {
                RestoreValue(ref c.augments.augs[x].augEnergy, ref c.idleEnergy, Augs_Energy[x]);
                RestoreValue(ref c.augments.augs[x].upgradeEnergy, ref c.idleEnergy, Augs_Upgrade_Energy[x]);
            }

            RestoreValue(ref c.machine.speedEnergy, ref c.idleEnergy, TM_Energy);
            RestoreValue(ref c.machine.goldMultiMagic, ref c.magic.idleMagic, TM_Magic);

            for (var x = 0; x < 8; x++)
            {
                RestoreValue(ref c.bloodMagic.ritual[x].magic, ref c.magic.idleMagic, BM_Magic[x]);
            }

            for (var x = 0; x < 5; x++)
            {
                RestoreValue(ref c.advancedTraining.energy[x], ref c.idleEnergy, AT_Energy[x]);
            }

            for (var x = 0; x < 9; x++)
            {
                RestoreValue(ref c.NGU.skills[x].energy, ref c.idleEnergy, NGU_Energy[x]);

                if (x < 7)
                    RestoreValue(ref c.NGU.magicSkills[x].magic, ref c.magic.idleMagic, NGU_Magic[x]);
            }

            for (var x = 0; x < 16; x++)
            {
                RestoreValue(ref c.hacks.hacks[x].res3, ref c.res3.idleRes3, Hacks_Res3[x]);
            }

            for (var x = 0; x < 231; x++)
            {
                RestoreValue(ref c.wishes.wishes[x].energy, ref c.idleEnergy, Wishes_Energy[x]);
                RestoreValue(ref c.wishes.wishes[x].magic, ref c.magic.idleMagic, Wishes_Magic[x]);
                RestoreValue(ref c.wishes.wishes[x].res3, ref c.res3.idleRes3, Wishes_Res3[x]);
            }
        }

        private void RestoreValue(ref long currentValue, ref long idle, long newValue)
        {
            if (currentValue < 0)
                currentValue = 0;

            idle += currentValue;
            
            var minValue = Math.Max(newValue, idle);
            if (minValue < 0)
                minValue = 0;

            currentValue = minValue;
            idle -= minValue;
        }

        public override string ToString()
        {
            return ToJson();
        }

        internal string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
