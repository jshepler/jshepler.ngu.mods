using System.Collections.Generic;

namespace jshepler.ngu.mods.GameData
{
    internal record ZoneData(int id, string name, difficulty minDifficulty, int bossId, int effectiveBossId);

    internal static class Zones
    {
        internal const int MAXZONEID = 45;
        internal static List<int> TitanZoneIds = [6, 8, 11, 14, 16, 19, 23, 26, 30, 34, 38, 42];

        internal static List<ZoneData> Zone;
        internal static ZoneData SafeZone = new(-1, "Safe Zone: Awakening Site", difficulty.normal, 0, 0);

        private static AdventureController _ac;
        private static int[] _effBossIds = [4, 7, 17, 37, 48, 58, 58, 66, 66, 74, 82, 82, 90, 100, 100, 108, 116, 116, 124, 132, 137, 359, 401, 426, 459, 467, 467, 475, 483, 491, 491, 501, 727, 752, 777, 810, 818, 826, 826, 834, 842, 850, 850, 871, 897, 902];

        static Zones()
        {
            _ac = Plugin.Character.adventureController;

            Zone = new();
            for (var x = 0; x < _effBossIds.Length; x++)
                Zone.Add(buildZone(x));
        }

        private static ZoneData buildZone(int id)
        {
            var effBossId = _effBossIds[id];
            var bossId = effBossId switch
            {
                > 602 => effBossId - 602,
                > 301 => effBossId - 301,
                _ => effBossId
            };

            var diff = id switch
            {
                < 21 => difficulty.normal,
                < 32 => difficulty.evil,
                _ => difficulty.sadistic
            };

            return new(id, _ac.zoneName(id), diff, bossId, effBossId);
        }
    }
}
