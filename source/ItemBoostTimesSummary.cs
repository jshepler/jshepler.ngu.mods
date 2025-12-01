using System.Collections;
using HarmonyLib;
using jshepler.ngu.mods.ItemTooltips;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ItemBoostTimesSummary
    {
        private static string _baseMessage = "<b>Keyboard Shortcuts:\n\nA+Click item: Use all possible boosts on this item.\nD+Click item: Merge all possible copies onto this item.\nCTRL+Click item: Trash/consumes/transforms item based on context.\nSHIFT+Click item: Protect item from trashing or transforming.\nRight Click Item: Quick-equip.</b>";
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static Coroutine _cor;

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "quickShortcutsTooltip")]
        private static void InventoryController_quickShortcutsTooltip_postfix(InventoryController __instance)
        {
            if (_cor != null)
                Plugin.EndCoroutine(_cor);

            var tt = Traverse.Create(__instance.tooltip).Field<Text>("tooltipText").Value;
            _cor = Plugin.BeginCoroutine(updateTooltip(tt));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "hideTooltip")]
        private static void InventoryController_hideTooltip_postfix()
        {
            if (_cor != null)
            {
                Plugin.EndCoroutine(_cor);
                _cor = null;
            }
        }

        private static IEnumerator updateTooltip(Text tt)
        {
            while (true)
            {
                tt.text = Plugin.AltIsDown ? Boosts.BuildEstBoostTimeSummary() : _baseMessage;
                yield return _wait;
            }
        }
    }
}
