using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class YggRedWhenInactiveFruits
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance)
        {
            if (Options.FruitActivationIndicator.Enabled.Value == true
                && __instance.character.yggdrasil.fruits.Any(f => f.maxTier > 0 && !f.activated))
                __instance.yggdrasil.image.color = Plugin.ButtonColor_Red;
        }
    }
}
