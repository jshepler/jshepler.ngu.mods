using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HackBonusesTooltip
    {
        // Canvas/Hacks Canvas/Hacks Menu + HacksController/Title Panel/Title
        // Canvas/Hacks Canvas/Hacks Menu + HacksController/WTF Button

        private static HacksController _controller;
        private static string _display(double d) => $"{Plugin.Character.display(d * 100.0, 1)}%";

        private static FieldInfo _tooltipTextField => typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Text _tooltipText;

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _controller = __instance;
            _tooltipText = (Text)_tooltipTextField.GetValue(__instance.tooltip);

            var go = GameObject.Find("Canvas/Hacks Canvas/Hacks Menu + HacksController/WTF Button");

            go.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnMouseExit);

            //go.GetComponent<Text>().raycastTarget = true;
        }

        private static void OnPointerEnter(PointerEventData e)
        {
            var text = "       <b><color=blue>Current Hack Effects</color></b>\n"
                + $"\n<b>Attack/Defense:</b> {_display(_controller.totalStatBonus())}"
                + $"\n     <b>Adv Stats:</b> {_display(_controller.totalAdventureBonus())}"
                + $"\n      <b>TM Speed:</b> {_display(_controller.totalTMSpeedBonus())}"
                + $"\n   <b>Drop Chance:</b> {_display(_controller.totalDropChanceBonus())}"
                + $"\n <b>Augment Speed:</b> {_display(_controller.totalAugSpeedBonus())}"
                + $"\n   <b>E NGU Speed:</b> {_display(_controller.totalEnergyNGUBonus())}"
                + $"\n   <b>M NGU Speed:</b> {_display(_controller.totalMagicNGUBonus())}"
                + $"\n    <b>Blood Gain:</b> {_display(_controller.totalBloodGainBonus())}"
                + $"\n       <b>QP Gain:</b> {_display(_controller.totalQPGainBonus())}"
                + $"\n <b>Daycare Speed:</b> {_display(_controller.totalDaycareSpeedBonus())}"
                + $"\n      <b>EXP Gain:</b> {_display(_controller.totalEXPBonus())}"
                + $"\n        <b>Number:</b> {_display(_controller.totalNumberBonus())}"
                + $"\n       <b>PP Gain:</b> {_display(_controller.totalPPGainBonus())}"
                + $"\n    <b>Hack Speed:</b> {_display(_controller.totalHackBonus())}"
                + $"\n    <b>Wish Speed:</b> {_display(_controller.totalWishSpeedBonus())}";

            _tooltipText.font = Fonts.LiberationMono_Regular;
            Plugin.ShowTooltip(text);
        }

        private static void OnMouseExit(PointerEventData e)
        {
            Plugin.HideTooltip();
            _tooltipText.font = Fonts.LiberationSans_Regular;
        }
    }
}
