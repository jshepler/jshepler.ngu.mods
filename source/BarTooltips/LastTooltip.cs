using HarmonyLib;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class LastTooltip
    {
        internal static string Message;

        [HarmonyPostfix, HarmonyPatch(typeof(HoverTooltip), "showTooltip", typeof(string))]
        private static void HoverTooltip_showTooltip_postfix(string message)
        {
            Message = message;
        }
    }
}
