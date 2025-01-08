using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HackSummaryTooltip
    {
        private static HacksController _controller;
        private static string _display(double d) => $"{Plugin.Character.display(d * 100.0, 1)}%";

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _controller = __instance;

            var go = GameObject.Find("Canvas/Hacks Canvas/Hacks Menu + HacksController/WTF Button");
            go.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnMouseExit);
        }

        private static string[] _names = ["A/D", "Adv", "TM", "DC", "Augs", "E-NGU", "M-NGU", "Blood", "QP", "Daycare", "Exp", "Number", "PP", "Hacks", "Wishes" ];
        private static void OnPointerEnter(PointerEventData e)
        {
            var hacks = Plugin.Character.hacks.hacks;
            var levels = new string[15];
            var bonuses = new string[15];
            for (var x = 0; x < 15; x++)
            {
                levels[x] = $"L{hacks[x].level}";
                bonuses[x] = _display(_controller.hackBonus(x));
            }

            var col1 = _names.Max(s => s.Length);
            var col2 = levels.Max(s => s.Length);
            var col3 = bonuses.Max(s => s.Length);
            var text = Enumerable.Range(0, 15)
                .Select(i => $"<b>{_names[i].PadLeft(col1)}:</b> {levels[i].PadLeft(col2)} {bonuses[i].PadLeft(col3)}")
                .Join(s => s, "\n");

            var header = "Hacks Summary";
            var padding = (col1 + col2 + col3 + 4) / 2 - (header.Length / 2);
            text = string.Empty.PadLeft(padding) + $"<b><color=blue>{header}</color></b>\n\n{text}";

            Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
            Plugin.ShowTooltip(text);
        }

        private static void OnMouseExit(PointerEventData e)
        {
            Plugin.HideTooltip();
            Plugin.ResetTooltipFont();
        }
    }
}
