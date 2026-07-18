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

            WishList.Enabled = Config.Bind("WishList", "Enabled", false, "enables the wish list automation");
            WishList.AutoAdvance = Config.Bind("WishList", "Auto Advance", true, "if enabled and a wish finishes, start the next wish");
            WishList.SingleLevelMode = Config.Bind("WishList", "SingleLevelMode", false, "if enabled, wishes gain a single level then starts the next one in current list or sort order");
            WishList.BlacklistMode = Config.Bind("WishList", "BlacklistMode", false, "if enabled, listed wishes will be ignored when starting next wish");
            WishR3Cap.Enabled = Config.Bind("WishR3Cap", "Enabled", true, "when auto-allocating resources or when a wish completes a level, will (re)distribute R3 amongst running wishes to not be more than is needed for min wish time");

            Yggdrasil.AutoHarvest = Config.Bind("Yggdrasil", "AutoHarvest", false, "enable auto harvest/eat fruits when fully grown (max tier)");

            Experimental.LoadoutSwapKeepsAutoAllocators = Config.Bind("Experimental", "LoadoutSwapKeepsAutoAllocators", false, "if enabled, and have the game setting 'Unassign E/M on Loadout Swap' enabled, auto allocators won't disable on loadout swap");
            Experimental.ModPopupScaling = Config.Bind("Experimental", "ModPopupScaling", 1.0f, "additional scaling multiplier on mod popups, applied after game resolution and dpi (windows scaling)");
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

        internal static class WishList
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> AutoAdvance;
            internal static ConfigEntry<bool> SingleLevelMode;
            internal static ConfigEntry<bool> BlacklistMode;
        }

        internal static class WishR3Cap
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class Yggdrasil
        {
            internal static ConfigEntry<bool> AutoHarvest;
        }

        internal static class Experimental
        {
            internal static ConfigEntry<bool> LoadoutSwapKeepsAutoAllocators;
            internal static ConfigEntry<float> ModPopupScaling;
        }
    }
}
