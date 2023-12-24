using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class Beards
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeardController), "showTooltip")]
        private static void BeardController_showTooltip_postfix(BeardController __instance, ref string ___message)
        {
            var id = __instance.id;
            var character = __instance.character;

            var ppt = character.allBeards.beardProgressPerTick(id);
            var capPct = ((decimal)ppt).Truncate(4) * 100m;

            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);

            var secondsRemaining = 0f;
            if (ppt > 1)
            {
                var capLevel = Calculators.BeardCalculators[id].LevelFromResource(0);
                var currentLevel = character.beards.beards[id].beardLevel;
                secondsRemaining = (capLevel - currentLevel) / 50f;
            }

            ___message += $"\n\n<b>% Allocated:</b> {capPct}%";

            if (secondsRemaining > 0)
                ___message += $" ({NumberOutput.timeOutput(secondsRemaining)})";

            ___message += $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";
            __instance.tooltip.showTooltip(___message);
        }
    }
}
