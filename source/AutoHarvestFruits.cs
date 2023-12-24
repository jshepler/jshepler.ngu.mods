using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoHarvestFruits
    {
        // auto harvest/eat all max tier fruits when any are max tier
        //[HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "updateFruitTimers")]
        private static void AllYggdrasil_updateFruitTimers_postfix(AllYggdrasil __instance)
        {
            if (Options.AutoHarvest.Enabled.Value && __instance.anyFruitMaxxed())
                __instance.consumeAll();
        }

        // auto harvest/east all fruits >= tier 1 on rebirth
        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_bool_prefix(Rebirth __instance)
        {
            if (Options.AutoHarvest.Enabled.Value)
                __instance.character.yggdrasilController.consumeAll(true);
        }
    }
}
