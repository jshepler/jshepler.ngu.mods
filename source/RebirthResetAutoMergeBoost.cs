using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RebirthResetAutoMergeBoost
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix(bool hardReset, Rebirth __instance)
        {
            __instance.character.inventory.mergeTime.reset();
            __instance.character.inventory.mergeTime.advanceTime(2);
            __instance.character.inventory.boostTime.reset();
            __instance.character.inventory.boostTime.advanceTime(1);
        }
    }
}
