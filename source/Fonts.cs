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

        internal static Font Arial => _fonts["Arial"];
        internal static Font LiberationMono_BoldItalic => _fonts["LiberationMono-BoldItalic"];
        internal static Font LiberationMono_Bold => _fonts["LiberationMono-Bold"];
        internal static Font The_Bold_Font => _fonts["theboldfont"];
        internal static Font LiberationMono_Italic => _fonts["LiberationMono-Italic"];
        internal static Font LiberationMono_Regular => _fonts["LiberationMono-Regular"];
        internal static Font LiberationSans_Bold => _fonts["LiberationSans-Bold"];
        internal static Font Windows_Command_Prompt => _fonts["windows_command_prompt"];
        internal static Font LiberationSans_Regular => _fonts["LiberationSans-Regular"];
    }
}
