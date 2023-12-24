using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InfinityCubeSoftCap
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(LoadoutController), "infinityCubeTooltip")]
        private static IEnumerable<CodeInstruction> LoadoutController_infinityCubeTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var powerString = "<b>Power:</b> ";
            var toughString = "\n<b>Toughness:</b> ";

            var inventoryCubePower = typeof(Inventory).GetField("cubePower");
            var concat2strings = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            var concat3strings = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) });

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .RemoveInstructions(8)
                .InsertAndAdvance(Transpilers.EmitDelegate(CubePowerWithSoftcap))

                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, toughString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, toughString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .RemoveInstructions(8)
                .InsertAndAdvance(Transpilers.EmitDelegate(CubeToughnessWithSoftcap))
                
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Call, concat2strings))
                .SetInstruction(new CodeInstruction(OpCodes.Call, concat3strings))
                .Advance(-1)
                .Insert(Transpilers.EmitDelegate(NextTier));

            var labelAfterSoftcapWarnings = cm
                .MatchBack(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldarg_0))
                .Labels[0];

            cm.MatchBack(false, new CodeMatch(OpCodes.Ldfld, inventoryCubePower))
                .Advance(-1)
                .MatchBack(false, new CodeMatch(OpCodes.Ldfld, inventoryCubePower))
                .Advance(-3)
                .Set(OpCodes.Br, labelAfterSoftcapWarnings);

            return cm.InstructionEnumeration();
        }

        private static string CubePowerWithSoftcap()
        {
            var character = Plugin.Character;
            var cubePower = character.inventory.cubePower;
            var softcap = character.inventoryController.cubePowerSoftcap();
            var capped = cubePower <= softcap ? cubePower : softcap + Mathf.Pow(cubePower - softcap, 0.5f);
            var color = cubePower >= softcap ? "green" : "red";

            return $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)} (sc)\n<b>Power (uncapped):</b> {character.display(cubePower)}";
        }

        private static string CubeToughnessWithSoftcap()
        {
            var character = Plugin.Character;
            var cubeToughness = character.inventory.cubeToughness;
            var softcap = character.inventoryController.cubeToughnessSoftcap();
            var capped = cubeToughness <= softcap ? cubeToughness : softcap + Mathf.Pow(cubeToughness - softcap, 0.5f);
            var color = cubeToughness >= softcap ? "green" : "red";

            return $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)} (sc)\n<b>Toughness (uncapped):</b> {character.display(cubeToughness)}";
        }

        private static string NextTier()
        {
            var character = Plugin.Character;
            var cubePower = character.inventory.cubePower;
            var cubeToughness = character.inventory.cubeToughness;
            var total = cubePower + cubeToughness;
            var nextTier = (int)Mathf.Log10(total);

            if (nextTier > 10)
                return "\n\n<color=blue><b>AT MAX TIER</b></color>";

            var need = Mathf.Pow(10, nextTier + 1) - total;
            return $"\n\n<b>P + T (uncapped):</b> {character.display(total)}"
                    + $"\n<b>Need for next tier:</b> {character.display(need)}";
        }
    }
}
