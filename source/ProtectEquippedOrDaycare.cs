using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ProtectEquippedOrDaycare
    {
        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutController), "OnPointerClick")]
        private static bool LoadoutController_OnPointerClick_prefix(PointerEventData eventData, LoadoutController __instance)
        {
            if (__instance.id == -100
                || eventData.button == PointerEventData.InputButton.Right
                || (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)))
                return true;

            var item = __instance.character.inventory.GetItem(__instance.id);
            item.removable = !item.removable;
            __instance.updateItem();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(DaycareItemController), "OnPointerClick")]
        private static bool DaycareItemController_OnPointerClick_prefix(PointerEventData eventData, DaycareItemController __instance)
        {
            if (eventData.button == PointerEventData.InputButton.Right
                || (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)))
                return true;

            var item = __instance.character.inventory.daycare[__instance.id];
            item.removable = !item.removable;
            __instance.updateItem();

            return false;
        }
    }
}
