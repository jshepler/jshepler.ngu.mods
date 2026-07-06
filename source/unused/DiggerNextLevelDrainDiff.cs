using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DiggerNextLevelDrainDiff
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GoldDiggerUIController), "updateUI")]
        private static void GoldDiggerUIController_updateUI_postfix(GoldDiggerUIController __instance)
        {
            var character = __instance.character;
            var diggerId = __instance.id;
            var digger = character.diggers.diggers[__instance.id];

            if (!character.InMenu(Menu.GoldDiggers)
                || diggerId < 0 || diggerId >= character.diggers.diggers.Count
                || !digger.active
                || digger.curLevel + 1 == character.allDiggers.hardCapLevel(diggerId))
            {
                return;
            }

            var curDrain = character.allDiggers.drain(diggerId);
            var nextDrain = character.allDiggers.drain(diggerId, 1);

            __instance.diggerInfo.text += $"\n<b>GPS Drain (Next Level Diff): {character.display(nextDrain - curDrain)}</b>";
        }
    }
}
