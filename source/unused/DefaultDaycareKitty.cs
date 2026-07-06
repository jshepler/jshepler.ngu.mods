using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DefaultDaycareKitty
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AllDaycareController), "Start")]
        private static void AllDaycareController_Start_postfix(AllDaycareController __instance)
        {
            // 250x110

            var filename = Options.DefaultDaycareKitty.Filename.Value?.Trim();
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists($"{Paths.ConfigPath}\\{filename}"))
                return;

            var bytes = File.ReadAllBytes($"{Paths.ConfigPath}\\{filename}");
            var tex = new Texture2D(890, 390);
            ImageConversion.LoadImage(tex, bytes);
            __instance.kittySprites[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }
    }
}
