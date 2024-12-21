using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace jshepler.ngu.mods
{
    internal static class Fonts
    {
        private static Dictionary<string, Font> _fonts;

        static Fonts()
        {
            _fonts = UnityEngine.Resources.FindObjectsOfTypeAll<Font>().ToDictionary(f => f.name, f => f);
        }

        // game's default
        internal static Font LiberationSans_Regular => _fonts["LiberationSans-Regular"];
        internal static Font LiberationSans_Bold => _fonts["LiberationSans-Bold"];

        // used in NGU logo and many menu titles
        internal static Font The_Bold_Font => _fonts["theboldfont"];

        // used in hacks menu title
        internal static Font Windows_Command_Prompt => _fonts["windows_command_prompt"];

        // I use this when I want a mono-spaced font (e.g. total time played per difficulty)
        internal static Font LiberationMono_Regular => _fonts["LiberationMono-Regular"];
        internal static Font LiberationMono_Bold => _fonts["LiberationMono-Bold"];
        internal static Font LiberationMono_Italic => _fonts["LiberationMono-Italic"];
        internal static Font LiberationMono_BoldItalic => _fonts["LiberationMono-BoldItalic"];

        // unity's default/fallback (if requested font not found)
        internal static Font Arial => _fonts["Arial"];
    }
}
