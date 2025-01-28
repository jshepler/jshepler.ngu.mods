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
            get => Get("LastYggRewards", new string[21]);
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

        internal static List<int> WishList
        {
            get => Get<List<int>>("WishList", new());
            set => Set("WishList", value);
        }

        internal static List<int> WishTargets
        {
            get => Get<List<int>>("WishTargets", new());
            set => Set("WishTargets", value);
        }

        internal static List<int> WishesLastRunning
        {
            get => Get<List<int>>("WishesLastRunning", new());
            set => Set("WishesLastRunning", value);
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

        internal static double TotalTimeOffline
        {
            get => Get("TotalTimeOffline", 0.0);
            set => Set("TotalTimeOffline", value);
        }

        internal static double TotalTimeOfflineNormal
        {
            get => Get("TotalTimeOfflineNormal", 0.0);
            set => Set("TotalTimeOfflineNormal", value);
        }

        internal static double TotalTimeOfflineEvil
        {
            get => Get("TotalTimeOfflineEvil", 0.0);
            set => Set("TotalTimeOfflineEvil", value);
        }

        internal static double TotalTimeOfflineSadistic
        {
            get => Get("TotalTimeOfflineSadistic", 0.0);
            set => Set("TotalTimeOfflineSadistic", value);
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

        internal static int CurrentDiggerLoadoutId
        {
            get => Get("CurrentDiggerLoadoutId", 0);
            set => Set("CurrentDiggerLoadoutId", value);
        }

        internal static float CubePowerBoostThisRB
        {
            get => Get("CubePowerBoostThisRB", 0f);
            set => Set("CubePowerBoostThisRB", value);
        }

        internal static float CubePowerBoostLastRB
        {
            get => Get("CubePowerBoostLastRB", 0f);
            set => Set("CubePowerBoostLastRB", value);
        }

        internal static float CubeToughnessBoostThisRB
        {
            get => Get("CubeToughnessBoostThisRB", 0f);
            set => Set("CubeToughnessBoostThisRB", value);
        }

        internal static float CubeToughnessBoostLastRB
        {
            get => Get("CubeToughnessBoostLastRB", 0f);
            set => Set("CubeToughnessBoostLastRB", value);
        }

        internal static Dictionary<int, List<int>> DiggerLoadouts
        {
            get => Get<Dictionary<int, List<int>>>("DiggerLoadouts", new() { { 0, new() }, { 1, new() }, { 2, new() } });
            set => Set("DiggerLoadouts", value);
        }

        internal static Dictionary<int, List<long>> DiggerLevels
        {
            get => Get<Dictionary<int, List<long>>>("DiggerLevels", new() { { 0, new() }, { 1, new() }, { 2, new() } });
            set => Set("DiggerLevels", value);
        }

        internal static double LastRebirthTime
        {
            get => Get("LastRebirthTime", 0.0);
            set => Set("LastRebirthTime", value);
        }

        internal static long[] ExpSourcesThisRB
        {
            get => Get<long[]>("ExpSourcesThisRB", [0L, 0L, 0L, 0L]);
            set => Set("ExpSourcesThisRB", value);
        }

        internal static long[] ExpSourcesLastRB
        {
            get => Get<long[]>("ExpSourcesLastRB", [0L, 0L, 0L, 0L]);
            set => Set("ExpSourcesLastRB", value);
        }

        internal static long[] PPSourcesThisRB
        {
            get => Get<long[]>("PPSourcesThisRB", [0L, 0L, 0L]);
            set => Set("PPSourcesThisRB", value);
        }

        internal static long[] PPSourcesLastRB
        {
            get => Get<long[]>("PPSourcesLastRB", [0L, 0L, 0L]);
            set => Set("PPSourcesLastRB", value);
        }

        internal static long[] QPSourcesThisRB
        {
            get => Get("QPSourcesThisRB", new long[TrackQPGained.SOURCE_COUNT]);
            set => Set("QPSourcesThisRB", value);
        }

        internal static long[] QPSourcesLastRB
        {
            get => Get("QPSourcesLastRB", new long[TrackQPGained.SOURCE_COUNT]);
            set => Set("QPSourcesLastRB", value);
        }

        internal static float BaseAdvPowerGained
        {
            get => Get("BaseAdvPowerGained", 0f);
            set => Set("BaseAdvPowerGained", value);
        }

        internal static float[] BaseAdvPowerGainSources
        {
            get => Get("BaseAdvPowerGainSources", new float[TrackBaseAdvPowerGained.Sources.COUNT]);
            set => Set("BaseAdvPowerGainSources", value);
        }

        internal static bool Hardcore
        {
            get => Get("HardCore", false);
            set => Set("HardCore", value);
        }

        internal static bool PermaTC
        {
            get => Get("PermaTC", false);
            set => Set("PermaTC", value);
        }

        internal static float PTC_Timer
        {
            get => Get("PTC_Timer", 0f);
            set => Set("PTC_Timer", value);
        }

        internal static int PTC_TrollCount
        {
            get => Get("PTC_TrollCount", 0);
            set => Set("PTC_TrollCount", value);
        }

        internal static int[][] PCBRatios
        {
            get => Get<int[][]>("PCBRatios", [[1, 37500, 1], [1, 37500, 1], [1, 37500, 1]]);
            set => Set("PCBRatios", value);
        }

        internal static int Deaths_Normal
        {
            get => Get("Deaths_Normal", 0);
            set => Set("Deaths_Normal", value);
        }

        internal static int Deaths_Evil
        {
            get => Get("Deaths_Evil", 0);
            set => Set("Deaths_Evil", value);
        }

        internal static int Deaths_Sadistic
        {
            get => Get("Deaths_Sadistic", 0);
            set => Set("Deaths_Sadistic", value);
        }

        // REMINDER: NO TYPES DEFINED IN MODS ELSE NOT LOADABLE IN VANILLA
    }
}
