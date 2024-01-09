using System.Collections;
using UnityEngine;

namespace jshepler.ngu.mods.WebService.Triggers
{
    internal class FightBoss
    {
        private static WaitForSeconds _bossDiedPause = new WaitForSeconds(.5f);

        internal static bool IsRunning = false;

        internal static IEnumerator Run()
        {
            IsRunning = true;

            var currentMenu = Plugin.Character.CurrentMenu();
            Plugin.Character.menuSwapper.SwapMenu(Menu.FightBoss);

            yield return mods.FightBoss.RunFight();
            yield return _bossDiedPause;

            Plugin.Character.menuSwapper.SwapMenu(currentMenu);
            IsRunning = false;
        }
    }
}
