using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    internal static class Tooltips
    {
        internal static string BuildCurrentCapText(double currentCap, double overCappedDuration, float ppt)
        {
            var capPct = ((decimal)ppt).Truncate(4) * 100m;
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);

            var text = $"<b>Current Speed Cap:</b> {Plugin.Character.display(currentCap)}"
                + $"\n<b>% Allocated:</b> {capPct}%";

            if (overCappedDuration > 0)
                text += $" ({NumberOutput.timeOutput(overCappedDuration)})";

            text += $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";

            return text;
        }

        internal static string BuildTimeToTargetText(double secondsToTarget)
        {
            return $"<b>Time to Target:</b> {NumberOutput.timeOutput(secondsToTarget)}";
        }
    }
}
