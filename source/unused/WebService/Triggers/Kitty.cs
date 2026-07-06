using System.Collections;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.WebService.Triggers
{
    [HarmonyPatch]
    internal class Kitty
    {
        internal static bool IsRunning = false;

        [HarmonyPostfix, HarmonyPatch(typeof(TrollChallengeController), "Start")]
        private static void TrollChallengeController_Start_postfix(TrollChallengeController __instance)
        {
            // 900x600

            var filename = Options.TrollKitty.Filename.Value?.Trim() ?? string.Empty;
            var path = Path.Combine(Paths.ConfigPath, filename);

            if (filename == string.Empty)
                return;

            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(900, 600);
            ImageConversion.LoadImage(tex, bytes);
            __instance.kitty.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }

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
