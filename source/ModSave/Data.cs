using System;
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

        internal static long ExpGainedLastRB
        {
            get => Get("ExpGainedLastRB", 0L);
            set => Set("ExpGainedLastRB", value);
        }

        internal static long ExpGainedThisRB
        {
            get => Get("ExpGainedThisRB", 0L);
            set => Set("ExpGainedThisRB", value);
        }

        internal static long SeedsGainedLastRB
        {
            get => Get("SeedsGainedLastRB", 0L);
            set => Set("SeedsGainedLastRB", value);
        }

        internal static long SeedsGainedThisRB
        {
            get => Get("SeedsGainedThisRB", 0L);
            set => Set("SeedsGainedThisRB", value);
        }

        internal static long PoopGainedLastRB
        {
            get => Get("PoopGainedLastRB", 0L);
            set => Set("PoopGainedLastRB", value);
        }

        internal static long PoopGainedThisRB
        {
            get => Get("PoopGainedThisRB", 0L);
            set => Set("PoopGainedThisRB", value);
        }

        internal static long APGainedLastRB
        {
            get => Get("APGainedLastRB", 0L);
            set => Set("APGainedLastRB", value);
        }

        internal static long APGainedThisRB
        {
            get => Get("APGainedThisRB", 0L);
            set => Set("APGainedThisRB", value);
        }

        internal static long QPGainedLastRB
        {
            get => Get("QPGainedLastRB", 0L);
            set => Set("QPGainedLastRB", value);
        }

        internal static long QPGainedThisRB
        {
            get => Get("QPGainedThisRB", 0L);
            set => Set("QPGainedThisRB", value);
        }

        internal static long PPGainedLastRB
        {
            get => Get("PPGainedLastRB", 0L);
            set => Set("PPGainedLastRB", value);
        }

        internal static long PPGainedThisRB
        {
            get => Get("PPGainedThisRB", 0L);
            set => Set("PPGainedThisRB", value);
        }

        // REMINDER: NO TYPES DEFINED IN MODS ELSE NOT LOADABLE IN VANILLA
    }
}
