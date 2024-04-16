using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RebirthResetAutoMergeBoost
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            var i = Plugin.Character.inventory;
            i.mergeTime.reset();
            i.mergeTime.advanceTime(2);
            i.boostTime.reset();
            i.boostTime.advanceTime(1);

            if (!Plugin.Character.arbitrary.instaTrain)
                Plugin.Character.menuSwapper.swapMenu((int)Menu.BasicTraining);

            //CookingHelper.AdjustCookingTimer();
        }
    }
}
