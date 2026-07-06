using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class NakedEMR3
    {
        internal static string BuildNakedEMR3()
        {
            var root = new JSONObject();
            root.Add("Nude Energy Cap", nakedEcap());
            root.Add("Nude Magic Cap", nakedMcap());
            root.Add("Nude Energy Power", nakedEpow());
            root.Add("Nude Magic Power", nakedMpow());
            root.Add("Nude Energy Bars", nakedEbars());
            root.Add("Nude Magic Bars", nakedMbars());
            root.Add("Nude Resource 3 Power", nakedR3pow());
            root.Add("Nude Resource 3 Cap", nakedR3cap());
            root.Add("Nude Resource 3 Bars", nakedR3bars());
            root.Add("modifiers", true);

            return root.ToString();
        }

        private static long nakedEpow()
        {
            var c = Plugin.Character;

            var d = (double)c.energyPower
                * c.adventureController.itopod.totalEnergyPowerBonus()
                * c.inventory.macguffinBonuses[0]
                * c.beastQuestPerkController.totalEnergyPowerBonus()
                * c.wishesController.totalEnergyPowerBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedEcap()
        {
            var c = Plugin.Character;

            var d = (double)c.capEnergy
                * c.adventureController.itopod.totalEnergyCapBonus()
                * c.inventory.macguffinBonuses[1]
                * c.beastQuestPerkController.totalEnergyCapBonus()
                * c.wishesController.totalEnergyCapBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedEbars()
        {
            var c = Plugin.Character;

            var d = (double)c.energyBars
                * c.adventureController.itopod.totalEnergyBarBonus()
                * c.inventory.macguffinBonuses[6]
                * c.beastQuestPerkController.totalEnergyBarBonus()
                * c.wishesController.totalEnergyBarBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedMpow()
        {
            var c = Plugin.Character;

            var d = c.magic.magicPower
                * c.adventureController.itopod.totalMagicPowerBonus()
                * c.inventory.macguffinBonuses[2]
                * c.beastQuestPerkController.totalMagicPowerBonus()
                * c.wishesController.totalMagicPowerBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedMcap()
        {
            var c = Plugin.Character;

            var d = (double)c.magic.capMagic
                * c.adventureController.itopod.totalMagicCapBonus()
                * c.inventory.macguffinBonuses[3]
                * c.beastQuestPerkController.totalMagicCapBonus()
                * c.wishesController.totalMagicCapBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedMbars()
        {
            var c = Plugin.Character;

            var d = (double)c.magic.magicPerBar
                * c.adventureController.itopod.totalMagicBarBonus()
                * c.inventory.macguffinBonuses[7]
                * c.beastQuestPerkController.totalMagicBarBonus()
                * c.wishesController.totalMagicBarBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedR3pow()
        {
            var c = Plugin.Character;

            var d = (double)c.res3.res3Power
                * c.adventureController.itopod.totalRes3PowerBonus()
                * c.inventory.macguffinBonuses[20]
                * c.beastQuestPerkController.totalRes3PowerBonus()
                * c.wishesController.totalRes3PowerBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedR3cap()
        {
            var c = Plugin.Character;

            var d = (double)c.res3.capRes3
                * c.adventureController.itopod.totalRes3CapBonus()
                * c.inventory.macguffinBonuses[21]
                * c.beastQuestPerkController.totalRes3CapBonus()
                * c.wishesController.totalRes3CapBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }

        private static long nakedR3bars()
        {
            var c = Plugin.Character;

            var d = (double)c.res3.res3PerBar
                * c.adventureController.itopod.totalRes3BarBonus()
                * c.inventory.macguffinBonuses[22]
                * c.beastQuestPerkController.totalRes3BarBonus()
                * c.wishesController.totalRes3BarBonus();

            return d > long.MaxValue ? long.MaxValue : (long)d;
        }
    }
}
