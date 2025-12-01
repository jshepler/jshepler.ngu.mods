using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    //[HarmonyPatch]
    internal class LootRolls
    {
        private static MethodInfo _getValue = typeof(UnityEngine.Random).GetProperty("value").GetMethod;

        [HarmonyTranspiler, HarmonyPatch(typeof(LootDrop), "zone30Drop")]
        private static IEnumerable<CodeInstruction> LootDrop_zone30Drop_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 342))
                .MatchBack(false, new CodeMatch(OpCodes.Call, _getValue))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(showLootRoll("BEUC")));

            return cm.InstructionEnumeration();
        }

        private static Func<float, float> showLootRoll(string item)
        {
            return (float f) =>
            {
                Plugin.Character.adventureController.log.AddEvent($"{item} roll: {f}");
                return f;
            };
        }

        //[HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if (Input.GetKeyDown(KeyCode.F4))
                    Plugin.Character.adventure.boss9Spawn.setTime(Plugin.Character.adventureController.boss9SpawnTime());
            };
        }
    }
}
