using System.Collections;
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
        private static bool _altIsDown => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private static string[] _names = ["A/D", "Adv", "TM", "DC", "Augs", "E-NGU", "M-NGU", "Blood", "QP", "Daycare", "Exp", "Number", "PP", "Hacks", "Wishes"];
        private static Coroutine _cor;
        private static WaitForSeconds _delay = new WaitForSeconds(.1f);

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _controller = __instance;

            var go = GameObject.Find("Canvas/Hacks Canvas/Hacks Menu + HacksController/WTF Button");
            go.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(ShowTooltip)
                .OnPointerExit(HideTooltip);
        }

        private static void ShowTooltip(PointerEventData e)
        {
            HideTooltip(e);
            _cor = Plugin.BeginCoroutine(ShowTooltip());
        }

        private static void HideTooltip(PointerEventData e)
        {
            if (_cor != null)
                Plugin.EndCoroutine(_cor);

            _cor = null;
            Plugin.HideTooltip();
            Plugin.ResetTooltipFont();
        }

        private static IEnumerator ShowTooltip()
        {
            var hacks = Plugin.Character.hacks.hacks;

            while (true)
            {
                var levels = new string[16];
                var bonuses = new string[16];
                var totalSecondsToTargets = 0.0;

                for (var x = 0; x < 15; x++)
                {
                    var secondsToTarget = _altIsDown ? BarTooltips.Hacks.GetSecondsToTarget(x) : 0.0;
                    totalSecondsToTargets += secondsToTarget;

                    levels[x] = _altIsDown? $"T{hacks[x].target}" : $"L{hacks[x].level}";
                    bonuses[x] = _altIsDown ? $"{NumberOutput.timeOutput(secondsToTarget)}" : _display(_controller.hackBonus(x));
                }

                levels[15] = string.Empty;
                bonuses[15] = totalSecondsToTargets == 0.0 ? string.Empty : NumberOutput.timeOutput(totalSecondsToTargets);

                var col1 = _names.Max(s => s.Length);
                var col2 = levels.Max(s => s.Length);
                var col3 = bonuses.Max(s => s.Length);
                var text = Enumerable.Range(0, 15)
                    .Select(i => $"<b>{_names[i].PadLeft(col1)}:</b> {levels[i].PadLeft(col2)} {bonuses[i].PadLeft(col3)}")
                    .Join(s => s, "\n");

                if (totalSecondsToTargets > 0.0)
                    text += $"\n\n<b>{"Total Time".PadLeft(col1 + col2 + 1)}:</b> {bonuses[15].PadLeft(col3)}";

                var header = "Hacks Summary";
                var padding = (col1 + col2 + col3 + 4) / 2 - (header.Length / 2);
                text = string.Empty.PadLeft(padding) + $"<b><color=blue>{header}</color></b>\n\n{text}";

                Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
                Plugin.ShowTooltip(text);

                yield return _delay;
            }
        }
    }
}
