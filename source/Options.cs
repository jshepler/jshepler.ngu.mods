using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            Options.AutoHarvest.Enabled = Config.Bind("AutoHarvest", "Enabled", false, "enable auto harvest/eat fruits when fully grown (max tier); or when rebirthing and growth >= tier 1");
            Options.AutoSnipe.SnipeDroop.Enabled = Config.Bind("AutoSnipe.SnipeDroop", "Enabled", false, "when sniping Forest, snipe Droop instead of Bosses");

            Options.RemoteTriggers.Enabled = Config.Bind("RemoteTriggers", "Enabled", false, "enables receiving of remote commands");
            Options.RemoteTriggers.UrlPrefix = Config.Bind("RemoteTriggers", "Prefix", "http://localhost:8088/ngu/", "urls must start with this prefix else will be ignored");
            Options.RemoteTriggers.AutoBoost.Enabled = Config.Bind("RemoteTriggers.AutoBoost", "Enabled", true, "enables auto-boost trigger");
            Options.RemoteTriggers.AutoMerge.Enabled = Config.Bind("RemoteTriggers.AutoMerge", "Enabled", true, "enables auto-merge trigger");
            Options.RemoteTriggers.TossGold.Enabled = Config.Bind("RemoteTriggers.TossGold", "Enabled", true, "enables toss gold trigger");
            Options.RemoteTriggers.FightBoss.Enabled = Config.Bind("RemoteTriggers.FightBoss", "Enabled", true, "enables fight boss trigger");
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
        }

        internal static class AutoHarvest
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class AutoSnipe
        {
            internal static class SnipeDroop
            {
                internal static ConfigEntry<bool> Enabled;
            }
        }
    }
}
