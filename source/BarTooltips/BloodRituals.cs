using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class BloodRituals
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BloodMagicController), "showTooltip")]
        private static void BloodMagicController_showTooltip_postfix(BloodMagicController __instance, ref string ___message)
        {
            var ppt = __instance.progressPerTick();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            ___message += $"\n\n<b>% Allocated:</b> {capPct}%"
                + $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
