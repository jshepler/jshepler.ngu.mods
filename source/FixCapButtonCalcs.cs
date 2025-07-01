using System;
using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FixCapButtonCalcs
    {
        // NGUs
        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "cap")]
        private static bool NGUController_cap_prefix(NGUController __instance)
        {
            // ctrl-click handled in TimeTargets_NGU.cs
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                return false;

            var id = __instance.id;
            var character = __instance.character;
            
            var idle = character.idleEnergy + character.NGU.skills[id].energy;
            if (idle <= 0)
                return true;

            var cap = Calculators.NGU_EnergyCalculators[id].ResourceFromLevel(__instance.CurrentLevel() + 1) * character.settings.nguCapModifier;
            var allocate = (cap / Math.Ceiling(cap / idle)).CeilToLong();

            idle -= allocate;
            character.idleEnergy = idle;
            character.NGU.skills[id].energy = allocate;
            __instance.updateText();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUMagicController), "cap")]
        private static bool NGUMagicController_cap_prefix(NGUMagicController __instance)
        {
            // ctrl-click handled in TimeTargets_NGU.cs
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                return false;

            var id = __instance.id;
            var character = __instance.character;

            var idle = character.magic.idleMagic + character.NGU.magicSkills[id].magic;
            if (idle <= 0)
                return true;

            var cap = Calculators.NGU_MagicCalculators[id].ResourceFromLevel(__instance.CurrentLevel() + 1);
            var allocate = cap.CeilToLong();

            if (idle < allocate)
                allocate = (cap / Math.Ceiling(cap / idle)).CeilToLong();
            else
                allocate = Math.Min(idle, (allocate * character.settings.nguCapModifier).CeilToLong());

            idle -= allocate;
            character.magic.idleMagic = idle;
            character.NGU.magicSkills[id].magic = allocate;
            __instance.updateText();

            return false;
        }

        // wandoos
        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "addCapEnergy")]
        private static bool Wandoos98Controller_addCapEnergy_prefix(Wandoos98Controller __instance)
        {
            var character = __instance.character;
            var idle = character.idleEnergy + character.wandoos98.wandoosEnergy;
            if (idle <= 0)
                return true;

            var cap = (double)__instance.baseEnergyTime() / (double)character.totalWandoosEnergySpeed() * 1.000002;
            var allocate = cap.CeilToLong();

            if(idle < allocate)
                allocate = (cap / Math.Ceiling(cap / idle)).CeilToLong();

            idle -= allocate;
            character.idleEnergy = idle;
            character.wandoos98.wandoosEnergy = allocate;
            __instance.updateText();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "addCapMagic")]
        private static bool Wandoos98Controller_addCapMagic_prefix(Wandoos98Controller __instance)
        {
            var character = __instance.character;
            var idle = character.magic.idleMagic + character.wandoos98.wandoosMagic;
            if (idle <= 0)
                return true;

            var cap = (double)__instance.baseMagicTime() / (double)character.totalWandoosMagicSpeed() * 1.000002;
            var allocate = cap.CeilToLong();
            if (idle < allocate)
                allocate = (cap / Math.Ceiling(cap / idle)).CeilToLong();

            idle -= allocate;
            character.magic.idleMagic = idle;
            character.wandoos98.wandoosMagic = allocate;
            __instance.updateText();

            return false;
        }

        // blood rituals
        [HarmonyPrefix, HarmonyPriority(2), HarmonyPatch(typeof(BloodMagicController), "cap")]
        private static bool BloodMagicController_capValue_prefix(BloodMagicController __instance)
        {
            // PartialCaps mod handles this
            if (Input.GetKey(KeyCode.LeftShift))
                return true;

            var id = __instance.id;
            var character = __instance.character;
            var controller = character.bloodMagicController;

            if (id >= controller.ritualsUnlocked())
                return true;

            var idle = character.magic.idleMagic + character.bloodMagic.ritual[id].magic;
            if (idle <= 0)
                return true;

            var cap = calcBloodRitualCap(id);
            var allocate = cap.CeilToLong();

            if (idle < allocate)
                allocate = (cap / Math.Ceiling(cap / idle)).CeilToLong();

            idle -= allocate;
            character.magic.idleMagic = idle;
            character.bloodMagic.ritual[id].magic = allocate;
            __instance.updateBloodMagicText();

            return false;
        }

        internal static double calcBloodRitualCap(int id)
        {
            var character = Plugin.Character;
            var controller = character.bloodMagicController;

            double power = character.totalMagicPower();
            double speedBonus = controller.bloodMagics[id].totalBloodMagicSpeedBonus();

            double speedDivider = character.settings.rebirthDifficulty switch
            {
                difficulty.normal => controller.normalSpeedDividers[id] * 50000.0,
                difficulty.evil => controller.evilSpeedDividers[id] * 50000.0,
                difficulty.sadistic => controller.sadisticSpeedDividers[id] * 5E+08f,
                _ => 0
            };

            return speedDivider / (power * speedBonus) * 1.000002;
        }
    }
}
