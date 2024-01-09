using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.WebService.Triggers
{
    [HarmonyPatch]
    internal class Kitty
    {
        internal static bool IsRunning = false;

        [HarmonyPostfix, HarmonyPatch(typeof(TrollChallengeController), "trollKittyOff")]
        private static void TrollChallengeController_trollKittyOff_postfix()
        {
            IsRunning = false;
        }

        private static WaitUntil _waitUntilKittyStopped = new WaitUntil(() => !IsRunning);
        internal static IEnumerator Run()
        {
            IsRunning = true;

            var kitty = Plugin.Character.allChallenges.trollChallenge.kitty;
            kitty.transform.SetAsLastSibling();
            kitty.color = Color.white;
            kitty.gameObject.transform.localPosition = new Vector3(0f, -550f);

            return _waitUntilKittyStopped;
        }
    }
}
