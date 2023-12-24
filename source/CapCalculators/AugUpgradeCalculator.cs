using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal class AugUpgradeCalculator : BaseCalculator
    {
        private AllAugsController _controller;
        private int _id;

        private double p; // power
        private double d; // speed divider
        private double e; // equip bonus
        private double m; // macguffin
        private double h; // hack
        private double k; // perk
        private double c; // card
        private double a; // evil NOAUG challenges
        private double n; // normal NOAUG challenges
        private double s; // sad divider
        private double modifier;

        internal AugUpgradeCalculator(AllAugsController controller, int id)
        {
            _controller = controller;
            _id = id;
        }

        protected override long GetLevel(long r)
        {
            var L = r / modifier;
            return L >= long.MaxValue ? long.MaxValue : (long)L;
        }

        protected override double GetProgressPerTick(long r, long L)
        {
            var ppt = p / d / 50000 * r / L * e * m * h * k * c * a * n / s;
            return ppt;
        }

        protected override long GetResource(long L)
        {
            var r = L * modifier;
            return r >= long.MaxValue ? long.MaxValue : (long)r;
        }

        protected override void UpdateModifier()
        {
            var character = _controller.character;

            p = character.totalEnergyPower();
            d = augUpgradeDivider(_id);
            e = 1.0 + character.inventoryController.bonuses[specType.Augs];
            m = character.inventory.macguffinBonuses[12];
            h = character.hacksController.totalAugSpeedBonus();
            k = character.adventureController.itopod.totalAugSpeedBonus();
            c = character.cardsController.getBonus(cardBonus.augSpeed);
            s = (character.settings.rebirthDifficulty >= difficulty.sadistic) ? 5E+07 : 1.0;

            n = (character.allChallenges.noAugsChallenge.evilCompletions() >= character.allChallenges.noAugsChallenge.maxCompletions)
                ? 1.25
                : 1;

            a = 1.0 + character.allChallenges.noAugsChallenge.evilCompletions() * 0.05;
            if (character.allChallenges.noAugsChallenge.completions() >= 1)
            {
                a *= 1.1000000238418579;
            }

            modifier = 50000.0 * d * s / (p * e * m * h * k * c * a * n);
        }

        private float augUpgradeDivider(int id) => _controller.character.settings.rebirthDifficulty switch
        {
            difficulty.normal => _controller.normalUpgradeSpeedDividers[id],
            difficulty.evil => _controller.evilUpgradeSpeedDividers[id],
            difficulty.sadistic => _controller.sadisticUpgradeSpeedDividers[id],
            _ => 0
        };
    }
}

/*
 * ppt = p / d / 50000 * r / L * e * m * h * k * c * a * n / s
 * 
 * 1 = p / d / 50000 * r / L * e * m * h * k * c * a * n / s
 * r = 50000 * d * L * s / (p * e * m * h * k * c * a * n)
 * L = p * r * e * m * h * c * a * n / (50000 * d * s)
 * 
 * r = L *  d * s / (p * e * m * h * k * c * a * n)
 * L = r / (d * s / (p * e * m * h * k * c * a * n))
 * 
 * mod = d * s / (p * e * m * h * k * c * a * n)
 * r = L * mod
 * L = r / mod
 * 
 * 
 * p = power
 * d = speed divider
 * r = resource (E/M allocated)
 * L = level
 * e = equipment bonus
 * m = macguffin
 * h = hack
 * k = perk
 * c = card
 * a = evil NOAUG challenge
 * n = normal NOAUG challenge
 * s = sad divider
 * 
 */
