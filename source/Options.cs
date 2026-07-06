using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            Questing.AlwaysRandom = Config.Bind("Questing", "AlwaysRandom", false, "If true, new quests will always be random instead of targeting current zone");
        }

        internal static class Questing
        {
            internal static ConfigEntry<bool> AlwaysRandom;
        }
    }
}
