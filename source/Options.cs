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

            Cards.AutoSortEnabled = Config.Bind("Cards", "AutoSort.Enabled", true, "if enabled, sorts cards as they are added");
            Cards.AutoSortBy = Config.Bind("Cards", "AutoSort.By", CardSortBy.RarityFirst, "RarityFirst: rarity, type, bonus; TypeFirst: type, rarity, bonus; Efficency: based on bonus/mayo");
            Cards.AutoSortDirection = Config.Bind("Cards", "AutoSort.Direction", CardSortDirection.Ascending, "the order cards are sorted");
            Cards.AutoYeetMode = Config.Bind("Cards", "AutoYeet.Mode", CardYeetMode.Disabled, "What is used to determine when to auto yeet a card");
            Cards.MaxYeetRarity = Config.Bind("Cards", "AutoYeet.MaxYeetRarity", rarity.Crappy, "if AutoYeet.Mode is Rarity, this is a card's max rarity that will get yeeted");
            Cards.MaxYeetEfficiency = Config.Bind("Cards", "AutoYeet.MaxYeetEfficiency", 0f, "if AutoYeet.Mode is Efficiency, this is a card's max mayo efficiency that will get yeeted");
            Cards.MaxYeetVariance = Config.Bind("Cards", "AutoYeet.MaxYeetVariance", 0f, "if AutoYeet.Mode is Variance, this a card's max variance that will get yeeted, 0.8 to 1.2");
            Cards.AlwaysYeetCSV = Config.Bind("Cards", "AutoYeet.AlwaysYeet", "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0", "set in-game via F1 popup on cards screen");
            Cards.AutoProtectChonkers = Config.Bind("Cards", "AutoProtectChonkers", true, "vanilla game always protects chonkers when spawned - this makes it an option");
            Cards.ShowTooltip = Config.Bind("Cards", "ShowTooltip", false, "if enabled, shows a hover tooltip on cards with efficiency/variance details; auto sort/yeet work off the same calculations regardless of this setting");

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

        internal static class Cards
        {
            internal static ConfigEntry<bool> AutoSortEnabled;
            internal static ConfigEntry<CardSortBy> AutoSortBy;
            internal static ConfigEntry<CardSortDirection> AutoSortDirection;

            internal static ConfigEntry<CardYeetMode> AutoYeetMode;
            internal static ConfigEntry<rarity> MaxYeetRarity;
            internal static ConfigEntry<float> MaxYeetEfficiency;
            internal static ConfigEntry<float> MaxYeetVariance;
            internal static ConfigEntry<string> AlwaysYeetCSV;

            internal static ConfigEntry<bool> AutoProtectChonkers;
            internal static ConfigEntry<bool> ShowTooltip;
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
