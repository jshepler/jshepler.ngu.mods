using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class HacksCalculator : BaseCalculator
    {
        private HacksController _controller;
        private int _id;

        private double p;
        private double b;
        private double d;

        internal HacksCalculator(HacksController controller, int id)
        {
            _controller = controller;
            _id = id;
        }

        protected override long GetLevel(long r)
        {
            throw new NotImplementedException();
        }

        protected override double GetProgressPerTick(long r, long L)
        {
            var levelDivider = Math.Pow(1.0078, L - 1) * L;
            var ppt = r * p * b / (d * levelDivider);

            if (ppt > float.MaxValue)
                return float.MaxValue;

            if (ppt < 0)
                return 0.0;

            return ppt;
        }

        internal double PPT(long r, long L)
        {
            return GetProgressPerTick(r, L);
        }

        protected override double GetResource(long L)
        {
            return double.MaxValue;
        }

        protected override void UpdateModifier()
        {
            p = _controller.character.totalRes3Power();
            b = _controller.totalHackSpeedBonus();
            d = _controller.properties[_id].baseDivider;
        }
    }
}
