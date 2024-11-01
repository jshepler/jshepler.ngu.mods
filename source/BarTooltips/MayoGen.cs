using HarmonyLib;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class MayoGen
    {
        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "showManaGenTooltip", [])]
        private static bool CardsController_showManaGenTooltip_postfix(CardsController __instance)
        {
            var character = __instance.character;

            var id = __instance.curManaID;
            if (id < 0 || id > character.cards.manas.Count)
                return false;

            var name = __instance.getManaName(id);
            var ppt = __instance.manaGenProgressPerTick(id);
            var secondsPerMayo = 1f / ppt / 50f;
            var mayoPerDay = 86400f / secondsPerMayo;
            var timeToNextMayo = (1f - character.cards.manas[id].progress) / ppt / 50f;

            var runningText = character.cards.manas[id].running ? string.Empty : " <color=red>(NOT RUNNING)</color>";

            var text = $"<b>{name}</b>"
                + $"\n\n<b>Time per Mayo:</b> {NumberOutput.timeOutput(secondsPerMayo)}"
                + $"\n<b> ... Remaining:</b> {NumberOutput.timeOutput(timeToNextMayo)}{runningText}"
                + $"\n\n<b>Mayo/day:</b> {mayoPerDay}";

            __instance.tooltip.showTooltip(text);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "updateManaGenText")]
        private static bool CardsController_updateManaGenText_prefix(CardsController __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Cards))
                return false;

            var curGens = __instance.curManaToggleCount();
            var maxGens = __instance.maxManaGenSize();
            var ppt = __instance.manaGenProgressPerTick() * curGens;
            var secondsPerMayo = 1f / ppt / 50f;
            var mayoPerDay = 86400f / secondsPerMayo;

            __instance.manaGenText.text = $"<b>Total Mayo /day: <color=blue>{mayoPerDay:#,##0.#}</color></b>";
            //__instance.manaGenText.resizeTextForBestFit = true;
            return false;
        }
    }
}
