using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class BloodRituals
    {
        private static Regex rex = new Regex("<b>Current Speed Cap: </b>.*?Magic");

        [HarmonyPostfix, HarmonyPatch(typeof(BloodMagicController), "showTooltip")]
        private static void BloodMagicController_showTooltip_postfix(BloodMagicController __instance, ref string ___message)
        {
            var ppt = __instance.progressPerTick();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            if (__instance.character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                var realCap = FixCapButtonCalcs.calcBloodRitualCap(__instance.id);
                ___message = rex.Replace(___message, $"<b>Current Speed Cap: </b> {__instance.character.display(realCap)} Magic");
            }

            ___message += $"\n\n<b>% Allocated:</b> {capPct}%"
                + $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
