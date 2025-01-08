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
        internal static bool HardcoreRequested = false;
        internal static EventHandler AfterStartup;

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
                setVersion();

                AfterStartup?.Invoke(null, EventArgs.Empty);
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
                        x++;
                    }

                    else if (arg == "-vsynccount" && int.TryParse(args[x + 1], out var count))
                    {
                        QualitySettings.vSyncCount = count;
                        x++;
                    }

                    else if (arg == "-targetframerate" && int.TryParse(args[x + 1], out var rate))
                    {
                        QualitySettings.vSyncCount = 0;
                        Application.targetFrameRate = rate;
                        x++;
                    }

                    else if (arg == "-hc")
                        HardcoreRequested = true;
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

        private static void setVersion()
        {
            var build = Plugin.Character.getVersionAsString();
            var mod = $"(jshepler mods {PluginInfo.PLUGIN_VERSION})";
            var hc = Hardcore.Enabled ? " [HC]" : string.Empty;
            var text = $"<b>Build {build}{hc}</b>\n{mod}";

            var bt = Plugin.Character.mainMenu.buildText;
            bt.text = text;
            bt.resizeTextForBestFit = true;
            bt.alignment = TextAnchor.MiddleCenter;

            var vn = Plugin.Character.versionNumbering.versionNumber;
            vn.text = text;
            vn.resizeTextForBestFit = true;
        }
    }
}
