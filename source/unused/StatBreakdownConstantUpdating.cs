using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StatBreakdownConstantUpdating
    {
        private static StatsDisplay _controller;
        private static MethodInfo _scrollValue = typeof(Scrollbar).GetProperty("value").GetSetMethod();
        private static string _lastMethod = string.Empty;

        [HarmonyTranspiler,
            HarmonyPatch(typeof(StatsDisplay), "displayAdventure"),
            HarmonyPatch(typeof(StatsDisplay), "displayAttackDefense"),
            HarmonyPatch(typeof(StatsDisplay), "displayAugments"),
            HarmonyPatch(typeof(StatsDisplay), "displayBeards"),
            HarmonyPatch(typeof(StatsDisplay), "displayEnergy"),
            HarmonyPatch(typeof(StatsDisplay), "displayEXPGain"),
            HarmonyPatch(typeof(StatsDisplay), "displayMagic"),
            HarmonyPatch(typeof(StatsDisplay), "displayMisc"),
            HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure"),
            HarmonyPatch(typeof(StatsDisplay), "displayNGU"),
            HarmonyPatch(typeof(StatsDisplay), "displayRes3"),
            HarmonyPatch(typeof(StatsDisplay), "displayWandoos")]
        // removes the line of code that resets scroller, otherwise it would reset every refresh interval below
        private static IEnumerable<CodeInstruction> StatsDisplay_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __original)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _scrollValue))
                .Advance(-3)
                .RemoveInstructions(4);

            return cm.InstructionEnumeration();
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(StatsDisplay), "displayAdventure"),
            HarmonyPatch(typeof(StatsDisplay), "displayAttackDefense"),
            HarmonyPatch(typeof(StatsDisplay), "displayAugments"),
            HarmonyPatch(typeof(StatsDisplay), "displayBeards"),
            HarmonyPatch(typeof(StatsDisplay), "displayEnergy"),
            HarmonyPatch(typeof(StatsDisplay), "displayEXPGain"),
            HarmonyPatch(typeof(StatsDisplay), "displayMagic"),
            HarmonyPatch(typeof(StatsDisplay), "displayMisc"),
            HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure"),
            HarmonyPatch(typeof(StatsDisplay), "displayNGU"),
            HarmonyPatch(typeof(StatsDisplay), "displayRes3"),
            HarmonyPatch(typeof(StatsDisplay), "displayWandoos")]
        private static void StatsDisplay_prefix(StatsDisplay __instance, MethodBase __originalMethod)
        {
            var method = __originalMethod.Name;
            if (method != _lastMethod)
            {
                _lastMethod = method;
                __instance.scrollbar.value = 1f;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StatsDisplay), "Start")]
        private static void StatsDisplay_Start_postfix(StatsDisplay __instance)
        {
            _controller = __instance;
            __instance.character.StartCoroutine(RefreshDisplay());
        }

        private static WaitForSeconds _wait = new WaitForSeconds(0.5f);
        private static IEnumerator RefreshDisplay()
        {
            while (true)
            {
                yield return _wait;

                if (!Plugin.Character.InMenu(Menu.StatBreakdowns))
                    continue;

                switch (_lastMethod)
                {
                    case "displayAdventure":
                        _controller.displayAdventure();
                        break;

                    case "displayAttackDefense":
                        _controller.displayAttackDefense();
                        break;

                    case "displayAugments":
                        _controller.displayAugments();
                        break;

                    case "displayBeards":
                        _controller.displayBeards();
                        break;

                    case "displayEnergy":
                        _controller.displayEnergy();
                        break;

                    case "displayEXPGain":
                        _controller.displayEXPGain();
                        break;

                    case "displayMagic":
                        _controller.displayMagic();
                        break;

                    case "displayMisc":
                        _controller.displayMisc();
                        break;

                    case "displayMiscAdventure":
                        _controller.displayMiscAdventure();
                        break;

                    case "displayNGU":
                        _controller.displayNGU();
                        break;

                    case "displayRes3":
                        _controller.displayRes3();
                        break;

                    case "displayWandoos":
                        _controller.displayWandoos();
                        break;
                }
            }
        }
    }
}
