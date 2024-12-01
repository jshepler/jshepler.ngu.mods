using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;

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

            if (!__instance.validID(fruitId) || string.IsNullOrEmpty(text))
                return;

            ___message += $"\n\n<b>Last Gained:</b>\n{text}";
            __instance.tooltip.showTooltip(___message);
        }

        // appends [NGU YIELD FH] to the fruit name in the tooltip
        // NGU = ngu yield, YIELD = ygg yield from equipment (and quirk 92), FH = first harvest perk
        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "showTooltip")]
        private static IEnumerable<CodeInstruction> FruitController_showTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<b>"))
                .Advance(1)
                .RemoveInstructions(4)
                .Advance(2)
                .SetInstruction(Transpilers.EmitDelegate(AppendFruitModifiers));

            return cm.InstructionEnumeration();
        }

        private static string AppendFruitModifiers(int fruitId)
        {
            var name = Plugin.Character.yggdrasilController.fruitName[fruitId];
            var mod = Fruits.ModifedBy[(FruitId)fruitId];
            var ngu = mod.NGU ? Plugin.TEXT_GREEN : Plugin.TEXT_RED;
            var yield = mod.YIELD ? Plugin.TEXT_GREEN : Plugin.TEXT_RED;
            var fh = mod.FH ? Plugin.TEXT_GREEN : Plugin.TEXT_RED;

            return $"{name} [<color={ngu}>NGU</color> <color={yield}>YIELD</color> <color={fh}>FH</color>]";
        }
    }
}
