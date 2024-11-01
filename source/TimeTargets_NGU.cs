using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TimeTargets_NGU
    {
        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "cap")]
        private static bool NGUController_cap_prefix(NGUController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return true;

            var character = __instance.character;
            var calc = Calculators.NGU_EnergyCalculators[__instance.id];
            var resource = character.NGU.skills[__instance.id].energy;
            if (resource == 0)
                resource = character.totalCapEnergy();

            var runTimeSeconds = character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return false;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = __instance.CurrentLevel();

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            __instance.target.text = targetLevel.ToString();
            __instance.setTarget();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUMagicController), "cap")]
        private static bool NGUMagicController_cap_prefix(NGUMagicController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return true;

            var character = __instance.character;
            var calc = Calculators.NGU_MagicCalculators[__instance.id];
            var resource = character.NGU.magicSkills[__instance.id].magic;
            if (resource == 0)
                resource = character.totalCapMagic();

            var runTimeSeconds = character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return false;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = __instance.CurrentLevel();

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            __instance.magicTarget.text = targetLevel.ToString();
            __instance.setTarget();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "capAllEnergy")]
        private static bool AllNGUController_capAllEnergy_prefix(AllNGUController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return true;

            __instance.NGU.Do(ngu => ngu.cap());
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllNGUController), "capAllMagic")]
        private static bool AllNGUController_capAllMagic_prefix(AllNGUController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return true;

            __instance.NGUMagic.Do(ngu => ngu.cap());
            return false;
        }
    }
}
