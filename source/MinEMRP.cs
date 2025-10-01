using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class MinEMRP
    {
        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix, HarmonyPatch(typeof(TooltipDisplay), "OnPointerEnter")]
        private static void TooltipDisplay_OnPointerEnter_postfix(TooltipDisplay __instance)
        {
            var character = Plugin.Character;
            var tt = _tooltipText.GetValue(__instance.tooltip) as Text;

            switch (__instance.id)
            {
                case 3:
                    tt.text += buildMinString(character.energyPower);
                    break;

                case 17:
                    tt.text += buildMinString(character.magic.magicPower);
                    break;

                case 1002:
                    tt.text += buildMinString(character.res3.res3Power);
                    break;
            }
        }

        private static string buildMinString(float power)
        {
            var ulp = power.ULP();
            var min = minPurchase(ulp);
            var next = getValueWhenMinChanges(ulp);

            return $"\n\nMin purchase: <b>{min}</b> (spacing: {ulp})"
                + $"\nIncreases at <b>{(ulong)next:#,###0}</b> power";
        }

        private static float minPurchase(float ulp)
        {
            if (ulp < 0.25f)
                return 0.1f;

            if (ulp < 2f)
                return 1f;

            return (int)ulp / 2 + 1;
        }

        private static float getValueWhenMinChanges(float ulp)
        {
            var nextULP = 0f;

            if (ulp < .25f)
                nextULP = .25f;

            else if (ulp < 2f)
                nextULP = 2f;

            else
                nextULP = ulp * 2f;

            var log2 = Math.Log(nextULP, 2.0);
            var k = (int)Math.Round(log2);
            var e = k + 23;

            return Mathf.Pow(2.0f, e);
        }



        // these transpilers change the EMR3 power that's displayed on the purchase screens in the exp shop
        // since the powers are floats, their actual values are not displayed - rounded to 6 or 7 significant digits
        // this changes them to show their actual values by casting to ulong, but only if ULP >= 1 (the spacing between float values)

        [HarmonyTranspiler, HarmonyPatch(typeof(EnergyPurchases), "updateEnergyPurchases")]
        private static IEnumerable<CodeInstruction> EnergyPurchases_updateEnergyPurchases_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var energyPower = typeof(Character).GetField("energyPower");
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldflda, energyPower))
                .SetOpcodeAndAdvance(OpCodes.Ldfld)
                .RemoveInstructions(2)
                .Insert(Transpilers.EmitDelegate(getPowerString));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(MagicPurchases), "updateMagicPurchases")]
        private static IEnumerable<CodeInstruction> MagicPurchases_updateMagicPurchases_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var magicPower = typeof(Magic).GetField("magicPower");
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldflda, magicPower))
                .SetOpcodeAndAdvance(OpCodes.Ldfld)
                .RemoveInstructions(2)
                .Insert(Transpilers.EmitDelegate(getPowerString));

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(Resource3Purchases), "updateRes3Purchases")]
        private static IEnumerable<CodeInstruction> Resource3Purchases_updateRes3Purchases_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var res3Power = typeof(Character).GetField("res3Power");
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldflda, res3Power))
                .SetOpcodeAndAdvance(OpCodes.Ldfld)
                .RemoveInstructions(2)
                .Insert(Transpilers.EmitDelegate(getPowerString));

            return cm.InstructionEnumeration();
        }

        private static string getPowerString(float power)
        {
            var ulp = power.ULP();
            if (ulp < 1)
                return power.FormatRoundTripWithSeparators();

            return ((ulong)power).ToString("N0");
        }
    }
}
