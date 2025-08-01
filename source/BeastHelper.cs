using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BeastHelper
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "displayEnemyStats")]
        private static void AdventureController_updateAdventureStats_postfix(AdventureController __instance)
        {
            if (Hardcore.IsHardcoreGame)
                return;

            var ai = __instance.enemyAI;
            if (!__instance.character.InMenu(Menu.Adventure)
                || __instance.currentEnemy == null
                || !ai.isBeast())
                return;

            var aura = ai.auraID switch
            {
                1 => "HYPER RED ANIME",
                2 => "POWER SMASH",
                3 => "METAL ARMOR",
                4 => "SLOW TIME",
                5 => "RUBBERY SKIN",
                6 => "RANCID FART",
                _ => "none"
            };

            __instance.enemyStats.text += $"\n<b>Aura:</b> {aura}";
        }
    }
}
