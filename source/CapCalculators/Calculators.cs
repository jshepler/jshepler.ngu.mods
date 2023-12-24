using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods.CapCalculators
{
    [HarmonyPatch]
    internal class Calculators
    {
        internal static Dictionary<int, NGU_EnergyCalculator> NGU_EnergyCalculators = new();
        internal static Dictionary<int, NGU_MagicCalculator> NGU_MagicCalculators = new();
        internal static TM_EnergyCalculator TM_EnergyCalculator;
        internal static TM_MagicCalculator TM_MagicCalculator;
        internal static Dictionary<int, IBeardCalculator> BeardCalculators = new();
        internal static Dictionary<int, AdvancedTrainingCalculator> AdvancedTrainingCalculators;
        internal static Dictionary<int, AugCalculator> AugCalculators = new();
        internal static Dictionary<int, AugUpgradeCalculator> AugUpgradeCalculators = new();

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            var atc = __instance.advancedTrainingController;
            AdvancedTrainingCalculators = Enumerable.Range(0, 6).ToDictionary(i => i, i => new AdvancedTrainingCalculator(atc, i));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllNGUController), "Start")]
        private static void AllNGUController_Start_postfix(AllNGUController __instance)
        {
            __instance.NGU.Do((c, i) => NGU_EnergyCalculators.Add(i, new NGU_EnergyCalculator(c)));
            __instance.NGUMagic.Do((c, i) => NGU_MagicCalculators.Add(i, new NGU_MagicCalculator(c)));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            TM_EnergyCalculator = new TM_EnergyCalculator(__instance);
            TM_MagicCalculator = new TM_MagicCalculator(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllBeardsController), "Start")]
        private static void AllBearsController_Start_postfix(AllBeardsController __instance)
        {
            __instance.character.beards.beards.Do((b, i) =>
            {
                if (__instance.usesEnergy[i])
                    BeardCalculators.Add(i, new Beard_EnergyCalculator(__instance, i));
                else
                    BeardCalculators.Add(i, new Beard_MagicCalculator(__instance, i));
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllAugsController), "Start")]
        private static void AllAugsController_Start_postfix(AllAugsController __instance)
        {
            __instance.augments.Do((c, i) =>
            {
                AugCalculators.Add(i, new AugCalculator(__instance, i));
                AugUpgradeCalculators.Add(i, new AugUpgradeCalculator(__instance, i));
            });
        }
    }
}
