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

            /* ppt = mc * mp / speed / sad * bonus
             *   t = c  * p  / d     / s   * b
             *   c = d * s * t / (p * b)
             *   c = d * s * 1 / (p * b)  setting t = 1 to calc cap (how much m to reach 1.0 progress per tick, or 100% speed, or 1 tick per bar)
             *   c = d * s / (p * b)
             */

            //var realCap = (double)__instance.character.bloodMagicController.sadisticSpeedDividers[__instance.id]
            //    * (double)__instance.sadisticDivider()
            //    / ((double)__instance.character.totalMagicPower() * (double)__instance.totalBloodMagicSpeedBonus());

            ___message += $"\n\n<b>% Allocated:</b> {capPct}%"
                + $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";
                //+ $"\n   ({realCap})";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
