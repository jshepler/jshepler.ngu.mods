using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedYggTooltips
    {
        private static string[] _texts
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
            if (fruitId < 0 || fruitId >= _texts.Length)
                return;

            var text = _texts[fruitId];
            if (!__instance.validID(fruitId) || string.IsNullOrEmpty(text))
                return;

            // these 5 fruits have levels and their respective bonuses are derived from these levels
            // the wiki calls these "invisible levels" - make them visible
            var ygg = __instance.character.yggdrasil;
            if (fruitId == 1)
                ___message += $"\n\n<b>Level:</b> {ygg.fruits[1].totalLevels:#,##0}"
                    + "\n<size=10>(A/D Bonus = 1 + Level ^ 1.5)</size>";

            else if (fruitId == 5)
                ___message += $"\n\n<b>Level:</b> {ygg.totalLuck:#,##0}"
                    + "\n<size=10>(DC Bonus = 1 + Level * 0.0005)</size>";

            else if (fruitId == 6)
                ___message += $"\n\n<b>Level:</b> {ygg.totalPermStatBonus:#,##0}"
                    + "\n<size=10>(A/D Bonus = 1 + Level ^ 2 * 0.0005)</size>";

            else if (fruitId == 8)
                ___message += $"\n\n<b>Level:</b> {ygg.totalPermNumberBonus:#,##0}"
                    + "\n<size=10>(Number Bonus = 1 + Level ^ 1.3 * 0.0005)</size>";

            else if (fruitId == 11)
                ___message += $"\n\n<b>Level:</b> {ygg.totalPermStatBonus2:#,##0}"
                    + "\n<size=10>(A/D Bonus = 1 + Level ^ 1.3 * 0.000001)</size>";

            ___message += $"\n\n<b>Last Gained:</b>\n{text}";
            __instance.tooltip.showTooltip(___message);
        }

        // to be consistent with permStatBonus() and permStatBonus2(), add 1 to convert from bonus to multiplier
        // this method is only used in displaying the bonus at the bottom of the ygg screen
        [HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "totalStatBonus")]
        private static void AllYggdrasil_totalStatBonus_postfix(ref double __result)
        {
            __result += 1.0;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumePowerFruit")]
        private static IEnumerable<CodeInstruction> FruitController_consumePowerFruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You eat the fruit and icrease your Attack and Defense! Power Fruit α's multiplier increased from <b>"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, (byte)4))
                .SetInstruction(Transpilers.EmitDelegate((long l) => $"You eat the fruit and increase your Attack and Defense! You gain +{l:#,##0} levels, increasing Power Fruit α's multiplier from <b>"));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumeLuckFruit")]
        private static IEnumerable<CodeInstruction> FruitController_consumeLuckFruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You eat the fruit and gain:\n+"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .SetInstruction(Transpilers.EmitDelegate((long l) => $"You eat the fruit and gain:\n+{l:#,##0} levels, resulting in +"));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumePermStatFruit")]
        private static IEnumerable<CodeInstruction> FruitController_consumePermStatFruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You eat the fruit. It tastes fruity. You also gain:\n+"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .SetInstruction(Transpilers.EmitDelegate((long l) => $"You eat the fruit. It tastes fruity. You also gain:\n+{l:#,##0} levels, resulting in +"));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumePermNumberFruit")]
        private static IEnumerable<CodeInstruction> FruitController_consumePermNumberFruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You eat the fruit. It tastes fruity. You also gain:\n+"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .SetInstruction(Transpilers.EmitDelegate((long l) => $"You eat the fruit. It tastes fruity. You also gain:\n+{l:#,##0} levels, resulting in +"));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumePermStatFruit2")]
        private static IEnumerable<CodeInstruction> FruitController_consumePermStatFruit2_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You put on an extra strong pair of shades and eat the fruit. The glasses melt onto your face causing unbearable pain, but you gain:\n+"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .SetInstruction(Transpilers.EmitDelegate((long l) => $"You put on an extra strong pair of shades and eat the fruit. The glasses melt onto your face causing unbearable pain, but you gain:\n+{l:#,##0} levels, resulting in +"));

            return cm.InstructionEnumeration();//.DumpToLog();
        }




        // appends [NGU YIELD FH] to the fruit name in the tooltip
        // NGU = ngu yield, YIELD = ygg yield from equipment (and quirk 92), FH = first harvest perk
        // and inserts time to max tier
        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "showTooltip")]
        private static IEnumerable<CodeInstruction> FruitController_showTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var concat3 = typeof(string).GetMethod("Concat", [typeof(object), typeof(object), typeof(object)]);
            var concat4 = typeof(string).GetMethod("Concat", [typeof(object), typeof(object), typeof(object), typeof(object)]);

            if (concat3 == null)
            {
                Plugin.LogInfo("concat3 is null");
                return instructions;
            }

            if (concat4 == null)
            {
                Plugin.LogInfo("concat4 is null");
                return instructions;
            }

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<b>"))
                .Advance(1)
                .RemoveInstructions(4)
                .Advance(2)
                .SetInstruction(Transpilers.EmitDelegate(AppendFruitModifiers))

                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n\n<b>Time to next Tier:</b> "))
                .SetOperandAndAdvance("\n<b>Time to next Tier:</b> ")

                .MatchForward(false, new CodeMatch(OpCodes.Call, concat3))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    Transpilers.EmitDelegate(InsertTimeToMaxTier))
                .SetOperandAndAdvance(concat4);

            return cm.InstructionEnumeration();
        }

        private static string AppendFruitModifiers(int fruitId)
        {
            var name = Plugin.Character.yggdrasilController.fruitName[fruitId];
            var mod = Fruits.ModifedBy[(FruitId)fruitId];
            var ngu = mod.NGU ? "blue" : "#cccccc";
            var yield = mod.YIELD ? "blue" : "#cccccc";
            var fh = mod.FH ? "blue" : "#cccccc";

            return $"{name} [<color={ngu}>NGU</color> <color={yield}>YIELD</color> <color={fh}>FH</color>]";
        }

        private static string InsertTimeToMaxTier(FruitController controller)
        {
            var id = controller.id;
            var fruit = controller.character.yggdrasil.fruits[id];
            var currentTier = controller.harvestTier(id);
            if (currentTier >= fruit.maxTier)
                return $"\n<b>Time to Max Tier:</b> DONE";

            var secondsPerTier = controller.tierThreshold();
            var secondsToNextTier = (int)(secondsPerTier - fruit.seconds % secondsPerTier);
            var tiersAfterNext = fruit.maxTier - currentTier - 1;
            var secondsToMaxTier = (int)(secondsToNextTier + tiersAfterNext * secondsPerTier);

            return $"\n<b>Time to max Tier:</b> {NumberOutput.timeOutput(secondsToMaxTier)}";
        }
    }
}
