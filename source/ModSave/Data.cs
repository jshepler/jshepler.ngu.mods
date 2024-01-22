using System.Collections.Generic;

namespace jshepler.ngu.mods.ModSave
{
    internal static class Data
    {
        internal static Dictionary<string, object> Values = new();

        internal static T Get<T>(string key, T defaultValue = default)
        {
            if (!Values.ContainsKey(key))
                Values[key] = defaultValue;

            return (T)Values[key];
        }

        internal static void Set(string key, object value)
        {
            if (!Values.ContainsKey(key))
                Values.Add(key, value);
            else
                Values[key] = value;
        }

        internal static bool AutoQuestingEnabled
        {
            get => Get<bool>("AutoQuestingEnabled");
            set => Set("AutoQuestingEnabled", value);
        }

        internal static string[] LastYggRewards
        {
            get => Get<string[]>("LastYggRewards", new string[21]);
            set => Set("LastYggRewards", value);
        }

        internal static Loadout LastLoadout
        {
            get => Get<Loadout>("LastLoadout");
            set => Set("LastLoadout", value);
        }

        internal static float BM_IronPill_LastGained
        {
            get => Get<float>("BM_IronPill_LastGained");
            set => Set("BM_IronPill_LastGained", value);
        }

        internal static List<int> WishQueue
        {
            get => Get<List<int>>("WishQueue",new());
            set => Set("WishQueue", value);
        }

        internal static Dictionary<int, int[]> EnabledEnergyIDs
        {
            get => Get<Dictionary<int, int[]>>("EnabledEnergyIDs", new());
            set => Set("EnabledEnergyIDs", value);
        }

        internal static Dictionary<int, int[]> EnabledMagicIDs
        {
            get => Get<Dictionary<int, int[]>>("EnabledMagicIDs", new());
            set => Set("EnabledMagicIDs", value);
        }

        internal static Dictionary<int, int[]> EnabledRes3IDs
        {
            get => Get<Dictionary<int, int[]>>("EnabledRes3IDs", new());
            set => Set("EnabledRes3IDs", value);
        }

        internal static double TotalTimePlayedNormal
        {
            get => Get<double>("TotalTimePlayedNormal");
            set => Set("TotalTimePlayedNormal", value);
        }

        internal static double TotalTimePlayedEvil
        {
            get => Get<double>("TotalTimePlayedEvil");
            set => Set("TotalTimePlayedEvil", value);
        }

        internal static double TotalTimePlayedSadistic
        {
            get => Get<double>("TotalTimePlayedSadistic");
            set => Set("TotalTimePlayedSadistic", value);
        }

        internal static int AutoMayGenMode
        {
            get => Get("AutoMayGenMode", 0);
            set => Set("AutoMayGenMode", value);
        }

        // REMINDER: NO TYPES DEFINED IN MODS ELSE NOT LOADABLE IN VANILLA
    }
}
