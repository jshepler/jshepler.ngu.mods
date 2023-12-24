using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class Beard_EnergyCalculator : BaseCalculator, IBeardCalculator
    {
        AllBeardsController _controller;
        int _id;

        private double b; // total energy bars
        private double p; // total energy power
        private double e; // equipment beards bonus
        private double d; // speed divider
        private double c; // beard count divider
        private double g; // digger energy beard bonus
        private double q; // quick beards bonus
        private double u; // UUG hair set bonus

        internal Beard_EnergyCalculator(AllBeardsController controller, int id)
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

            b = character.totalEnergyBar();
            p = Math.Sqrt(character.totalEnergyPower());
            e = 1d + character.inventoryController.bonuses[specType.Beards] + character.inventoryController.bonuses[specType.Beards2];
            d = _controller.speedDivider[_id];
            c = Math.Max(character.beards.energyBeardCount, 1d);
            g = character.allDiggers.totalEnergyBeardBonus();
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
