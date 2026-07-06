using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrollRestoreBasicTraining
    {
        [HarmonyPostfix,
            HarmonyPatch(typeof(TrollChallengeController), "wipeEnergy"),
            HarmonyPatch(typeof(TrollChallengeController), "wipeEnergyMagic"),
            HarmonyPatch(typeof(TrollChallengeController), "disableEquipment")]
        private static void TrollChallengeController_wipeEnergy_postfix(TrollChallengeController __instance)
        {
            var character = __instance.character;

            if (character.arbitrary.instaTrain)
            {
                character.idleEnergy -= 12L;
                character.training.attackEnergy[0] += 6L;
                character.training.defenseEnergy[0] += 6L;
            }
        }
    }
}
