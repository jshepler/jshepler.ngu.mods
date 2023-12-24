using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FixZoneDropdownBug
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "constructDropdown")]
        private static void AdventureController_constructDropdown_postfix(AdventureController __instance)
        {
            __instance.zoneDropdown.SetValueWithoutNotify(__instance.zone == 1000 ? 1 : __instance.zone + 1);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "updateMenu")]
        private static void AdventureController_updateMenu_postfix(AdventureController __instance)
        {
            if (__instance.character.menuID == 3 && __instance.zone == 1000)
                __instance.zoneDropdown.captionText.text = __instance.zoneName(1000);
        }
    }
}
