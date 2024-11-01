using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImrovedItemTooltips
    {
        private static bool _appendDaycareText = false;
        private static bool _appendDualWieldText = false;
        private static Coroutine _cor;
        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

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
            var item = Plugin.Character.inventory.daycare[__instance.id];
            if (item != null && item.id != 0)
            {
                _appendDaycareText = false;
                var messageField = Traverse.Create(__instance).Field<string>("message");
                StartShowTooltip(item, __instance.updateTooltipMessage, () => messageField.Value);
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DaycareItemController), "OnPointerExit")]
        private static void DaycareItemController_OnPointerExit_postfix()
        {
            StopShowTooltip();
        }

        // inventory
        [HarmonyPrefix, HarmonyPatch(typeof(ItemController), "OnPointerEnter")]
        private static bool ItemController_OnPointerEnter_prefix(ItemController __instance)
        {
            var id = __instance.id;
            var character = __instance.character;

            if (character.inventoryController.midDrag)
            {
                character.inventory.item2 = id;
            }

            var item = GetItemFromSlotId(id);
            if (item != null && item.id != 0)
            {
                __instance.hovered = true;
                _appendDaycareText = true;

                var messageField = Traverse.Create(__instance).Field<string>("message");
                StartShowTooltip(item, __instance.updateTooltipMessage, () => messageField.Value);
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "OnPointerExit")]
        private static void ItemController_OnPointerExit_postfix()
        {
            StopShowTooltip();
        }

        // loadouts
        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutDisplayController), "OnPointerEnter")]
        private static bool LoadoutDisplayController_OnPointerEnter_prefix(LoadoutDisplayController __instance)
        {
            var item = __instance.GetItem();
            if (item == null)
                return true;

            var messageField = Traverse.Create(__instance).Field<string>("message");
            StartShowTooltip(item, __instance.updateTooltipMessage, () => messageField.Value);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutDisplayController), "OnPointerExit")]
        private static void LoadoutDisplayController_OnPointerExit_postfix()
        {
            StopShowTooltip();
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

            _appendDualWieldText = slotId == -6;
            _appendDaycareText = true;

            var item = GetItemFromSlotId(slotId);
            var messageField = Traverse.Create(__instance).Field<string>("message");
            StartShowTooltip(item, __instance.updateTooltipMessage, () => messageField.Value);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutController), "OnPointerExit")]
        private static void LoadoutController_OnPointerExit_postfix()
        {
            StopShowTooltip();
            _appendDualWieldText = false;
        }

        // item list
        [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "OnPointerEnter")]
        private static void ItemListController_OnPointerEnter_postfix(ItemListController __instance)
        {
            var character = __instance.character;
            var id = __instance.id;
            if (id > character.itemInfo.highestID()
                || !character.inventory.itemList.itemDropped[id])
                return;

            var tt = _tooltipText.GetValue(__instance.tooltip) as Text;
            var sources = BuildItemSourcesString(id);

            if (_cor != null)
                character.StopCoroutine(_cor);

            _cor = character.StartCoroutine(ShowItemListItemTooltip(tt, tt.text, sources));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemListController), "OnPointerExit")]
        private static void ItemListController_OnPointerExit_postfix()
        {
            StopShowTooltip();
        }

        private static void StartShowTooltip(Equipment item, Action updateTooltipMessage, Func<string> getTooltipMessage)
        {
            StopShowTooltip();
            _cor = Plugin.Character.StartCoroutine(ShowTooltip(item, updateTooltipMessage, getTooltipMessage));
        }

        private static void StopShowTooltip()
        {
            if (_cor != null)
                Plugin.Character.StopCoroutine(_cor);

            _cor = null;
        }

        private static WaitForSeconds _waiter = new WaitForSeconds(0.1f);
        private static IEnumerator ShowTooltip(Equipment item, Action updateTooltipMessage, Func<string> getTooltipMessage)
        {
            while (true)
            {
                updateTooltipMessage();
                var text = getTooltipMessage();

                if (_appendDaycareText)
                    text += BuildDaycareString(item);

                if (_appendDualWieldText)
                    text += $"\n\n<b>Dual-Wield Effectiveness:</b> {Plugin.Character.inventoryController.weapon2Factor() * 100f:0}%";

                if (Input.GetKey(KeyCode.LeftAlt))
                    text += BuildItemSourcesString(item);

                Plugin.Character.tooltip.showOverrideTooltip(text);
                yield return _waiter;
            }
        }

        private static IEnumerator ShowItemListItemTooltip(Text tooltipText, string baseText, string sources)
        {
            while (true)
            {
                var isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                tooltipText.text = baseText + (isAlt ? sources : string.Empty);
                yield return _waiter;
            }
        }

        private static string BuildDaycareString(Equipment item)
        {
            var daycare = Plugin.Character.inventory.daycare;
            var dcId = -1;

            for (var x = 0; x < daycare.Count; x++)
                if (daycare[x].id == item.id)
                    dcId = x;

            if (dcId == -1)
                return null;

            var dcLevel = daycare[dcId].level + Plugin.Character.inventoryController.daycares[dcId].levelsAdded();
            var text = $"\n\n<b>Item level in Daycare:</b> {dcLevel}";

            if (item.type != part.MacGuffin)
            {
                var afterMerge = Math.Min(100, dcLevel + item.level + 1);
                text += $" ({afterMerge} after merge)";
            }

            return text;
        }

        private static string BuildItemSourcesString(Equipment item)
        {
            return BuildItemSourcesString(item.id);
        }

        private static string BuildItemSourcesString(int itemId)
        {
            var sources = new List<string>();

            for (var zoneId = 0; zoneId < DropTable.Zones.Count; zoneId++)
            {
                var zone = DropTable.Zones[zoneId];
                var zName = Plugin.Character.adventureController.zoneName(zoneId);

                if (zone.NormalDrops != null && zone.NormalDrops.Items.Any(di => di.ItemIds.Contains(itemId)))
                    sources.Add($"<b>{zName}:</b> normal drops");

                if (zone.BossDrops != null && zone.BossDrops.Items.Any(di => di.ItemIds.Contains(itemId)))
                    sources.Add($"<b>{zName}:</b> boss drops");

                if (zone.TitanV1Drops != null)
                {
                    if (zone.TitanV2Drops == null && zone.TitanV1Drops.Items.Any(di => di.ItemIds.Contains(itemId)))
                        sources.Add($"<b>{zName}:</b> titan drops");

                    else if (zone.TitanV2Drops != null)
                    {
                        if (zone.TitanV1Drops.Items.Any(di => di.ItemIds.Contains(itemId)))
                            sources.Add($"<b>{zName}:</b> V1 drops");

                        if (zone.TitanV2Drops.Items.Any(di => di.ItemIds.Contains(itemId)))
                            sources.Add($"<b>{zName}:</b> V2 drops");

                        if (zone.TitanV3Drops.Items.Any(di => di.ItemIds.Contains(itemId)))
                            sources.Add($"<b>{zName}:</b> V3 drops");

                        if (zone.TitanV4Drops.Items.Any(di => di.ItemIds.Contains(itemId)))
                            sources.Add($"<b>{zName}:</b> V4 drops");
                    }
                }

                if (zone.EnemyDrops != null)
                    foreach (var enemy in zone.EnemyDrops)
                        if (enemy.Items.Any(di => di.ItemIds.Contains(itemId)))
                            sources.Add($"<b>{zName}:</b> {Plugin.Character.adventureController.fetchEnemyNamebySpriteID(enemy.EnemyId)}");

                if (zone.MacGuffinDrop != null && (int)zone.MacGuffinDrop.MacGuffinItem == itemId)
                    sources.Add($"<b>{zName}:</b> MacGuffin");

                if (zone.QuestItemDrop != null && (int)zone.QuestItemDrop.QuestItem == itemId)
                    sources.Add($"<b>{zName}:</b> Quest Item");
            }

            return $"\n\n<b>source(s):</b>\n{sources.Join(s => s, "\n")}";
        }

        private static Equipment GetItemFromSlotId(int slotId)
        {
            var inventory = Plugin.Character.inventory;

            if (slotId >= 1000000 && slotId < 2000000)
                return inventory.macguffins[slotId - 1000000];

            if (slotId >= 10000 && slotId < 100000)
                return inventory.accs[slotId - 10000];

            return slotId switch
            {
                -1 => inventory.head,
                -2 => inventory.chest,
                -3 => inventory.legs,
                -4 => inventory.boots,
                -5 => inventory.weapon,
                -6 => inventory.weapon2,
                -69 => inventory.trash,
                _ => inventory.inventory[slotId]
            };
        }
    }
}
