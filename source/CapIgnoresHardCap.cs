using System;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CapIgnoresHardCap
    {
        // from NGUController.cap(), basically copy everything except using double for cap, which isn't clipped to hard cap;
        // the check for if > idle energy, idle energy would never be > hard cap, so that clips it implicitly
        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "cap")]
        private static bool NGUController_cap_prefix(NGUController __instance)
        {
            var character = __instance.character;
            var id = __instance.id;
            
            character.idleEnergy += character.NGU.skills[id].energy;
            character.NGU.skills[id].energy = 0L;
            if (character.idleEnergy == 0) return false;

            var cap = energyNGUCapAmount(__instance);
            cap = cap / Math.Ceiling(cap / character.idleEnergy) * 1.000002d;

            if (cap + 1 <= long.MaxValue) cap++;
            if (cap > character.idleEnergy) cap = character.idleEnergy;
            if (cap < 0) cap = 0;

            character.NGU.skills[id].energy += (long)cap;
            character.idleEnergy -= (long)cap;
            __instance.updateText();

            return false;
        }

        // from AllNGUController.energyNGUCapAmount(id), basically copy everything except clipping to hard cap;
        // and return double instead of long to allow higher than long.MaxValue
        private static double energyNGUCapAmount(NGUController controller)
        {
            var character = controller.character;
            var num = controller.CurrentLevel() + 1;

            double num2 = character.totalEnergyPower();
            num2 *= (double)character.totalNGUSpeedBonus();
            num2 *= (double)character.adventureController.itopod.totalEnergyNGUBonus();
            num2 *= (double)character.inventory.macguffinBonuses[4];
            num2 *= (double)character.NGUController.energyNGUBonus();
            num2 *= (double)character.allDiggers.totalEnergyNGUBonus();
            num2 *= (double)character.hacksController.totalEnergyNGUBonus();
            num2 *= (double)character.beastQuestPerkController.totalEnergyNGUSpeed();
            num2 *= (double)character.wishesController.totalEnergyNGUSpeed();
            num2 *= (double)character.cardsController.getBonus(cardBonus.energyNGUSpeed);
            if (character.allChallenges.trollChallenge.sadisticCompletions() >= 1)
            {
                num2 *= 3.0;
            }
            if (character.settings.nguLevelTrack >= difficulty.sadistic)
            {
                num2 /= (double)character.NGUController.NGU[0].sadisticDivider();
            }

            double num3 = (double)character.NGUController.energySpeedDivider(controller.id) * (double)num / num2;
            num3 *= (double)character.settings.nguCapModifier;

            if (double.IsInfinity(num3)) return double.MaxValue;
            if (num3 <= 1.0) return 1.0d;

            return num3;
        }
    }
}
