using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class AdvancedTrainingCalculator : BaseCalculator
    {
        private AllAdvancedTraining _controller;
        private int _id;

        private double p; // power
        private double b; // baseTime
        private double e; // equipment multiplier

        internal AdvancedTrainingCalculator(AllAdvancedTraining controller, int id)
        {
            _controller = controller;
            _id = id;
        }

        protected override long GetLevel(long r)
        {
            var L = r * Math.Sqrt(p) * e / (50 * b);
            return L >= long.MaxValue ? long.MaxValue : (long)L;
        }

        protected override double GetProgressPerTick(long r, long L)
        {
            return r / 50 * Math.Sqrt(p) * e / (b * L);
        }

        protected override long GetResource(long L)
        {
            var r = 50 * b * L * Math.Sqrt(p) / (e * p);
            return r >= long.MaxValue ? long.MaxValue : (long)r;
        }

        protected override void UpdateModifier()
        {
            p = (double)Plugin.Character.totalEnergyPower();
            b = (double)Plugin.Character.advancedTrainingController.AdvancedTrainingController(_id).baseTime;
            e = (double)Plugin.Character.totalAdvancedTrainingSpeedBonus();
        }
    }
}

/*
 * ppt = r / 50 * sqrt(p) * e / (b * L)
 * 
 * 1 = r / 50 * sqrt(p) * e / (b * L)
 * L = r * sqrt(p) * e / (50 * b)
 * r = 50 * b * L * sqrt(p) / (e * p)
 * 
 */