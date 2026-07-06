using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class YggInactiveDiggerWarnings
    {
        const int FOK_ID = 3;
        const int FOR_ID = 9;
        const int EXP_DIGGER_ID = 11;
        const int PP_DIGGER_ID = 8;

        private static bool _expDiggerActive => Plugin.Character.diggers.diggers[EXP_DIGGER_ID].active;
        private static bool _ppDiggerActive => Plugin.Character.diggers.diggers[PP_DIGGER_ID].active;

        [HarmonyPostfix, HarmonyPatch(typeof(FruitController), "updateButton")]
        private static void FruitController_updateButton_postfix(FruitController __instance)
        {
            if (showWarning(__instance.id))
                __instance.actionButton.image.color = Plugin.ButtonColor_Red;
        }

        // Canvas/Yggdrasil Canvas/Yggdrasil, The World Tree/Bottom Panel/Button
        [HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "Start")]
        private static void AllYggdresil_Start_postfix()
        {
            var button = GameObject.Find("Canvas/Yggdrasil Canvas/Yggdrasil, The World Tree/Bottom Panel/Button")
                .GetComponent<Button>();

            Plugin.BeginCoroutine(checkDiggers(button));
        }

        private static IEnumerator checkDiggers(Button button)
        {
            var delay = new WaitForSeconds(0.5f);

            while (true)
            {
                var showRed = showWarning(FOK_ID) || showWarning(FOR_ID);
                button.image.color = showRed ? Plugin.ButtonColor_Red : Color.white;

                yield return delay;
            }
        }

        private static bool showWarning(int fruitId)
        {
            var character = Plugin.Character;
            if ((fruitId != FOK_ID && fruitId != FOR_ID)
                || !character.yggdrasil.fruits[fruitId].activated
                || !character.yggdrasil.fruits[fruitId].eatFruit)
                return false;

            var currentTier = character.yggdrasilController.fruits[0].harvestTier(fruitId);
            var maxTier = (int)character.yggdrasil.fruits[fruitId].maxTier;
            var diggerActive = fruitId == FOK_ID ? _expDiggerActive : _ppDiggerActive;

            return (currentTier == maxTier && !diggerActive);
        }
    }
}
