using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.ItemTooltips
{
    [HarmonyPatch]
    internal class ItemListItemTooltip
    {
        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Coroutine _cor;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);

        [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "OnPointerEnter")]
        private static void ItemListController_OnPointerEnter_postfix(ItemListController __instance)
        {
            var character = __instance.character;
            var id = __instance.id;
            if (id > character.itemInfo.highestID()
                || !character.inventory.itemList.itemDropped[id])
                return;

            var tt = _tooltipText.GetValue(__instance.tooltip) as Text;
            var sources = ItemSources.BuildString(id);

            if (_cor != null)
                character.StopCoroutine(_cor);

            _cor = character.StartCoroutine(showItemListItemTooltip(tt, tt.text, sources));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "OnPointerExit")]
        private static void ItemListController_OnPointerExit_postfix()
        {
            if (_cor != null)
                Plugin.Character.StopCoroutine(_cor);

            _cor = null;
        }

        // trying something different - instead of calling Plugin.ShowTooltip(), just alter the text of the existing tooltip
        // the coroutine is soley for detecting if alt is down and append the sources text
        private static IEnumerator showItemListItemTooltip(Text tooltipText, string baseText, string sources)
        {
            while (true)
            {
                tooltipText.text = baseText + (Plugin.AltIsDown ? sources : string.Empty);
                yield return _wait;
            }
        }
    }
}
