using HarmonyLib;
using jshepler.ngu.mods.ModSave;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackPlayerDeaths
    {
        internal static int NormalDeaths
        {
            get => Data.Deaths_Normal;
            set => Data.Deaths_Normal = value;
        }

        internal static int EvilDeaths
        {
            get => Data.Deaths_Evil;
            set => Data.Deaths_Evil = value;
        }

        internal static int SadDeaths
        {
            get => Data.Deaths_Sadistic;
            set => Data.Deaths_Sadistic = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "playerDeath")]
        private static void AdventureController_playerDeath_postfix()
        {
            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    NormalDeaths++;
                    break;

                case difficulty.evil:
                    EvilDeaths++;
                    break;

                case difficulty.sadistic:
                    SadDeaths++;
                    break;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "fight")]
        private static void BossController_fight_postfix()
        {
            if (Plugin.Character.curHP > 0)
                return;

            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    NormalDeaths++;
                    break;

                case difficulty.evil:
                    EvilDeaths++;
                    break;

                case difficulty.sadistic:
                    SadDeaths++;
                    break;
            }
        }
    }
}
