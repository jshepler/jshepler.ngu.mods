using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InfinityCubeSoftCap
    {
        internal static float CubeBoostDivider
        {
            get
            {
                var character = Plugin.Character;
                if (character == null)
                    return 1f;

                return (character.adventure.itopod.perkLevel[26] >= 1 ? 50f : 100f)
                    / character.wishesController.totalBoostRatioDivider();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutController), "OnPointerEnter")]
        private static bool LoadoutController_OnPointerEnter_prefix(LoadoutController __instance)
        {
            if (__instance.id != -100)
                return true;

            StartShowTooltip(__instance);
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutController), "OnPointerExit")]
        private static void LoadoutController_OnPointerExit_postfix()
        {
            StopShowTooltip();
        }

        private static Coroutine _cor;
        private static void StartShowTooltip(LoadoutController controller)
        {
            StopShowTooltip();
            _cor = Plugin.Character.StartCoroutine(ShowTooltip(controller));
        }

        private static void StopShowTooltip()
        {
            if (_cor != null)
                Plugin.Character.StopCoroutine(_cor);

            _cor = null;
        }

        private static WaitForSeconds _waiter = new WaitForSeconds(0.1f);
        private static FieldInfo _message = typeof(LoadoutController).GetField("message", BindingFlags.NonPublic | BindingFlags.Instance);
        private static IEnumerator ShowTooltip(LoadoutController controller)
        {
            while (true)
            {
                controller.infinityCubeTooltip();

                var message = _message.GetValue(controller) as string;
                Plugin.Character.tooltip.showOverrideTooltip(message);

                yield return _waiter;
            }
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(LoadoutController), "infinityCubeTooltip")]
        private static IEnumerable<CodeInstruction> LoadoutController_infinityCubeTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var powerString = "<b>Power:</b> ";
            var toughString = "\n<b>Toughness:</b> ";

            var inventoryCubePower = typeof(Inventory).GetField("cubePower");
            var concat2strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]);
            var concat3strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string)]);

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
                .Insert(Transpilers.EmitDelegate(Additionalnfo));

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
            var color = cubePower >= softcap ? "green" : "blue";
            var text = $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)} (sc)";

            if (cubePower < softcap)
            {
                var gainedLastRB = TrackCubeBoosts.PowerGainedLastRebirth;
                var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : gainedLastRB / (secondsLastRB / 60.0);
                var estDays = gainPerMin == 0 ? 0 : ((softcap - cubePower) / (gainPerMin * 1440.0));

                text += $"\n<b>   ... est. days to SC:</b> {(estDays == 0 ? "n/a" : character.display(estDays))}";
            }

            else
                text += $"\n<b>   ... (uncapped):</b> {character.display(cubePower)}";

            return text;
        }

        private static string CubeToughnessWithSoftcap()
        {
            var character = Plugin.Character;
            var cubeToughness = character.inventory.cubeToughness;
            var softcap = character.inventoryController.cubeToughnessSoftcap();
            var capped = cubeToughness <= softcap ? cubeToughness : softcap + Mathf.Pow(cubeToughness - softcap, 0.5f);
            var color = cubeToughness >= softcap ? "green" : "blue";
            var text = $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)} (sc)";

            if (cubeToughness < softcap)
            {
                var gainedLastRB = TrackCubeBoosts.ToughnessGainedLastRebirth;
                var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : gainedLastRB / (secondsLastRB / 60.0);
                var estDays = gainPerMin == 0 ? 0 : ((softcap - cubeToughness) / (gainPerMin * 1440.0));

                text += $"\n<b>   ... est. days to SC:</b> {(estDays == 0 ? "n/a" : character.display(estDays))}";
            }

            else
                text += $"\n<b>   ... (uncapped):</b> {character.display(cubeToughness)}";

            return text;
        }

        private static string Additionalnfo()
        {
            var character = Plugin.Character;
            var cubePower = character.inventory.cubePower;
            var cubeToughness = character.inventory.cubeToughness;
            var total = (long)(cubePower + cubeToughness);
            var nextTier = total < 10 ? 1 : (int)Mathf.Log10(total);

            var powLastRB = TrackCubeBoosts.PowerGainedLastRebirth;
            var toughLastRB = TrackCubeBoosts.ToughnessGainedLastRebirth;
            var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;

            var text = "\n\n<color=blue><b>AT MAX TIER</b></color>";
            if (nextTier <= 10)
            {
                var totalLastRB = powLastRB + toughLastRB;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : totalLastRB / (secondsLastRB / 60.0);

                var need = Mathf.Pow(10, nextTier + 1) - total;
                var estDays = gainPerMin == 0 ? 0 : (need / (gainPerMin * 1440.0));

                text = $"\n\n<b>P + T (uncapped):</b> {character.display(total)}"
                        + $"\n<b>Need for next tier:</b> {character.display(need)}"
                        + $"\n<b>   ... est. days:</b> {(estDays == 0 ? "n/a" : character.display(estDays))}";
            }

            var powThisRB = TrackCubeBoosts.PowerGainedThisRebirth;
            var toughThisRB = TrackCubeBoosts.ToughnessGainedThisRebirth;
            text += $"\n\n<b>Power gained this RB:</b> {character.display(powThisRB)}"
                + $"\n<b>Power gained last RB:</b> {character.display(powLastRB)}"
                + $"\n<b>Toughness gained this RB:</b> {character.display(toughThisRB)}"
                + $"\n<b>Toughness gained last RB:</b> {character.display(toughLastRB)}";
                //+ $"\n<b>Last Rebirth Time:</b> {NumberOutput.timeOutput(secondsLastRB)}";

            text += $"\n\n<b>Boost Divider:</b> {CubeBoostDivider:0.0#}";

            return text;
        }
    }
}
