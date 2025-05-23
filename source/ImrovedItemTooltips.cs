using HarmonyLib;
using jshepler.ngu.mods.GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static jshepler.ngu.mods.TrackBaseAdvPowerGained;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImrovedItemTooltips
    {
        private static bool _appendDaycareText = false;
        private static bool _appendDualWieldText = false;
        private static Coroutine _cor;
        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(int)])]
        private static void InventoryController_itemTooltipText_prefix(int id)
        {
            _isWeap2 = id == -6;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(Equipment)])]
        private static IEnumerable<CodeInstruction> InventoryController_itemTooltipText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var toe = typeof(Equipment);
            var curAttack = toe.GetField("curAttack");
            var curDefense = toe.GetField("curDefense");
            var spec1Cur = toe.GetField("spec1Cur");
            var spec2Cur = toe.GetField("spec2Cur");
            var spec3Cur = toe.GetField("spec3Cur");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curAttack))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, curDefense))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec1Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec2Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, spec3Cur))
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(effectiveWeap2Value));

            return cm.InstructionEnumeration();
        }

        private static bool _isWeap2 = false;
        private static float effectiveWeap2Value(float value)
        {
            var effectiveness = Plugin.Character.inventoryController.weapon2Factor();
            return _isWeap2 ? value * effectiveness : value;
        }

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

        // this fixes bug in vanilla where a loadout item is in daycare and shows empty tooltip
        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutDisplayController), "updateTooltipMessage")]
        private static bool LoadoutDisplayController_updateTooltipMessage_prefix(LoadoutDisplayController __instance, ref string ___message)
        {
            var iSlotId = __instance.GetInventorySlotId();
            var dcId = __instance.inventoryController.daycareID(iSlotId);

            _appendDaycareText = true;
            if (dcId == -1)
                return true;

            _appendDaycareText = false;
            var dcLevel = Plugin.Character.inventory.daycare[dcId].level + Plugin.Character.inventoryController.daycares[dcId].levelsAdded();

            ___message = __instance.inventoryController.itemTooltipText(__instance.character.inventory.daycare[dcId])
                + $"\n\n<b>Item level in Daycare:</b> {dcLevel} (this item)";
            return false;
        }

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
            var character = Plugin.Character;

            while (true)
            {
                updateTooltipMessage();
                var text = getTooltipMessage();

                if (_appendDaycareText)
                    text += BuildDaycareString(item);

                if (_appendDualWieldText)
                    text += $"\n\n<b>Dual-Wield Effectiveness:</b> {character.inventoryController.weapon2Factor() * 100f:0}%";

                if (item.isMacGuffin())
                {
                    var muff = character.arbitrary.macGuffinBooster1Time.totalseconds > 0.0 || character.arbitrary.macGuffinBooster1InUse;
                    text += $"\n\n<b>Time Factor:</b> x{character.inventoryController.macGuffinBonusTimeFactor()}{(muff ? " (muffin active)" : string.Empty)}";
                }

                if (item.id == 92 && item.level > 0 && character.settings.yggdrasilOn)
                    text += $"\n\n<b>Gain <color=blue>{(int)(item.level * (1f + item.level / 100f))}</color> seeds if consumed now</b>";

                if (Input.GetKey(KeyCode.LeftAlt))
                    text += BuildItemSourcesString(item) + BuildBoostETAString(item);

                character.tooltip.showOverrideTooltip(text);
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

            var controller = Plugin.Character.inventoryController.daycares[dcId];
            var dcLevel = daycare[dcId].level + controller.levelsAdded();
            var text = $"\n\n<b>Item level in Daycare:</b> {dcLevel}";

            if (item.type != part.MacGuffin)
            {
                // level + 1 to account for the extra level gained when merging
                var afterMerge = Math.Min(100, dcLevel + item.level + 1);
                var timeToMaxLevel = Math.Max(0, DaycareBarText.TimeToMaxLevel(controller, item.level + 1));

                text += $" ({afterMerge} after merge)"
                    + $"\n  ... time to 100 (merged): {NumberOutput.timeOutput(timeToMaxLevel)}";
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

        private static string BuildBoostETAString(Equipment item)
        {

            var boostsStrings = new List<string>();

            var missingPower = Mathf.Floor(item.capAttack * (1f + (float)item.level / 100f)) - item.curAttack;
            var missingToughness = Mathf.Floor(item.capDefense * (1f + (float)item.level / 100f)) - item.curDefense;
            var missingSpecial1 = Mathf.Floor(item.spec1Cap * (1f + (float)item.level / 100f)) - item.spec1Cur;
            var missingSpecial2 = Mathf.Floor(item.spec2Cap * (1f + (float)item.level / 100f)) - item.spec2Cur;
            var missingSpecial3 = Mathf.Floor(item.spec3Cap * (1f + (float)item.level / 100f)) - item.spec3Cur;

            var character = Plugin.Character;

            var completedBoostsCount = character.inventory.itemList.itemMaxxed.Skip(1).Take(39).Count(b => b);
            var completedBoostsBonus = (completedBoostsCount * .02f) + 1f;
            var bdwCompleteBonus = character.inventory.itemList.badlyDrawnComplete ? 1.2f : 1f;
            var constructionCompleteBonus = character.inventory.itemList.constructionComplete ? 1.2f : 1f;
            var perksBonus = character.adventureController.itopod.totalBoostBonus();
            var quirksBonus = character.beastQuestPerkController.totalBoostBonus();

            var totalBonus = completedBoostsBonus * bdwCompleteBonus * constructionCompleteBonus * perksBonus * quirksBonus;
            var totalBoostMissing = missingPower + missingToughness + missingSpecial1 + missingSpecial2 + missingSpecial3;

            if ( totalBoostMissing == 0) return "";

            string boostText = $"<b>P:</b> {(int)(missingPower / totalBonus)} / <b>T:</b> {(int)(missingToughness / totalBonus)} / <b>S:</b> {(int)((missingSpecial1 + missingSpecial2 + missingSpecial3) / totalBonus)}";
            boostsStrings.Add(boostText);

            //TODO: it's better to check for zones with no boost drops but this is easier.
            // Leaves out bosses altogether though!!
            if (Plugin.Character.adventureController.zone >= 1000
                || Zones.TitanZoneIds.Contains(Plugin.Character.adventureController.zone)
                || Plugin.Character.adventureController.zone == -1
                ) return $"\n\n<b>base boosts needed to cap:</b>\n{boostsStrings.Join(s => s, "\n")}"; ;

            var zoneid = Plugin.Character.adventureController.zone;
            var zone = DropTable.Zones[zoneid];
            var boostList = zone.NormalDrops.Items.Where(item => DropTable.CheckIfBoost(item.ItemIds.Cast<Items>().ToArray()).Item1);

            var expectedBoostsPerKill = EstimateBoostPerKillFromCurrentZone(boostList);

            if (expectedBoostsPerKill.normal == 0) return $"\n\n<b>base boosts needed to cap:</b>\n{boostsStrings.Join(s => s, "\n")}";
             
            var enemies = Plugin.Character.adventureController.enemyList[zoneid];
            var eCount = enemies.Count;
            var nCount = enemies.Count(e => e.enemyType == enemyType.normal);
            var bCount = enemies.Count(e => e.enemyType == enemyType.boss);
            var nRatio = (float)nCount / (float)eCount;

            float killsForCap = totalBoostMissing / totalBonus / (expectedBoostsPerKill.normal * nRatio) ;
            float recycledKillsForCap = totalBoostMissing / totalBonus / (expectedBoostsPerKill.recycled * nRatio);
            
            boostsStrings.Add($"Kills required (estimate): {killsForCap}");
            boostsStrings.Add($"... with boost recycling: {recycledKillsForCap}");

            var respawnTime = Plugin.Character.adventureController.respawnTime();
            var idleAttackSpeed = Plugin.Character.adventure.attackSpeed;
            var secondsPerKill = respawnTime + idleAttackSpeed;

            boostsStrings.Add($"Time needed (1-shot kills): {NumberOutput.timeOutput(killsForCap * secondsPerKill)}");
            boostsStrings.Add($"... with boost recycling: {NumberOutput.timeOutput(recycledKillsForCap * secondsPerKill)}");



            return $"\n\n<b>base boosts needed to cap:</b>\n{boostsStrings.Join(s => s, "\n")}";
        }

        private static (float normal,float recycled) EstimateBoostPerKillFromCurrentZone(IEnumerable<DropItems> boostList)
        {
            var character = Plugin.Character;
            var zoneid = Plugin.Character.adventureController.zone;

            float dcMultiplier = 0f;
            var rooted = zoneid >= 20;
            dcMultiplier = rooted ? character.lootFactorRooted() : character.lootFactor();


            // does it need to be added?
            //if (_shiftIsDown && !isCharmActive)
            //   _dcMulti *= rooted ? rootedCharmMulti : 2f;
            float totalWeightedBoost = 0f;
            int boostCount = 0;

            foreach (var boost in boostList)
            {
                float moddedDC = boost.BaseDC * dcMultiplier + boost.BonuseDC;
                float dc = Math.Min(moddedDC, boost.MaxDC);

                var (isBoost, boostValue) = DropTable.CheckIfBoost(boost.ItemIds.Cast<Items>().ToArray());

                if (isBoost)
                {
                    totalWeightedBoost += boostValue * dc;
                    boostCount++;
                }
            }

            float expectedBoostPerKill = boostCount > 0 ? totalWeightedBoost : 0f;

            totalWeightedBoost = 0f;
            boostCount = 0;


            foreach (var boost in boostList)
            {
                float moddedDC = boost.BaseDC * dcMultiplier + boost.BonuseDC;
                float dc = Math.Min(moddedDC, boost.MaxDC);

                var (isBoost, boostValue) = DropTable.CheckIfBoost(boost.ItemIds.Cast<Items>().ToArray(), true);

                if (isBoost)
                {
                    totalWeightedBoost += boostValue * dc;
                    boostCount++;
                }
            }

            float recycledExpectedBoostPerKill = boostCount > 0 ? totalWeightedBoost : 0f;

            return (expectedBoostPerKill, recycledExpectedBoostPerKill);
        }

    }
}
