using System.Collections.Generic;

namespace jshepler.ngu.mods.GameData
{
    internal record AK(int effectiveBossId, int enemyId, int version, string name, PTR ptr, int optionalKills = 0);

    internal class TitanAK
    {
        internal static List<AK> Requirements =
        [
            new AK(58, 302, 1, "GRB", new PTR(3000f, 2500f)),
            new AK(66, 303, 1, "GCT", new PTR(9000f, 7000f)),
            new AK(82, 304, 1, "JAKE", new PTR(25000f, 15000f)),
            new AK(100, 305, 1, "UUG", new PTR(800000f, 400000f, 14000f)),
            new AK(116, 310, 1, "WALDERP", new PTR(1.3e+7f, 7.0e+6f, 150000f)),
            new AK(132, 312, 1, "BEAST v1", new PTR(2.5E+09f, 1.6E+09f, 2.5E+07f)),
            new AK(132, 313, 2, "BEAST v2", new PTR(2.5E+10f, 1.6E+10f, 2.5E+08f)),
            new AK(132, 314, 3, "BEAST v3", new PTR(2.5E+11f, 1.6E+11f, 2.5E+09f)),
            new AK(132, 315, 4, "BEAST v4", new PTR(2.5E+12f, 1.6E+12f, 2.5E+10f)),
            new AK(426, 334, 1, "NERD v1", new PTR(5E+14f, 2.5E+14f, 5E+12f)),
            new AK(426, 335, 2, "NERD v2", new PTR(1E+16f, 5E+15f, 1E+14f)),
            new AK(426, 336, 3, "NERD v3", new PTR(2E+17f, 1E+17f, 2E+15f)),
            new AK(426, 337, 4, "NERD v4", new PTR(5E+18f, 2.5E+18f, 5E+16f)),
            new AK(467, 339, 1, "GM v1", new PTR(5E+18f, 2.5E+18f, 5E+16f)),
            new AK(467, 340, 2, "GM v2", new PTR(1E+20f, 5E+19f, 1E+18f)),
            new AK(467, 341, 3, "GM v3", new PTR(2E+21f, 1E+21f, 2E+19f)),
            new AK(467, 342, 4, "GM v4", new PTR(5E+22f, 2.5E+22f, 5E+20f)),
            new AK(491, 344, 1, "EXILE v1", new PTR(1E+23f, 5E+22f, 1E+21f), 24),
            new AK(491, 345, 2, "EXILE v2", new PTR(2E+24f, 1E+24f, 2E+22f), 24),
            new AK(491, 346, 3, "EXILE v3", new PTR(4E+25f, 2E+25f, 4E+23f), 24),
            new AK(491, 347, 4, "EXILE v4", new PTR(7.5E+26f, 3.7E+26f, 7.5E+24f), 24),
            new AK(727, 365, 1, "IH v1", new PTR(4E+28f, 2E+28f, 4E+26f), 5),
            new AK(727, 366, 2, "IH v2", new PTR(3.2E+29f, 1.6E+29f, 1.6E+27f), 5),
            new AK(727, 367, 3, "IH v3", new PTR(2E+30f, 1E+30f, 1E+28f), 5),
            new AK(727, 368, 4, "IH v4", new PTR(1E+31f, 5E+30f, 5E+28f), 5),
            new AK(826, 369, 1, "RL v1", new PTR(1.8E+31f, 6E+30f, 1.2E+29f), 5),
            new AK(826, 370, 2, "RL v2", new PTR(9E+31f, 3E+31f, 6E+29f), 5),
            new AK(826, 371, 3, "RL v3", new PTR(3.6E+32f, 1.2E+32f, 2.5E+30f), 5),
            new AK(826, 372, 4, "RL v4", new PTR(1.1E+33f, 3.6E+32f, 7.5E+30f), 5),
            new AK(848, 373, 1, "AMAL v1", new PTR(3E+33f, 1E+33f, 2E+31f), 5),
            new AK(848, 374, 2, "AMAL v2", new PTR(1.2E+34f, 4E+33f, 8E+31f), 5),
            new AK(848, 375, 3, "AMAL v3", new PTR(3.6E+34f, 1.2E+34f, 2.4E+32f), 5),
            new AK(848, 376, 4, "AMAL v4", new PTR(7.2E+34f, 2.4E+34f, 4.8E+32f), 5)
        ];
    }

    internal class PTR
    {
        public float Power;
        public float Toughness;
        public float Regen;

        public bool IsNothing => (Power == 0f) && (Toughness == 0f) && (Regen == 0f);

        public PTR(long power, long toughness, long regen = 0)
        {
            this.Power = power;
            this.Toughness = toughness;
            this.Regen = regen;
        }

        public PTR(float power, float toughness, float regen = 0f)
        {
            this.Power = power;
            this.Toughness = toughness;
            this.Regen = regen;
        }

        public static bool operator >(PTR a, PTR b)
        {
            var aDefense = a.Toughness > a.Regen ? a.Toughness : a.Regen;
            var bDefense = b.Toughness > b.Regen ? b.Toughness : b.Regen;

            return (a.Power > b.Power) || (aDefense > bDefense);
        }

        public static bool operator <(PTR a, PTR b)
        {
            return !(a > b);
        }

        public static bool operator ==(PTR a, PTR b)
        {
            if ((object)a == null)
                return (object)b == null;

            return a.Equals(b);
        }

        public static bool operator !=(PTR a, PTR b)
        {
            return !(a == b);
        }

        public static bool operator <=(PTR a, PTR b)
        {
            return (a < b) || (a == b);
        }

        public static bool operator >=(PTR a, PTR b)
        {
            return (a > b) || (a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (PTR)obj;
            return (Power == other.Power) && (Toughness == other.Toughness) && (Regen == other.Regen);
        }

        public override int GetHashCode()
        {
            return (Power, Toughness, Regen).GetHashCode();
        }

        public override string ToString()
        {
            return $"{{p:{Power}, t:{Toughness}, r:{Regen}}}";
        }
    }
}
