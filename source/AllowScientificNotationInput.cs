using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AllowScientificNotationInput
    {
        private static MethodInfo longParse = typeof(long).GetMethod("Parse", new[] { typeof(string) });
        private static long ParseScientificNotation(string s) => long.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands);



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
                .Insert(Transpilers.EmitDelegate(ParseScientificNotation))
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
                .Insert(Transpilers.EmitDelegate(ParseScientificNotation))
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
    }
}
