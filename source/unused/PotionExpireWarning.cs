using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PotionExpireWarning
    {
        private static float _expireSeconds = Options.PotionWarning.ExpireSeconds.Value;
        private static Button _button;
        private static bool _flash = false;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _button = GameObject.Find("Canvas/Features Canvas/Features Panel/Sellout Shop").GetComponent<Button>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance)
        {
            var arb = __instance.character.arbitrary;
            var show = inRange(arb.energyPotion1Time)
                || inRange(arb.magicPotion1Time)
                || inRange(arb.res3Potion1Time)
                || inRange(arb.energyBarBar1Time)
                || inRange(arb.magicBarBar1Time)
                || inRange(arb.lootcharm1Time)
                || inRange(arb.macGuffinBooster1Time)
                || inRange(arb.mayoSpeedPotTime);

            _button.image.color = show && _flash ? Plugin.ButtonColor_Red : Color.white;
            _flash = !_flash;
        }

        private static bool inRange(PlayerTime time)
        {
            return time.totalseconds > 0f && time.totalseconds < _expireSeconds;
        }
    }
}
