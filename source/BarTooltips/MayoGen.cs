using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class MayoGen
    {
        private static bool _altIsDown = false;
        private static Text _text;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            _text = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Mana Pod/Amount Title").GetComponent<Text>();

            Plugin.OnUpdate += (o, e) =>
            {
                if (Plugin.Character == null)
                    return;

                var altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                if (_altIsDown != altIsDown)
                {
                    _altIsDown = altIsDown;
                    Plugin.Character.cardsController.updateManaPods();
                }
            };
        }

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

        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "updateManaAmount")]
        private static bool CardsController_updateManaAmount_prefix(int manaID, CardsController __instance)
        {
            var character = __instance.character;

            if (!character.InMenu(Menu.Cards))
                return false;

            _text.text = _altIsDown ? "<color=blue><b>DECK</b></color>" : "<b>Amount</b>";
            if (!_altIsDown)
                return true;

            var total = character.cards.cards.Sum(c => c.manaCosts[manaID]);
            __instance.manaUI[manaID].manaCount.text = $"<color=blue><b>{total}</b></color>";

            return false;
        }
    }
}
