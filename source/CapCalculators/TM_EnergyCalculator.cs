using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class TM_EnergyCalculator : BaseCalculator
    {
        TimeMachineController _controller;

        private double p; // total energy power
        private double d; // speed divider
        private double h; // hacks multiplier
        private double m; // TM challenges multiplier
        private double c; // cards multiplier
        private double s; // sadistic divider
        private double modifier;

        internal TM_EnergyCalculator(TimeMachineController controller)
        {
            _controller = controller;
        }

        protected override long GetLevel(long r)
        {
            var result = r / modifier;
            return result >= long.MaxValue ? long.MaxValue : (long)result;
        }

        protected override double GetProgressPerTick(long r, long L)
        {
            return p / d * (r / 50000) * h * m * c / L / s;
        }

        protected override long GetResource(long L)
        {
            var result = L * modifier;
            return result >= long.MaxValue ? long.MaxValue : (long)result;
        }

        protected override void UpdateModifier()
        {
            var character = _controller.character;

            p = character.totalEnergyPower();
            d = _controller.baseSpeedDivider();
            h = character.hacksController.totalTMSpeedBonus();
            m = character.allChallenges.timeMachineChallenge.TMSpeedBonus();
            c = character.cardsController.getBonus(cardBonus.TMSpeed);
            s = (character.settings.rebirthDifficulty >= difficulty.sadistic ? _controller.sadisticDivider() : 1.0);

            modifier = 50000f * d * s / (p * h * m * c);
        }

        internal long LevelFromPPT(double r, double ppt)
        {
            var L = p * r * h * m * c / (50000 * d * s * ppt);
            return L >= long.MaxValue ? long.MaxValue : (long)L;
        }
    }
}

/*
 * p = power
 * d = speed divider
 * r = resource (E/M allocated)
 * h = hack
 * m = TM challenges
 * c = card
 * L = level
 * s = sad divider
 * 
 *  from TimeMachineController.speedProgressPerTick()
 *      ppt = p / d * (r / 50000) * h * m * c / L / s
 *
 *  solve for r and L
 *      r = 50000 * d * L * s * t / (p * h * m * c)
 *      L = p * r * h * m * c / (50000 * d * s * t)
 *       
 *  re-arranging so everything not t, r, and L can be set to a single variable: modifier
 *   (removing t since it's always 1 for these functions)
 *      r = L *  50000 * d * s / (p * h * m * c)
 *      L = r / (50000 * d * s / (p * h * m * c))
 *      
 *   modifier = 50000 * d * s / (p * h * m * c)
 *      r = L * modifier
 *      L = r / modifier
 */
