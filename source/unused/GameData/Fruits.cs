using System.Collections.Generic;

namespace jshepler.ngu.mods.GameData
{
    internal enum FruitId
    {
        FoG,
        FoPa,
        FoA,
        FoK,
        POM,
        FoL,
        FoPb,
        FoAP,
        FoN,
        FoR,
        FoMa,
        FoPd,
        Melon,
        FoMb,
        FoQ,
        Mayo1,
        Mayo2,
        Mayo3,
        Mayo4,
        Mayo5,
        Mayo6
    }

    internal static class Fruits
    {
        internal static Dictionary<FruitId, (bool NGU, bool YIELD, bool FH)> ModifedBy = new()
        {
            { FruitId.FoG, (false, false, true) },
            { FruitId.FoPa, (true, true, true) },
            { FruitId.FoA, (true, true, true) },
            { FruitId.FoK, (true, true, true) },
            { FruitId.POM, (true, false, true) },
            { FruitId.FoL, (true, true, true) },
            { FruitId.FoPb, (true, true, true) },
            { FruitId.FoAP, (false, false, true) },
            { FruitId.FoN, (true, true, true) },
            { FruitId.FoR, (false, true, true) },
            { FruitId.FoMa, (false, true, true) },
            { FruitId.FoPd, (true, true, true) },
            { FruitId.Melon, (true, false, true) },
            { FruitId.FoMb, (false, true, true) },
            { FruitId.FoQ, (false, true, true) },
            { FruitId.Mayo1, (false, false, false) },
            { FruitId.Mayo2, (false, false, false) },
            { FruitId.Mayo3, (false, false, false) },
            { FruitId.Mayo4, (false, false, false) },
            { FruitId.Mayo5, (false, false, false) },
            { FruitId.Mayo6, (false, false, false) }
        };
    }
}
