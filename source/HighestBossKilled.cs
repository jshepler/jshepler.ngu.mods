using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HighestBossKilled
    {
        [HarmonyPostfix, HarmonyPatch(typeof(MiscStatsDisplay), "updateMiscStats")]
        private static void MiscStatsDisplay_updateMiscStats_postfix(MiscStatsDisplay __instance, ref string ___statsName, ref string ___statsValue)
        {
            var character = Plugin.Character;
            if (character == null)
                return;

            var i = ___statsValue.IndexOf("(");
            if (i > -1)
                ___statsValue = ___statsValue.Insert(i, " ");

            var nameIndex = ___statsName.IndexOf("Highest Boss Defeated:") + 22;
            var valueIndex = ___statsValue.IndexOf(")") + 1;

            var nameString = string.Empty;
            var valueString = string.Empty;

            if (character.highestHardBoss > 1)
            {
                nameString = "\n\nHighest Evil Boss Defeated:";
                valueString = $"\n\n{character.bossController.getBossName(character.highestHardBoss - 1)} ({character.highestHardBoss})";
            }

            if (character.highestSadisticBoss > 1)
            {
                nameString += "\n\nHighest Sadistic Boss Defeated:";
                valueString += $"\n\n{character.bossController.getBossName(character.highestSadisticBoss - 1)} ({character.highestSadisticBoss})";
            }

            ___statsName = ___statsName.Replace("Total Earned PP:", "<b>Total Earned PP:</b>");

            if (valueIndex > 0)
            {
                ___statsName = ___statsName.Insert(nameIndex, nameString);
                __instance.statsBreakdown.text = ___statsName;

                ___statsValue = ___statsValue.Insert(valueIndex, valueString);
                __instance.statValue.text = ___statsValue;
            }
        }
    }
}
