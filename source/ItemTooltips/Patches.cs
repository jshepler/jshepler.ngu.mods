using System;
using HarmonyLib;

namespace jshepler.ngu.mods.ItemTooltips
{
    [HarmonyPatch]
    internal class Patches
    {
        private static ItemTooltip tooltip = null;

        // prepends item id
        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(Equipment)])]
        private static void InventoryController_itemTooltipText_postfix(Equipment item, ref string __result)
        {
            __result = $"<b>({item.id})</b> {__result}";
        }

        // daycare
        [HarmonyPrefix, HarmonyPatch(typeof(DaycareItemController), "OnPointerEnter")]
        private static bool DaycareItemController_OnPointerEnter_prefix(DaycareItemController __instance)
        {
            var slotId = __instance.globalID();
            var item = __instance.character.inventory.GetItem(slotId);
            if (item == null || item.id == 0)
                return false;

            var messageField = Traverse.Create(__instance).Field<string>("message");
            startTooltip(slotId, __instance.updateTooltipMessage, () => messageField.Value);

            return false;
        }

        // inventory
        [HarmonyPrefix, HarmonyPatch(typeof(ItemController), "OnPointerEnter")]
        private static bool ItemController_OnPointerEnter_prefix(ItemController __instance)
        {
            var slotId = __instance.id;
            var character = __instance.character;

            // have to do this because the OnPointerEnter() method being replaced by this patch does it
            if (character.inventoryController.midDrag)
                character.inventory.item2 = slotId;

            var item = character.inventory.GetItem(slotId);
            if (item == null || item.id == 0)
                return false;

            __instance.hovered = true; // again, original OnPointerEnter() does this, so need to do it as well

            var messageField = Traverse.Create(__instance).Field<string>("message");
            startTooltip(slotId, __instance.updateTooltipMessage, () => messageField.Value, showDaycareLevel: true, showEstBoostTimes: true, showEmptySlotMessage: false);

            return false;
        }

        // loadouts

        // this fixes bug in vanilla where a loadout item is in daycare and shows empty tooltip
        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutDisplayController), "updateTooltipMessage")]
        private static bool LoadoutDisplayController_updateTooltipMessage_prefix(LoadoutDisplayController __instance, ref string ___message)
        {
            var slotId = __instance.GetInventorySlotId();
            var dcId = __instance.inventoryController.daycareID(slotId);
            if (dcId == -1)
                return true;

            var dcLevel = Plugin.Character.inventory.daycare[dcId].level + Plugin.Character.inventoryController.daycares[dcId].levelsAdded();
            ___message = __instance.inventoryController.itemTooltipText(__instance.character.inventory.daycare[dcId])
                + $"\n\n<b>Item level in Daycare:</b> {dcLevel} (this item)";

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutDisplayController), "OnPointerEnter")]
        private static bool LoadoutDisplayController_OnPointerEnter_prefix(LoadoutDisplayController __instance)
        {
            var item = __instance.GetItem();
            if (item == null || item.id == 0)
                return true;

            var slotId = __instance.GetInventorySlotId();
            var dcId = __instance.inventoryController.daycareID(slotId);
            var messageField = Traverse.Create(__instance).Field<string>("message");
            startTooltip(slotId, __instance.updateTooltipMessage, () => messageField.Value, showDaycareLevel: (dcId == -1), showEstBoostTimes: true);

            return false;
        }

        // equipped
        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutController), "OnPointerEnter")]
        private static bool LoadoutController_OnPointerEnter_prefix(LoadoutController __instance)
        {
            var slotId = __instance.id;

            // infinity cube
            if (slotId == -100)
                return true;

            if ((slotId <= -1 && slotId >= -6) || (slotId >= 10000 && slotId < 100000) || (slotId >= 1000000 && slotId < 20000000))
                __instance.hovered = true;

            var messageField = Traverse.Create(__instance).Field<string>("message");
            startTooltip(slotId, __instance.updateTooltipMessage, () => messageField.Value, showDaycareLevel: true, showEstBoostTimes: true);

            return false;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(DaycareItemController), "OnPointerExit"),
            HarmonyPatch(typeof(ItemController), "OnPointerExit"),
            HarmonyPatch(typeof(LoadoutDisplayController), "OnPointerExit"),
            HarmonyPatch(typeof(LoadoutController), "OnPointerExit")]
        private static void LoadoutController_OnPointerExit_postfix()
        {
            stopTooltip();
        }


        private static void startTooltip(
            int slotId
            , Action updateMessage
            , Func<string> getMessage
            , bool showDaycareLevel = false
            , bool showEstBoostTimes = false
            , bool showEmptySlotMessage = true)
        {
            if (tooltip != null)
                stopTooltip();

            tooltip = new ItemTooltip(slotId, updateMessage, getMessage, showDaycareLevel, showEstBoostTimes, showEmptySlotMessage);
            tooltip.Show();
        }

        private static void stopTooltip()
        {
            if (tooltip != null)
            {
                tooltip.Hide();
                tooltip = null;
            }
        }
    }
}
