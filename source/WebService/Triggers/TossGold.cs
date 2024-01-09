using System.Collections;
using System.Reflection;
using UnityEngine;

namespace jshepler.ngu.mods.WebService.Triggers
{
    internal class TossGold
    {
        private static object[] _noArgs = new object[0];
        private static WaitForSeconds _menuPause = new WaitForSeconds(0.25f);
        private static WaitForSeconds _rewardPause = new WaitForSeconds(5f);
        private static MethodInfo _tossGoldMethod = typeof(PitController).GetMethod("engage", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static bool IsRunning = false;

        internal static IEnumerator Run()
        {
            IsRunning = true;

            var currentMenu = Plugin.Character.CurrentMenu();
            Plugin.Character.menuSwapper.SwapMenu(Menu.MoneyPit);
            yield return _menuPause;

            _tossGoldMethod.Invoke(Plugin.Character.pitController, _noArgs);
            yield return _rewardPause;

            Plugin.Character.menuSwapper.SwapMenu(currentMenu);
            IsRunning = false;
        }
    }
}
