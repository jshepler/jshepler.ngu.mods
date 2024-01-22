using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DefaultPlayerPortrait
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "Start")]
        private static void BossController_Start_postfix(BossController __instance)
        {
            // 184x184

            var bossId = Options.DefaultPlayerPortait.BossId.Value;
            var filename = Options.DefaultPlayerPortait.Filename.Value?.Trim() ?? string.Empty;
            var path = Path.Combine(Paths.ConfigPath, filename);

            if (filename == string.Empty && bossId == 0)
                return;

            if (filename != string.Empty && File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(184, 184);
                ImageConversion.LoadImage(tex, bytes);
                __instance.playerPortraitSprites[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }

            else
                __instance.playerPortraitSprites[0] = __instance.bossPortraitSprites[bossId - 1];
        }
    }
}
