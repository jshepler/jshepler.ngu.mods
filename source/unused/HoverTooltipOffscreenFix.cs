using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HoverTooltipOffscreenFix
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(HoverTooltip), "UpdateTimerUI")]
        private static IEnumerable<CodeInstruction> HoverTooltip_UpdateTimerUI_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var tooltipRect = typeof(HoverTooltip).GetField("tooltipRect", BindingFlags.NonPublic);
            var canvas = typeof(HoverTooltip).GetField("canvas");

            return new CodeMatcher(instructions)
                .End()
                .MatchBack(false
                    , new CodeMatch(OpCodes.Ldarg_0)
                    , new CodeMatch(OpCodes.Ldarg_0)
                    , new CodeMatch(OpCodes.Ldfld, tooltipRect))

                .Advance(-1)
                .SetOpcodeAndAdvance(OpCodes.Blt_Un_S)

                .Advance(3)
                .RemoveInstructions(4)

                .Advance(2)
                .RemoveInstructions(3)

                .Advance(1)
                .InsertAndAdvance(CodeInstruction.LoadField(typeof(HoverTooltip), "y"))

                .RemoveInstructions(5)
                .Insert(Transpilers.EmitDelegate(CalcModY))
                .InstructionEnumeration();
                //.DumpToLog();
        }

        private static float CalcModY(RectTransform tooltipRect, Canvas canvas, float y)
        {
            return (Screen.height - tooltipRect.rect.height * canvas.scaleFactor) - y;
        }
    }
}
/*
	// mody = 0f - (tooltipRect.rect.height * canvas.scaleFactor + 10f * canvas.scaleFactor);
	IL_011a: ldarg.0
	IL_011b: ldarg.0
	IL_011c: ldfld class [UnityEngine.CoreModule]UnityEngine.RectTransform HoverTooltip::tooltipRect

	IL_0121: callvirt instance valuetype [UnityEngine.CoreModule]UnityEngine.Rect [UnityEngine.CoreModule]UnityEngine.RectTransform::get_rect()
	IL_0126: stloc.0
	IL_0127: ldloca.s 0
	IL_0129: call instance float32 [UnityEngine.CoreModule]UnityEngine.Rect::get_height()
	
    IL_012e: ldarg.0
	IL_012f: ldfld class [UnityEngine.UIModule]UnityEngine.Canvas HoverTooltip::canvas

	IL_0134: callvirt instance float32 [UnityEngine.UIModule]UnityEngine.Canvas::get_scaleFactor()
	IL_0139: mul
	IL_013a: ldc.r4 10

	IL_013f: ldarg.0

	IL_0140: ldfld class [UnityEngine.UIModule]UnityEngine.Canvas HoverTooltip::canvas
	IL_0145: callvirt instance float32 [UnityEngine.UIModule]UnityEngine.Canvas::get_scaleFactor()
	IL_014a: mul
	IL_014b: add
	IL_014c: neg

	IL_014d: stfld float32 HoverTooltip::mody
 */