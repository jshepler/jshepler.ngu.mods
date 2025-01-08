using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ResourceAllocationSummary
    {
        private static bool _altDown => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        [HarmonyPrefix, HarmonyPatch(typeof(Energy), "tooltipDisplay")]
        private static bool Eneryg_tooltipDisplay_prefix()
        {
            if (!_altDown)
            {
                Plugin.ResetTooltipFont();
                return true;
            }

            var character = Plugin.Character;
            var disp = character.display;

            var labels = new List<string>();
            var values = new List<string>();

            var bt = character.training.attackEnergy.Sum()
                   + character.training.defenseEnergy.Sum();
            if (bt > 0)
            {
                labels.Add("Basic Training");
                values.Add(disp(bt));
            }

            var augs = character.augments.augs.Sum(a => a.augEnergy + a.upgradeEnergy);
            if (augs > 0)
            {
                labels.Add("Augments");
                values.Add(disp(augs));
            }

            var at = character.advancedTraining.energy.Sum();
            if (at > 0)
            {
                labels.Add("Adv Training");
                values.Add(disp(at));
            }

            var tm = character.machine.speedEnergy;
            if (tm > 0)
            {
                labels.Add("Time Machine");
                values.Add(disp(tm));
            }

            var wand = character.wandoos98.wandoosEnergy;
            if (wand > 0)
            {
                labels.Add("Wandoos");
                values.Add(disp(wand));
            }

            var ngus = character.NGU.skills.Sum(n => n.energy);
            if (ngus > 0)
            {
                labels.Add("NGUs");
                values.Add(disp(ngus));
            }

            var wishes = character.wishes.wishes.Sum(w => w.energy);
            if (wishes > 0)
            {
                labels.Add("Wishes");
                values.Add(disp(wishes));
            }

            var text = $"<color=#2E814A><b>Energy Allocation Summary</b></color>\n\n";
            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                var totalLen = maxLen + values.Max(v => v.Length) + 2;
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
            Plugin.ShowTooltip(text);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MagicDisplay), "tooltipDisplay")]
        private static bool MagicDisplay_tooltipDisplay_prefix()
        {
            if (!_altDown)
            {
                Plugin.ResetTooltipFont();
                return true;
            }

            var character = Plugin.Character;
            var disp = character.display;

            var labels = new List<string>();
            var values = new List<string>();

            var tm = character.machine.goldMultiMagic;
            if (tm > 0)
            {
                labels.Add("Time Machine");
                values.Add(disp(tm));
            }

            var bm = character.bloodMagic.ritual.Sum(r => r.magic);
            if (bm > 0)
            {
                labels.Add("Blood Magic");
                values.Add(disp(bm));
            }

            var wand = character.wandoos98.wandoosMagic;
            if (wand > 0)
            {
                labels.Add("Wandoos");
                values.Add(disp(wand));
            }

            var ngus = character.NGU.magicSkills.Sum(n => n.magic);
            if (ngus > 0)
            {
                labels.Add("NGUs");
                values.Add(disp(ngus));
            }

            var wishes = character.wishes.wishes.Sum(w => w.magic);
            if (wishes > 0)
            {
                labels.Add("Wishes");
                values.Add(disp(wishes));
            }

            var text = $"<color=#275AAD><b>Magic Allocation Summary</b></color>\n\n";
            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                var totalLen = maxLen + values.Max(v => v.Length) + 2;
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
            Plugin.ShowTooltip(text);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Resource3Display), "tooltipDisplay")]
        private static bool Resource3Display_tooltipDisplay_prefix()
        {
            if (!_altDown)
            {
                Plugin.ResetTooltipFont();
                return true;
            }

            var character = Plugin.Character;
            var disp = character.display;

            var labels = new List<string>();
            var values = new List<string>();

            var hacks = character.hacks.hacks.Sum(h => h.res3);
            if (hacks > 0)
            {
                labels.Add("Hacks");
                values.Add(disp(hacks));
            }

            var wishes = character.wishes.wishes.Sum(w => w.res3);
            if (wishes > 0)
            {
                labels.Add("Wishes");
                values.Add(disp(wishes));
            }

            var title = $"{character.res3.res3Name} Allocation Summary";
            var color = "#" + character.res3.colourHexString();
            var text = $"<color={color}><b>{title}</b></color>\n\n";

            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                var totalLen = maxLen + values.Max(v => v.Length) + 2;
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
            Plugin.ShowTooltip(text);

            return false;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(Energy), "OnPointerExit"),
            HarmonyPatch(typeof(MagicDisplay), "OnPointerExit"),
            HarmonyPatch(typeof(Resource3Display), "OnPointerExit")]
        private static void OnPointerExit_postfix()
        {
            Plugin.ResetTooltipFont();
        }
    }
}
