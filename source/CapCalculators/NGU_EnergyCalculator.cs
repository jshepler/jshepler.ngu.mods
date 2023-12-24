﻿using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class NGU_EnergyCalculator : BaseCalculator
    {
        private NGUController _controller;

        private double p; // energy power
        private double d; // ngu speed divider
        private double e; // equipment multiplier
        private double k; // perks
        private double m; // macguffin
        private double n; // ngu energy ngu
        private double g; // diggers
        private double h; // hacks
        private double q; // quirks
        private double w; // wishes
        private double c; // cards
        private double b; // sad tc1
        private double s; // sad divider
        private double modifier;

        internal NGU_EnergyCalculator(NGUController controller)
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
            var ppt = p / d * r / L * e * k * m * n * g * h * q * w * c * b / s;
            return ppt;
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
            d = character.NGUController.energySpeedDivider(_controller.id);
            e = character.totalNGUSpeedBonus();
            k = character.adventureController.itopod.totalEnergyNGUBonus();
            m = character.inventory.macguffinBonuses[4];
            n = character.NGUController.energyNGUBonus();
            g = character.allDiggers.totalEnergyNGUBonus();
            h = character.hacksController.totalEnergyNGUBonus();
            q = character.beastQuestPerkController.totalEnergyNGUSpeed();
            w = character.wishesController.totalEnergyNGUSpeed();
            c = character.cardsController.getBonus(cardBonus.energyNGUSpeed);
            b = character.allChallenges.trollChallenge.sadisticCompletions() >= 1 ? 3.0d : 1.0d;
            s = character.settings.nguLevelTrack >= difficulty.sadistic ? _controller.sadisticDivider() : 1.0;

            modifier = d * s / (p * e * k * m * n * g * h * q * w * c * b);
        }
    }
}

/*
 * p = power
 * d = speed divider
 * r = resource (E/M allocated)
 * L = level
 * e = equipment bonus
 * k = perks
 * m = macguffin
 * n = ngu
 * g = digger
 * h = hack
 * q = quirks
 * w = wishes
 * c = cards
 * b = sad tc1
 * s = sad divider
 * 
 *  from NGUController.progressPerTick()
 *       t = p / d * r / L * e * k * m * n * g * h * q * w * c * b / s
 *  
 *  solve for r and L
 *       r = d * L * s * t / (p * e * k * m * n * g * h * q * w * c * b)
 *       L = p * r * e * k * m * n * g * h * q * w * c * b / (d * s * t)
 *       
 *  re-arranging so everything not t, r, and L can be set to a single variable: modifier
 *   (removing t since it's always 1 for these functions)
 *      r = L *  d * s / (p * e * k * m * n * g * h * q * w * c * b)
 *      L = r / (d * s / (p * e * k * m * n * g * h * q * w * c * b))
 *      
 *   modifier = d * s / (p * e * k * m * n * g * h * q * w * c * b)
 *      r = L * modifier
 *      L = r / modifier
 */
