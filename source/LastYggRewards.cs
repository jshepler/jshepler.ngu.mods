using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class LastYggRewards
    {
        private static string[] _texts// = new string[21];
        {
            get => ModSave.Data.LastYggRewards;
        }
        
        private static int _fruitId = -1;

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "consumeFruit", typeof(int)), HarmonyPatch(typeof(FruitController), "harvest", typeof(int))]
        private static void FruitController_consumeFruit_int_prefix(int fruitID)
        {
            _fruitId = fruitID;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TooltipLog), "AddEvent")]
        private static void TooltipLog_AddEvent_prefix(string eventString)
        {
            if (_fruitId == -1) return;

            _texts[_fruitId] = eventString;
            _fruitId = -1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(FruitController), "showTooltip")]
        private static void FruitController_showTooltip_postfix(FruitController __instance, ref string ___message)
        {
            var fruitId = __instance.id;
            var text = _texts[fruitId];

            if (!__instance.validID(fruitId) || string.IsNullOrEmpty(text)) return;

            ___message += $"\n\n<b>Last Gained:</b>\n{text}";
            __instance.tooltip.showTooltip(___message);
        }
    }
}
