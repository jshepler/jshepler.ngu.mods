using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareShockwaveFix
    {
        // vanilla doesn't check if item is a macguffin to allow it to go above level 100

        [HarmonyPrefix, HarmonyPatch(typeof(Equipment), "levelUp")]
        private static bool Equipment_levelUp_prefix(Equipment __instance)
        {
            if (!__instance.isMacGuffin())
                return true;

            __instance.level++;
            return false;
        }
    }
}
