using System.Collections.Generic;

namespace jshepler.ngu.mods.GameData
{
    internal static class Wandoos
    {
        public static Dictionary<difficulty, Dictionary<OSType, float>> BaseTimes = new()
        {
              { difficulty.normal,   new() {{ OSType.wandoos98, 1E+09f }, { OSType.wandoosMEH, 1E+12f }, { OSType.wandoosXL, 1E+15f }} }
            , { difficulty.evil,     new() {{ OSType.wandoos98, 1E+21f }, { OSType.wandoosMEH, 1E+27f }, { OSType.wandoosXL, 1E+33f }} }
            , { difficulty.sadistic, new() {{ OSType.wandoos98, 1E+21f }, { OSType.wandoosMEH, 1E+27f }, { OSType.wandoosXL, 1E+33f }} }
        };
    }
}
