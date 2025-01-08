using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            AutoSnipe.TargetZone = Config.Bind("AutoSnipe", "TargetZone", 0, "used to target specific enemy in specific zone, other zones always snipe bosses; enter zone number (from wiki: https://ngu-idle.fandom.com/wiki/Adventure_Mode#Zones)");
            AutoSnipe.TargetEnemy = Config.Bind("AutoSnipe", "TargetEnemy", 0, "used to target specific enemy in specific zone; enter enemy number (from bestiary), 0 = bosses");
            AutoMergeTransform.Enabled = Config.Bind("AutoMergeTransform", "Enabled", false, "enables/disables auto merging and transforming of pendants and looties");

            Cards.AutoSortEnabled = Config.Bind("Cards", "AutoSort.Enabled", true, "if enabled, sorts cards as they are added");
            Cards.AutoSortBy = Config.Bind("Cards", "AutoSort.By", CardSortBy.RarityFirst, "RarityFirst: rarity, type, bonus; TypeFirst: type, rarity, bonus; Efficency: based on bonus/mayo");
            Cards.AutoSortDirection = Config.Bind("Cards", "AutoSort.Direction", CardSortDirection.Ascending, "the order cards are sorted");
            Cards.AutoYeetEnabled = Config.Bind("Cards", "AutoYeet.Enabled", false, "if enabled, will yeet cards as they are added, at or below the configured max rarity");
            Cards.MaxYeetRarity = Config.Bind("Cards", "AutoYeet.MaxYeetRarity", rarity.Crappy, "if AutoYeet is enabled, this is the max rarity that will get yeeted");
            Cards.MaxYeetEfficiency = Config.Bind("Cards", "AutoYeet.MaxYeetEfficiency", 0f, "if set, overrides MaxYeetRarity and will yeet cards up to specified efficency, 0.0 to 1.0 (100%), 0 = disabled");
            Cards.AlwaysYeetCSV = Config.Bind("Cards", "AutoYeet.AlwaysYeet", "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0", "set in-game via F1 popup on cards screen");
            Cards.AutoProtectChonkers = Config.Bind("Cards", "AutoProtectChonkers", true, "vanilla game always protects chonkers when spawned - this makes it an option");

            CheckForNewVersion.Enabled = Config.Bind("CheckForNewVersion", "Enabled", true, "checks for new version when loading a save and every hour after");
            CustomResolution.Width = Config.Bind("CustomResolution", "Width", 0, "custom resolution width, 0 = disabled");
            CustomResolution.Height = Config.Bind("CustomResolution", "Height", 0, "custom resolution height, 0 = disabled");

            DefaultDaycareKitty.Filename = Config.Bind("DefaultDaycareKitty", "Filename", "", "filename of 250x110 image in config folder, used to replace default kitty sprite, leave empty to disable");
            DefaultPlayerPortait.BossId = Config.Bind("DefaultPlayerPortait", "BossId", 0, "replaces default player portrait with the portrait of boss id (enemy # from bestiary), 0 = disabled");
            DefaultPlayerPortait.Filename = Config.Bind("DefaultPlayerPortait", "Filename", "", "filename of 184x184 image in config folder, used to replace default player portrait (overrides BossId option), leave empty to disable");
            TrollKitty.Filename = Config.Bind("TrollKitty", "Filename", "", "filename of 900x600 image in config folder, used to replace troll kitty sprite, leave empty to disable");

            DiggerUpggradeIndicator.Enabled = Config.Bind("DiggerUpggradeIndicator", "Enabled", false, "if enabled, digger button will light up yellow if any digger can be upgraded");
            DropTableTooltip.Enabled = Config.Bind("DropTableTooltip", "Enabled", true, "enables display of zones' Drop Table tooltip by holding the alt key");
            DropTableTooltip.OnlyUnlocked = Config.Bind("DropTableTooltip", "OnlyUnlocked", true, "if true, only items that meet their drop conditions will be displayed");
            DropTableTooltip.UnknownItems = Config.Bind("DropTableTooltip", "UnknownItems", DropTableTooltip.UnknownItemDisplay.Blur, "how unknown items (not yet dropped) are displayed; Blur replaces names with \"????\"");

            GameModes.Hardcore = Config.Bind("GameModes", "Hardcore", false, "enable to disable loading local saves and game ends when player dies - cloud save erased; MUST START NEW GAME TO GO INTO EFFECT");
            GameModes.PermaTC = Config.Bind("GameModes", "PermaTC", false, "enable to permanently spawn trolls every 2 minutes, every 5th a big troll; MUST START NEW GAME TO GO INTO EFFECT");

            NotificationToasts.Enabled = Config.Bind("NotificationToasts", "Enabled", true, "enable to separate \"timed tooltips\" into separate notifications as toasts");
            NotificationToasts.TopDown = Config.Bind("NotificationToasts", "TopDown", true, "if true, toasts are displayed top-right and go down; if false, toasts are displayed bottom-right and go up");
            OverrideCulture.Enabled = Config.Bind("OverrideCulture", "Enabled", false, "if enabled, uses the specified locale string to override your system's current culture for the game - ONLY AFFECTS NUMBER FORMATTING");
            OverrideCulture.Locale = Config.Bind("OverrideCulture", "Locale", "en-US", "locale string used if OverrideCulture.Enabled is true; examples: de-DE, fr-FR");
            ResourceNames.ShowFullName = Config.Bind("ResourceNames", "ShowFullName", ShowFullResourceName.None, "Which of the top-left bars to show full names instead of first letter");

            PruneSaves.DaysToKeep = Config.Bind("PruneSaves", "DaysToKeep", 0, "When quick/auto saving, will delete saves older than value; 0 = disabled");
            Questing.AlwaysRandom = Config.Bind("Questing", "AlwaysRandom", false, "If true, new quests will always be random instead of targeting current zone");
            Questing.AutoButter = Config.Bind("Questing", "AutoButter", false, "If true, will automatically use butter when starting a major quest");

            RemoteTriggers.Enabled = Config.Bind("RemoteTriggers", "Enabled", false, "enables receiving of remote commands");
            RemoteTriggers.UrlPrefix = Config.Bind("RemoteTriggers", "Prefix", "http://localhost:8088/ngu/", "urls must start with this prefix else will be ignored");
            RemoteTriggers.AutoBoost.Enabled = Config.Bind("RemoteTriggers.AutoBoost", "Enabled", true, "enables auto-boost trigger");
            RemoteTriggers.AutoMerge.Enabled = Config.Bind("RemoteTriggers.AutoMerge", "Enabled", true, "enables auto-merge trigger");
            RemoteTriggers.TossGold.Enabled = Config.Bind("RemoteTriggers.TossGold", "Enabled", true, "enables toss gold trigger");
            RemoteTriggers.FightBoss.Enabled = Config.Bind("RemoteTriggers.FightBoss", "Enabled", true, "enables fight boss trigger");
            RemoteTriggers.Kitty.Enabled = Config.Bind("RemoteTriggers.Kitty", "Enabled", true, "enables kitty trigger");

            Twitch.Enabled = Config.Bind("Twitch", "Enabled", false, "Enables twitch integration");
            Twitch.AutoConnect = Config.Bind("Twitch", "AutoConnect", false, "Connects to twitch when game starts");
            Twitch.ClientId = Config.Bind("Twitch", "ClientId", "", "The client id for the registered twitch application (see README)");
            Twitch.ClientSecret = Config.Bind("Twitch", "ClientSecret", "", "The client secret from the registered twitch applicatino (see README)");
            Twitch.AppAccessToken = Config.Bind("Twitch", "AppAccessToken", "", "The stored access token for the app - set automatically");
            Twitch.UserAccessToken = Config.Bind("Twitch", "UserAccessToken", "", "The stored access token for the current user - set automatically");
            Twitch.UserRefreshToken = Config.Bind("Twitch", "UserRefreshToken", "", "The stored refresh token for the current user - set automatically");

            Twitch.RewardTriggers.Merge = Config.Bind("Twitch.RewardTriggers", "Merge", "", "Custom reward name to trigger merge");
            Twitch.RewardTriggers.Boost = Config.Bind("Twitch.RewardTriggers", "Boost", "", "Custom reward name to trigger boost");
            Twitch.RewardTriggers.MergeBoost = Config.Bind("Twitch.RewardTriggers", "MergeBoost", "", "Custom reward name to trigger merge+boost");
            Twitch.RewardTriggers.FightBoss = Config.Bind("Twitch.RewardTriggers", "FightBoss", "", "Custom reward name to trigger boss fight");
            Twitch.RewardTriggers.TossGold = Config.Bind("Twitch.RewardTriggers", "TossGold", "", "Custom reward name to toss gold into money pit");
            Twitch.RewardTriggers.Kitty = Config.Bind("Twitch.RewardTriggers", "Kitty", "", "Custom reward name to trigger troll kitty event");

            WishList.Enabled = Config.Bind("WishList", "Enabled", false, "enables the wish list automation");
            WishList.AutoAdvance = Config.Bind("WishList", "Auto Advance", true, "if enabled and a wish finishes, start the next wish");
            WishList.SingleLevelMode = Config.Bind("WishList", "SingleLevelMode", false, "if enabled, wishes gain a single level then starts the next one in current list or sort order");
            WishList.BlacklistMode = Config.Bind("WishList", "BlacklistMode", false, "if enabled, listed wishes will be ignored when starting next wish");
            WishR3Cap.Enabled = Config.Bind("WishR3Cap", "Enabled", true, "when auto-allocating resources or when a wish completes a level, will (re)distribute R3 amongst running wishes to not be more than is needed for min wish time");

            LSCreminder.MaxMinutesToTarget = Config.Bind("LSCreminder", "MaxMinutesToTarget", 5, "max time to target for both laser sword and quadruple laser sword together, will light up Challenges button on Rebirth screen; 0 = disabled");
            Yggdrasil.ActivationIndicator = Config.Bind("Yggdrasil", "ActivationIndicator", false, "when enabled, the Yggdrasil button will light up red if any fruit needs activation");
            Yggdrasil.AutoHarvest = Config.Bind("Yggdrasil", "AutoHarvest", false, "enable auto harvest/eat fruits when fully grown (max tier)");
            Yggdrasil.PoopAudioChance = Config.Bind("Yggdrasil", "PoopAudioChance", 0f, "chance a fart audio clip is played when gain fruit, 0.0 to 1.0");

            // when loading an old version of the cfg file, some options may have changed or been removed;
            // this will check for known things that have changed and copy values if appropriate,
            // then remove all orphaned entries
            var orphaned = (Dictionary<ConfigDefinition, string>)typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Config);
            if (orphaned.Count > 0)
            {
                string value;

                // WishQueue was renamed to WishList in 1.16
                if (orphaned.TryGetValue("WisheQueue", "Enabled", out value))
                    WishList.Enabled.Value = value == "true";

                // AutoCards renamed to Cards in 1.17
                if (orphaned.TryGetValue("AutoCards", "AutoSort.Enabled", out value))
                    Cards.AutoSortEnabled.Value = value == "true";

                if (orphaned.TryGetValue("AutoCards", "AutoSort.By", out value))
                    Cards.AutoSortBy.Value = (CardSortBy)Enum.Parse(typeof(CardSortBy), value);

                if (orphaned.TryGetValue("AutoCards", "AutoSort.Direction", out value))
                    Cards.AutoSortDirection.Value = (CardSortDirection)Enum.Parse(typeof(CardSortDirection), value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.Enabled", out value))
                    Cards.AutoYeetEnabled.Value = value == "true";

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.MaxYeetRarity", out value))
                    Cards.MaxYeetRarity.Value = (rarity)Enum.Parse(typeof(rarity), value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.MaxYeetEfficiency", out value))
                    Cards.MaxYeetEfficiency.Value = float.Parse(value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.AlwaysYeet", out value))
                    Cards.AlwaysYeetCSV.Value = value;

                if (orphaned.TryGetValue("AutoCards", "AutoProtectChonkers", out value))
                    Cards.AutoProtectChonkers.Value = value == "true";

                if (orphaned.TryGetValue("AutoHarvest", "Enabled", out value))
                    Yggdrasil.AutoHarvest.Value = value == "true";

                if (orphaned.TryGetValue("FruitActivationIndicator", "Enabled", out value))
                    Yggdrasil.ActivationIndicator.Value = value == "true";

                orphaned.Clear();
                Config.Save();
            }
        }

        private static bool TryGetValue(this Dictionary<ConfigDefinition, string> dict, string section, string key, out string value)
        {
            var def = new ConfigDefinition(section, key);
            return dict.TryGetValue(def, out value);
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

        internal static class Twitch
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> AutoConnect;
            internal static ConfigEntry<string> ClientId;
            internal static ConfigEntry<string> ClientSecret;
            internal static ConfigEntry<string> AppAccessToken;
            internal static ConfigEntry<string> UserAccessToken;
            internal static ConfigEntry<string> UserRefreshToken;

            internal static class RewardTriggers
            {
                internal static ConfigEntry<string> Merge;
                internal static ConfigEntry<string> Boost;
                internal static ConfigEntry<string> MergeBoost;
                internal static ConfigEntry<string> FightBoss;
                internal static ConfigEntry<string> TossGold;
                internal static ConfigEntry<string> Kitty;
            }
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

        internal static class CustomResolution
        {
            internal static ConfigEntry<int> Width;
            internal static ConfigEntry<int> Height;
        }

        internal static class AutoMergeTransform
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class DiggerUpggradeIndicator
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class Questing
        {
            internal static ConfigEntry<bool> AutoButter;
            internal static ConfigEntry<bool> AlwaysRandom;
        }

        internal static class Cards
        {
            internal static ConfigEntry<bool> AutoSortEnabled;
            internal static ConfigEntry<CardSortBy> AutoSortBy;
            internal static ConfigEntry<CardSortDirection> AutoSortDirection;
            internal static ConfigEntry<bool> AutoYeetEnabled;
            internal static ConfigEntry<rarity> MaxYeetRarity;
            internal static ConfigEntry<float> MaxYeetEfficiency;
            internal static ConfigEntry<string> AlwaysYeetCSV;
            internal static ConfigEntry<bool> AutoProtectChonkers;
        }

        internal static class LSCreminder
        {
            internal static ConfigEntry<int> MaxMinutesToTarget;
        }

        internal static class OverrideCulture
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<string> Locale;
        }

        internal static class GameModes
        {
            internal static ConfigEntry<bool> Hardcore;
            internal static ConfigEntry<bool> PermaTC;
        }

        internal static class Yggdrasil
        {
            internal static ConfigEntry<bool> AutoHarvest;
            internal static ConfigEntry<bool> ActivationIndicator;
            internal static ConfigEntry<float> PoopAudioChance;
        }

        internal static class ResourceNames
        {
            internal static ConfigEntry<ShowFullResourceName> ShowFullName;
        }
    }
}
