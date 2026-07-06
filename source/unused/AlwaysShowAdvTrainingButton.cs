using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AlwaysShowAdvTrainingButton
    {
        private static bool _enabled => Options.Experimental.AlwaysShowAT.Value;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance, Text ___advancedTrainingText)
        {
            if (_enabled)
            {
                __instance.advancedTraining.interactable = true;
                ___advancedTrainingText.text = "Adv. Training";
            }
        }
    }
}
