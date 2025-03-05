using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InputParser
    {
        private static MethodInfo longParse = typeof(long).GetMethod("Parse", [typeof(string)]);
        //private static long ParseScientificNotation(string s) => string.IsNullOrWhiteSpace(s) ? 0L : long.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands);


        
        // resource input box at top of screen
        [HarmonyPrefix, HarmonyPatch(typeof(EnergyInputController), "validateInput")]
        private static bool EnergyInputController_validateInput_prefix(EnergyInputController __instance)
        {
            var text = __instance.energyRequested.text.ToLower();
            var value = parseLong(text);
            if (value < 1)
                value = 1;

            __instance.energyMagicInput = value;
            __instance.character.settings.inputAmount = value;
            __instance.energyRequested.text = __instance.character.display(value);

            return false;
        }



        // advanced training
        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "Start")]
        private static void AdvancedTrainingController_Start_postfix(AdvancedTrainingController __instance)
        {
            __instance.target.characterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
            __instance.target.characterLimit = 15;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdvancedTrainingController), "checkTargetInput")]
        private static IEnumerable<CodeInstruction> AdvancedTrainingController_checkTargetInput_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, longParse))
                .RemoveInstruction()
                .Insert(Transpilers.EmitDelegate(parseLong))
                .InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "checkTargetInput")]
        private static void AdvancedTrainingController_updateText_postfix(AdvancedTrainingController __instance)
        {
            var character = __instance.character;

            if (!character.InMenu(Menu.AdvancedTraining))
                return;

            __instance.target.text = character.display(character.advancedTraining.levelTarget[__instance.id]);
        }



        // NGUs
        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "Start")]
        private static void NGUController_Start_postfix(NGUController __instance)
        {
            __instance.target.characterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "Start")]
        private static void NGUMagicController_Start_postfix(NGUMagicController __instance)
        {
            __instance.magicTarget.characterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
        }

        [HarmonyTranspiler
            , HarmonyPatch(typeof(NGUController), "setTarget")
            , HarmonyPatch(typeof(NGUMagicController), "setTarget")]
        private static IEnumerable<CodeInstruction> NGUController_setTarget_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, longParse))
                .RemoveInstruction()
                .Insert(Transpilers.EmitDelegate(parseLong))
                .InstructionEnumeration();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUController), "updateInput")]
        private static bool NGUController_updateInput_prefix(NGUController __instance)
        {
            var character = __instance.character;
            var ngu = character.NGU.skills[__instance.id];

            var value = character.settings.nguLevelTrack switch
            {
                difficulty.normal => ngu.target,
                difficulty.evil => ngu.evilTarget,
                difficulty.sadistic => ngu.sadisticTarget,
                _ => 0
            };

            __instance.target.text = character.settings.numberDisplay switch
            {
                1 => character.display(value),
                2 => character.display(value),
                _ => value.ToString()
            };

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NGUMagicController), "updateInput")]
        private static bool NGUMagicController_updateInput_prefix(NGUMagicController __instance)
        {
            var character = __instance.character;
            var ngu = character.NGU.magicSkills[__instance.id];

            var value = character.settings.nguLevelTrack switch
            {
                difficulty.normal => ngu.target,
                difficulty.evil => ngu.evilTarget,
                difficulty.sadistic => ngu.sadisticTarget,
                _ => 0
            };

            __instance.magicTarget.text = character.settings.numberDisplay switch
            {
                1 => character.display(value),
                2 => character.display(value),
                _ => value.ToString()
            };

            return false;
        }



        // modified version of EnergyInputController.validateInput()
        // in vanilla, using a culture that uses commas as decimal seperators doesn't work as they get removed before parsing - use current culture's decimal seperator character
        // also, it can't parse 1e6, has to be 1e+6 - allow 1e6
        private static long parseLong(string text)
        {
            text = Regex.Replace(text, @"(\d)e(\d)", "$1e+$2");

            // only do the custom parsing if the text can't be parsed as-is
            // the specified NumberStyles allows parsing sci/eng notation
            if (!double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, null, out var parsed))
            {
                var suffixMulti = 1.0;

                text = text.Replace("uadrillion", "").Replace("rillion", "").Replace("illion", "");
                if (text.EndsWith("k"))
                    suffixMulti = 1000.0;
                else if (text.EndsWith("m"))
                    suffixMulti = 1000000.0;
                else if (text.EndsWith("b"))
                    suffixMulti = 1000000000.0;
                else if (text.EndsWith("t"))
                    suffixMulti = 1000000000000.0;
                else if (text.EndsWith("q"))
                    suffixMulti = 1000000000000000.0;

                var decimalSeperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                text = Regex.Replace("0" + text, $"[^0-9{decimalSeperator}]", string.Empty);

                // no clue why 4G was checking for multiples, but keeping it anyway and using current culture intead of assuming '.'
                if (text.Split(decimalSeperator[0]).Length - 1 > 1)
                {
                    int num2 = text.Length - text.LastIndexOf(decimalSeperator);
                    text = text.Replace(decimalSeperator, "");
                    text = text.Insert(text.Length + 1 - num2, decimalSeperator);
                }

                parsed = double.Parse(text) * suffixMulti;
            }

            var value = parsed >= long.MaxValue ? long.MaxValue : (long)parsed;
            return value;
        }
    }
}
