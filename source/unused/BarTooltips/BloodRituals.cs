using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class BloodRituals
    {
        [HarmonyPrefix, HarmonyPatch(typeof(BloodMagicController), "showTooltip")]
        private static bool BloodMagicController_showTooltip_prefix(BloodMagicController __instance)
        {
            var allBM = __instance.character.bloodMagicController;
            var id = __instance.id;
            var bloodAdded = allBM.bloodAdded(id);
            var totalBoost = __instance.totalBoost();
            var timeLeft = __instance.timeLeft();
            var baseCost = __instance.baseCost;

            double speedCap = __instance.capValue();
            if (__instance.character.settings.rebirthDifficulty == difficulty.sadistic)
                speedCap = FixCapButtonCalcs.calcBloodRitualCap(id);

            var gps = __instance.goldConsumedPerSecond(); // baseCost * barFillsPerSecond
            var bps = __instance.bloodGainedPerSecond(); // bloodAdded * barFillsPerSecond

            var ppt = __instance.progressPerTick();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            var gpsMax = 0.0;
            var bpsMax = 0.0;
            if (ppt == 0)
            {
                var pptMax = __instance.progressPerTick1000();
                var fillsPerSecond = 50f / (float)Mathf.CeilToInt(1f / Mathf.Min(pptMax, 1f));
                gpsMax = baseCost * fillsPerSecond;
                bpsMax = bloodAdded * fillsPerSecond;
            }

            var magCap = Plugin.Character.totalCapMagic();
            var display = Plugin.Character.display;
            var text = $"<b>Blood Gained Per Bar Fill:</b> {display(bloodAdded)}"
                + $"\n<b>Total Blood gained from this ritual:</b> {display(totalBoost)}"
                + $"\n\n<b>Time left to Ritual Completion:</b> {timeLeft}"
                + $"\n\n<b>Cost of Ritual:</b> {display(baseCost)} Gold"
                + $"\n\n<b>Current Speed Cap:</b> {display(speedCap)} Magic"

                + $"\n\n<b>Gold Consumed Per Second:</b> {display(gps)}"
                + (ppt == 0 ? $"\n   <b>with {display(magCap)} Magic:</b> {display(gpsMax)}" : string.Empty)

                + $"\n\n<b>Blood Gained Per Second:</b> {display(bps)}"
                + (ppt == 0 ? $"\n   <b>with {display(magCap)} Magic:</b> {display(bpsMax)}" : string.Empty);

            if (!Plugin.Character.challenges.blindChallenge.inChallenge)
                text += $"\n\n<b>% Allocated:</b> {capPct}%"
                    + $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";

            __instance.tooltip.showTooltip(text);
            return false;
        }
    }
}
