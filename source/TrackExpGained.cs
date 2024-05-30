using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackExpGained
    {
        private static long _expLastRB
        {
            get => ModSave.Data.ExpGainedLastRB;
            set => ModSave.Data.ExpGainedLastRB = value;
        }

        private static long _expThisRB
        {
            get => ModSave.Data.ExpGainedThisRB;
            set => ModSave.Data.ExpGainedThisRB = value;
        }

        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix, HarmonyPatch(typeof(SpendExpTooltipTrigger), "OnPointerEnter")]
        private static void SpendExpTooltipTrigger_OnPointerEnter_postfix(SpendExpTooltipTrigger __instance)
        {
            var character = __instance.character;
            var tooltipText = _tooltipText.GetValue(__instance.tooltip) as Text;

            tooltipText.text += $"\n\nEXP gained this rebirth: {character.display(_expThisRB)}\nEXP gained last rebirth: {character.display(_expLastRB)}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            _expLastRB = _expThisRB;
            _expThisRB = 0L;
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(Character), "addExp", [typeof(long)]),
            HarmonyPatch(typeof(Character), "addExp", [typeof(float)])]
        private static IEnumerable<CodeInstruction> Character_addExp_long_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var realExp = typeof(Character).GetField("realExp");

            var cm = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, realExp),
                    new CodeMatch(OpCodes.Ldloc_1))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(addExp));

            return cm.InstructionEnumeration();
        }

        private static long addExp(long exp)
        {
            _expThisRB += exp;
            return exp;
        }
    }
}

/*

basically changes code from:
	if (!((double)realExp + (double)num2 >= 9.2233720368547758E+18))
	{
		realExp += num2;
		stats.totalExp += num2;
	}

to something like (but not exactly like):
	if (!((double)realExp + (double)num2 >= 9.2233720368547758E+18))
	{
		realExp += num2;
        addExp(num2);
		stats.totalExp += num2;
	}


changes IL from:
	// realExp += num2;
	IL_00e0: ldarg.0
	IL_00e1: ldarg.0
	IL_00e2: ldfld int64 Character::realExp
	IL_00e7: ldloc.1
	IL_00e8: add
	IL_00e9: stfld int64 Character::realExp

to:
	// realExp += num2;
	IL_00e0: ldarg.0
	IL_00e1: ldarg.0
	IL_00e2: ldfld int64 Character::realExp
	IL_00e7: ldloc.1
            call addExp(long)
	IL_00e8: add
	IL_00e9: stfld int64 Character::realExp


addExp takes a long and retuns the same value passed, so the add instruction will still work as expected,
but there is no C# code that would compile to this since I'm inserting the call between setting up the args
for the add instruction and the add instruction itself

 */