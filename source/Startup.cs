using System;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Startup
    {
        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                parseArgs();
                setCustomRes();
                setCustomCulture();
            };
        }

        private static void parseArgs()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 2)
            {
                for (var x = 1; x < args.Length - 1; x++)
                {
                    var arg = args[x].ToLowerInvariant();

                    if (arg == "-game")
                    {
                        AutoSaves.GameName = args[x + 1];
                        Fullscreen.SetWindowTitle($"NGU Idle - {args[x + 1]}");
                    }

                    else if (arg == "-vsynccount" && int.TryParse(args[x + 1], out var count))
                        QualitySettings.vSyncCount = count;

                    else if (arg == "-targetframerate" && int.TryParse(args[x + 1], out var rate))
                    {
                        QualitySettings.vSyncCount = 0;
                        Application.targetFrameRate = rate;
                    }
                }
            }
        }

        private static void setCustomRes()
        {
            var width = Options.CustomResolution.Width.Value;
            var height = Options.CustomResolution.Height.Value;

            if (width > 0 && height > 0)
                Screen.SetResolution(width, height, false);
        }

        private static void setCustomCulture()
        {
            if (Options.OverrideCulture.Enabled.Value == false)
                return;

            var localString = Options.OverrideCulture.Locale.Value;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(localString, false);
            }

            catch { }
        }
    }
}
