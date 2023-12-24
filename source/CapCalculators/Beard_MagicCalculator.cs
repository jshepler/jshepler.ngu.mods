using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class Beard_MagicCalculator : BaseCalculator, IBeardCalculator
    {
        AllBeardsController _controller;
        int _id;

        private double b; // total magic bars
        private double p; // total magic power
        private double e; // equipment beards bonus
        private double d; // speed divider
        private double c; // beard count divider
        private double g; // digger magic beard bonus
        private double q; // quick beards bonus
        private double u; // UUG hair set bonus

        internal Beard_MagicCalculator(AllBeardsController controller, int id)
        {
            _controller = controller;
            _id = id;
        }

        protected override long GetLevel(long r)
        {
            var L = (b * p * e * g * q * u / (d * c));
            return L >= long.MaxValue ? long.MaxValue : (long)L;
        }

        protected override double GetProgressPerTick(long r, long L)
        {
            throw new NotImplementedException();
        }

        protected override long GetResource(long L)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateModifier()
        {
            var character = _controller.character;

            b = character.totalMagicBar();
            p = Math.Sqrt(character.totalMagicPower());
            e = 1d + character.inventoryController.bonuses[specType.Beards] + character.inventoryController.bonuses[specType.Beards2];
            d = _controller.speedDivider[_id];
            c = Math.Max(character.beards.magicBeardCount, 1d);
            g = character.allDiggers.totalMagicBeardBonus();
            q = character.beastQuestPerkController.totalBeardSpeedBonus();
            u = character.inventory.itemList.uugComplete ? 1.1 : 1.0;

            if (c >= 2 && character.inventory.itemList.beardverseComplete)
                c *= 0.9d;
        }

        long IBeardCalculator.LevelFromResource(long resource)
        {
            return base.LevelFromResource(resource);
        }
    }
}
