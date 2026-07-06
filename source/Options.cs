using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            NotificationToasts.Enabled = Config.Bind("NotificationToasts", "Enabled", true, "enable to separate \"timed tooltips\" into separate notifications as toasts");
            NotificationToasts.TopDown = Config.Bind("NotificationToasts", "TopDown", true, "if true, toasts are displayed top-right and go down; if false, toasts are displayed bottom-right and go up");
            Questing.AlwaysRandom = Config.Bind("Questing", "AlwaysRandom", false, "If true, new quests will always be random instead of targeting current zone");
        }

        internal static class NotificationToasts
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> TopDown;
        }

        internal static class Questing
        {
            internal static ConfigEntry<bool> AlwaysRandom;
        }
    }
}
