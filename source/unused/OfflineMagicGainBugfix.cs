using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class OfflineMagicGainBugfix
    {
        // base game overflows a long when calcuating how much magic to add, this replaces that method and uses a float
        // (like it does for energy) to check if > long.MaxValue
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "makeOfflineMagic")]
        private static bool Character_makeOfflineMagic_prefix(int seconds, Character __instance)
        {
            __instance.message = "";

            if (__instance.magic.curMagic != __instance.totalCapMagic())
            {
                var offlineMagic = seconds * __instance.magicPerSecond();
                var magicToAdd = offlineMagic >= long.MaxValue ? long.MaxValue : (long)offlineMagic;

                if (magicToAdd > __instance.totalCapMagic() - __instance.magic.curMagic)
                    magicToAdd = __instance.totalCapMagic() - __instance.magic.curMagic;

                if (magicToAdd < 0)
                    magicToAdd = 0L;

                __instance.magic.idleMagic += magicToAdd;
                __instance.magic.curMagic += magicToAdd;
                __instance.message = "Gained " + __instance.format.suffixFormat(magicToAdd) + " Magic!\n\n";
            }

            return false;
        }
    }
}
