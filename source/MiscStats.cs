using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class MiscStats
    {
        [HarmonyPrefix, HarmonyPatch(typeof(MiscStatsDisplay), "updateMiscStats")]
        private static bool MiscStatsDisplay_updateMiscStats_prefix(MiscStatsDisplay __instance)
        {
            var character = __instance.character;
            if (character == null || !character.InMenu(Menu.Stats_Misc))
                return true;

            var display = character.display;
            var highestBoss_normal = (int)character.stats.highestBoss;
            var highestBoss_evil = character.highestHardBoss;
            var highestBoss_sad = character.highestSadisticBoss;

            var labels = "<b>Total Rebirths:"
                + "\n\nTotal Bosses Defeated:"
                + "\n\nHighest Boss Defeated:"
                + "\nHighest Evil Boss Defeated:"
                + "\nHighest Sadistic Boss Defeated:"
                + "\n\nTitans Defeated (Adventure Mode):"
                + "\n\nPissed Off Dudes Defeated:"
                + "\n\nPlayer Deaths (norm / evil / sad):"
                + "\n\nTotal Earned EXP:"
                + "\n\nTotal Earned AP:"
                + "\n\nTotal Earned Gold:"
                + "\n\nTotal Gold Tossed:";

            var values = display(character.stats.rebirthNumber)
                + $"\n\n{display(character.stats.bossesDefeated)}"
                + $"\n\n{(highestBoss_normal == 0 ? "----" : character.bossController.getBossName(highestBoss_normal - 1) + $" ({highestBoss_normal})")}"
                + $"\n{(highestBoss_evil == 1 ? "----" : character.bossController.getBossName(highestBoss_evil - 1) + $" ({highestBoss_evil})")}"
                + $"\n{(highestBoss_sad == 1 ? "----" : character.bossController.getBossName(highestBoss_sad - 1) + $" ({highestBoss_sad})")}"
                + $"\n\n{display(character.stats.titansDefeated)}"
                + $"\n\n{display(character.adventure.itopod.enemiesKilled)}"
                + $"\n\n{TrackPlayerDeaths.NormalDeaths} / {TrackPlayerDeaths.EvilDeaths} / {TrackPlayerDeaths.SadDeaths}"
                + $"\n\n{display(character.stats.totalExp)}"
                + $"\n\n{display(character.arbitrary.curLifetimePoints)}"
                + $"\n\n{display(character.stats.totalGold)}"
                + $"\n\n{display(character.pit.totalGold)}";

            var pp = character.adventure.itopod.lifetimePoints;
            if (pp > 0)
            {
                labels += "\n\nTotal Earned PP:";
                values += $"\n\n{display(pp)}";
            }

            __instance.statsBreakdown.text = labels + "</b>";
            __instance.statValue.text = values;
            return false;
        }
    }
}
