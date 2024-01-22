using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            Options.Allocators.AutoAllocatorEnabled = Config.Bind("Allocators", "AutoAllocator.Enabled", false, "auto allocates idle resources, maintaining speed cap");
            Options.Allocators.OverCapAllocatorEnabled = Config.Bind("Allocators", "OverCapAllocator.Enabled", false, "allocates enough of a resource to keep BB until target level or RB time");
            Options.Allocators.RatioSplitAllocatorEnabled = Config.Bind("Allocators", "RatioSplitAllocator.Enabled", false, "splits resource between bars, using their ratios, to keep even leveling speed");

            Options.AutoHarvest.Enabled = Config.Bind("AutoHarvest", "Enabled", false, "enable auto harvest/eat fruits when fully grown (max tier)");
            Options.AutoSnipe.TargetZone = Config.Bind("AutoSnipe", "TargetZone", 0, "used to target specific enemy in specific zone, other zones always snipe bosses; enter zone number (from wiki: https://ngu-idle.fandom.com/wiki/Adventure_Mode#Zones)");
            Options.AutoSnipe.TargetEnemy = Config.Bind("AutoSnipe", "TargetEnemy", 0, "used to target specific enemy in specific zone; enter enemy number (from bestiary), 0 = bosses");

            Options.CheckForNewVersion.Enabled = Config.Bind("CheckForNewVersion", "Enabled", true, "checks for new version when loading a save and every hour after");

            Options.DefaultDaycareKitty.Filename = Config.Bind("DefaultDaycareKitty", "Filename", "", "filename of 250x110 image in config folder, used to replace default kitty sprite, leave empty to disable");
            Options.DefaultPlayerPortait.BossId = Config.Bind("DefaultPlayerPortait", "BossId", 0, "replaces default player portrait with the portrait of boss id (enemy # from bestiary), 0 = disabled");
            Options.DefaultPlayerPortait.Filename = Config.Bind("DefaultPlayerPortait", "Filename", "", "filename of 184x184 image in config folder, used to replace default player portrait (overrides BossId option), leave empty to disable");
            Options.TrollKitty.Filename = Config.Bind("TrollKitty", "Filename", "", "filename of 900x600 image in config folder, used to replace troll kitty sprite, leave empty to disable");

            Options.DropTableTooltip.Enabled = Config.Bind("DropTableTooltip", "Enabled", true, "enables display of zones' Drop Table tooltip by holding the alt key");
            Options.DropTableTooltip.OnlyUnlocked = Config.Bind("DropTableTooltip", "OnlyUnlocked", true, "if true, only items that meet their drop conditions will be displayed");
            Options.DropTableTooltip.UnknownItems = Config.Bind("DropTableTooltip", "UnknownItems", DropTableTooltip.UnknownItemDisplay.Blur, "how unknown items (not yet dropped) are displayed; Blur replaces names with \"????\"");

            Options.NotificationToasts.Enabled = Config.Bind("NotificationToasts", "Enabled", true, "enable to separate \"timed tooltips\" into separate notifications as toasts");
            Options.NotificationToasts.TopDown = Config.Bind("NotificationToasts", "TopDown", true, "if true, toasts are displayed top-right and go down; if false, toasts are displayed bottom-right and go up");

            Options.PruneSaves.DaysToKeep = Config.Bind("PruneSaves", "DaysToKeep", 0, "When quick/auto saving, will delete saves older than value; 0 = disabled");

            Options.RemoteTriggers.Enabled = Config.Bind("RemoteTriggers", "Enabled", false, "enables receiving of remote commands");
            Options.RemoteTriggers.UrlPrefix = Config.Bind("RemoteTriggers", "Prefix", "http://localhost:8088/ngu/", "urls must start with this prefix else will be ignored");
            Options.RemoteTriggers.AutoBoost.Enabled = Config.Bind("RemoteTriggers.AutoBoost", "Enabled", true, "enables auto-boost trigger");
            Options.RemoteTriggers.AutoMerge.Enabled = Config.Bind("RemoteTriggers.AutoMerge", "Enabled", true, "enables auto-merge trigger");
            Options.RemoteTriggers.TossGold.Enabled = Config.Bind("RemoteTriggers.TossGold", "Enabled", true, "enables toss gold trigger");
            Options.RemoteTriggers.FightBoss.Enabled = Config.Bind("RemoteTriggers.FightBoss", "Enabled", true, "enables fight boss trigger");
            Options.RemoteTriggers.Kitty.Enabled = Config.Bind("RemoteTriggers.Kitty", "Enabled", true, "enables kitty trigger");
        }

        internal static class RemoteTriggers
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<string> UrlPrefix;

            internal static class AutoBoost
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class AutoMerge
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class TossGold
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class FightBoss
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class Kitty
            {
                internal static ConfigEntry<bool> Enabled;
            }
        }

        internal static class AutoHarvest
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class AutoSnipe
        {
            internal static ConfigEntry<int> TargetZone;
            internal static ConfigEntry<int> TargetEnemy;
        }

        internal static class DropTableTooltip
        {
            internal enum UnknownItemDisplay { Show, Blur, Hide }

            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> OnlyUnlocked;
            internal static ConfigEntry<UnknownItemDisplay> UnknownItems;
        }

        internal static class Allocators
        {
            internal static ConfigEntry<bool> AutoAllocatorEnabled;
            internal static ConfigEntry<bool> OverCapAllocatorEnabled;
            internal static ConfigEntry<bool> RatioSplitAllocatorEnabled;
        }

        internal static class DefaultPlayerPortait
        {
            internal static ConfigEntry<int> BossId;
            internal static ConfigEntry<string> Filename;
        }

        internal static class PruneSaves
        {
            internal static ConfigEntry<int> DaysToKeep;
        }

        internal static class NotificationToasts
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> TopDown;
        }

        internal static class DefaultDaycareKitty
        {
            internal static ConfigEntry<string> Filename;
        }

        internal static class TrollKitty
        {
            internal static ConfigEntry<string> Filename;
        }

        internal static class CheckForNewVersion
        {
            public static ConfigEntry<bool> Enabled;
        }
    }
}
