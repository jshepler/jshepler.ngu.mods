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
        private static bool _inBlind => Plugin.Character.challenges.blindChallenge.inChallenge;

        [HarmonyPrefix,
            HarmonyPatch(typeof(Energy), "tooltipDisplay"),
            HarmonyPatch(typeof(MagicDisplay), "tooltipDisplay"),
            HarmonyPatch(typeof(Resource3Display), "tooltipDisplay")]
        private static bool tooltipDisplay_prefix()
        {
            if (!_altDown || _inBlind)
            {
                Plugin.ResetTooltipFont();
                return true;
            }

            var text = $"{buildEnergySummary()}\n\n\n{buildMagicSummary()}";
            if(Plugin.Character.res3.capRes3 >= 10000)
                text += $"\n\n\n{buildRes3Summary()}";

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

        private static string buildEnergySummary()
        {
            var character = Plugin.Character;
            var tec = character.totalCapEnergy();
            var disp = (double d) => $"{character.display(d)} <color=blue>({d / tec * 100.0:0.##}%)</color>";

            var labels = new List<string>();
            var values = new List<string>();

            var bt = character.training.attackEnergy.Sum()
                   + character.training.defenseEnergy.Sum();
            if (bt > 0)
            {
                labels.Add("BT");
                values.Add(disp(bt));
            }

            var augs = character.augments.augs.Sum(a => a.augEnergy + a.upgradeEnergy);
            if (augs > 0)
            {
                labels.Add("Augs");
                values.Add(disp(augs));
            }

            var at = character.advancedTraining.energy.Sum();
            if (at > 0)
            {
                labels.Add("AT");
                values.Add(disp(at));
            }

            var tm = character.machine.speedEnergy;
            if (tm > 0)
            {
                labels.Add("TM");
                values.Add(disp(tm));
            }

            var wand = character.wandoos98.wandoosEnergy;
            if (wand > 0)
            {
                labels.Add("Wand");
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

            var text = $"<color=#2E814A><b>Energy Allocation Summary</b></color>\n---------------------------------\n";
            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            return text;
        }

        private static string buildMagicSummary()
        {
            var character = Plugin.Character;
            var tmc = character.totalCapMagic();
            var disp = (double d) => $"{character.display(d)} <color=blue>({d / tmc * 100.0:0.##}%)</color>";

            var labels = new List<string>();
            var values = new List<string>();

            var tm = character.machine.goldMultiMagic;
            if (tm > 0)
            {
                labels.Add("TM");
                values.Add(disp(tm));
            }

            var bm = character.bloodMagic.ritual.Sum(r => r.magic);
            if (bm > 0)
            {
                labels.Add("BM");
                values.Add(disp(bm));
            }

            var wand = character.wandoos98.wandoosMagic;
            if (wand > 0)
            {
                labels.Add("Wand");
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

            var text = $"<color=#275AAD><b>Magic Allocation Summary</b></color>\n---------------------------------\n";
            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            return text;
        }

        private static string buildRes3Summary()
        {
            var character = Plugin.Character;
            var trc = character.totalCapRes3();
            var disp = (double d) => $"{character.display(d)} <color=blue>({d / trc * 100.0:0.##}%)</color>";

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
            var text = $"<color={color}><b>{title}</b></color>\n---------------------------------\n";

            if (labels.Count > 0)
            {
                var maxLen = labels.Max(l => l.Length);
                text += labels.Zip(values, (l, v) => $"<b>{l.PadLeft(maxLen)}:</b> {v}").Join(s => s, "\n");
            }

            return text;
        }
    }
}
