using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StartInAdventure
    {
        private static bool _splashScreenClosing = false;

        [HarmonyPrefix, HarmonyPatch(typeof(OfflineProgressSplashScreen), "closeScreen")]
        private static void OfflineProgressSplashScreen_closeScreen_prefix()
        {
            _splashScreenClosing = true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MenuSwapper), "swapMenu")]
        private static void MenuSwapper_swapMenu_prefix(ref int menuIn)
        {
            if (_splashScreenClosing && Plugin.Character.settings.inventoryOn)
                menuIn = (int)Menu.Inventory;

            _splashScreenClosing = false;
        }
    }
}
