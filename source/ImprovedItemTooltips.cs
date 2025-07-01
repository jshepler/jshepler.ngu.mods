using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedItemTooltips
    {
        private static bool _appendDaycareText = false;
        private static bool _appendDualWieldText = false;
        private static bool _appendEstBoostTime = false;

        private static Coroutine _cor;
        private static FieldInfo _tooltipText = typeof(HoverTooltip).GetField("tooltipText", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(int)])]
        private static void InventoryController_itemTooltipText_prefix(int id)
        {
            _isWeap2 = id == -6;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "itemTooltipText", [typeof(int)])]
        private static void InventoryController_itemTooltipText_postfix(int id)
        {
            _isWeap2 = false;
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
                _appendEstBoostTime = false;
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

            var item = getItemFromSlotId(id);
            if (item != null && item.id != 0)
            {
                __instance.hovered = true;
                _appendDaycareText = true;
                _appendEstBoostTime = true;

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
            _appendEstBoostTime = false;
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
            _appendEstBoostTime = true;

            var item = getItemFromSlotId(slotId);
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
            var sources = buildItemSourcesString(id);

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

                if (Plugin.AltIsDown && _appendEstBoostTime && item.isEquipment())
                    text += buildEstBoostTime(item);

                if (_appendDaycareText)
                    text += buildDaycareString(item);

                if (_appendDualWieldText)
                    text += $"\n\n<b>Dual-Wield Effectiveness:</b> {character.inventoryController.weapon2Factor() * 100f:0}%";

                if (item.isMacGuffin())
                {
                    var muff = character.arbitrary.macGuffinBooster1Time.totalseconds > 0.0 || character.arbitrary.macGuffinBooster1InUse;
                    text += $"\n\n<b>Time Factor:</b> x{character.inventoryController.macGuffinBonusTimeFactor()}{(muff ? " (muffin active)" : string.Empty)}";
                }

                if (item.isBoost())
                    text += buildBoostValuesString(item);

                if (item.id == 92 && item.level > 0 && character.settings.yggdrasilOn)
                    text += $"\n\n<b>Gain <color=blue>{(int)(item.level * (1f + item.level / 100f))}</color> seeds if consumed now</b>";

                if (Input.GetKey(KeyCode.LeftAlt))
                    text += buildItemSourcesString(item);

                character.tooltip.showOverrideTooltip(text);
                yield return _waiter;
            }
        }

        private static IEnumerator ShowItemListItemTooltip(Text tooltipText, string baseText, string sources)
        {
            while (true)
            {
                tooltipText.text = baseText + (Plugin.AltIsDown ? sources : string.Empty);
                yield return _waiter;
            }
        }

        private static string buildDaycareString(Equipment item)
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

        private static string buildItemSourcesString(Equipment item)
        {
            return buildItemSourcesString(item.id);
        }

        private static string buildItemSourcesString(int itemId)
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

        private static List<int> _boosts = [1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000];
        private static float getAverageRecycledBoost(int startIndex, float boostBonus, float recycleChance)
        {
            var totalBoost = 0f;
            var probability = 1f;

            for (var x = startIndex; x >= 0; x--)
            {
                totalBoost += probability * _boosts[x] * boostBonus;
                probability *= recycleChance;
            }

            return totalBoost;
        }

        private static string buildBoostValuesString(Equipment item)
        {
            var boostIndex = (item.id - 1) % 13;
            var boostBonus = Plugin.Character.allItemList.boostBonus();
            var boostValue = _boosts[boostIndex] * boostBonus;
            var cubeBoost = boostValue / InfinityCubeSoftCap.CubeBoostDivider;
            var text = $"\n     <b>To Cube:</b> {cubeBoost:#,##0.##}";

            // totalRecycleBonus() is bugged and counts more than challenge completions and could return > 100%
            // need to cap it at 100% since it's being used in probability math
            // but need to keep using this method as it's what the game uses when doing recycling
            var recycleChance = Math.Min(1f, Plugin.Character.totalRecycleBonus());
            if (recycleChance > 0)
            {
                var avgBoostWithRecycling = getAverageRecycledBoost(boostIndex, boostBonus, recycleChance);
                var avgCubeBoostWithRecycling = avgBoostWithRecycling / InfinityCubeSoftCap.CubeBoostDivider;

                var avgTag = (recycleChance > 0 && recycleChance < 1) ? " (avg)" : string.Empty;
                text += $"\n\n<b> ... with Boost Recycling ({recycleChance * 100f:0.#}%):</b> {avgBoostWithRecycling:#,##0.##}{avgTag}"
                    + $"\n     <b>To Cube:</b> {avgCubeBoostWithRecycling:#,##0.##}{avgTag}";
            }

            return text;
        }

        private static Equipment getItemFromSlotId(int slotId)
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

        private static string buildEstBoostTime(Equipment item)
        {
            var levelMulti = 1f + item.level / 100f;
            var missingPower = Mathf.Floor(item.capAttack * levelMulti) - item.curAttack;
            var missingToughness = Mathf.Floor(item.capDefense * levelMulti) - item.curDefense;
            var missingSpecial1 = Mathf.Floor(item.spec1Cap * levelMulti) - item.spec1Cur;
            var missingSpecial2 = Mathf.Floor(item.spec2Cap * levelMulti) - item.spec2Cur;
            var missingSpecial3 = Mathf.Floor(item.spec3Cap * levelMulti) - item.spec3Cur;
            var totalBoostMissing = missingPower + missingToughness + missingSpecial1 + missingSpecial2 + missingSpecial3;
            if (totalBoostMissing <= 0)
                return string.Empty;

            var character = Plugin.Character;
            var text = $"\n\n<b>Total Boosts Remaining:</b> {character.display(totalBoostMissing)}";

            var zoneId = character.adventureController.zone;
            if (zoneId == -1 || Zones.TitanZoneIds.Contains(zoneId))
                return text;

            var expectedBoostsPerKill = getAverageBoostPerKillFromZone(zoneId, out var charmed);
            var color = charmed ? "blue" : "black";
            text += $"\n<b>Avg. Boosts per Kill:</b> <color={color}>{expectedBoostsPerKill:#,##0.#}</color>";

            if (expectedBoostsPerKill == 0f)
                return text;

            var ohKillsRemaining = totalBoostMissing / expectedBoostsPerKill;
            var respawnTime = character.adventureController.respawnTime();
            var idleAttackSpeed = character.adventure.attackSpeed;
            var secondsPerKill = respawnTime + idleAttackSpeed;
            var secondsRemaining = ohKillsRemaining * secondsPerKill;
            text += $"\n<b>Est. Time Remaining (OHK):</b> <color={color}>{NumberOutput.timeOutput(secondsRemaining)}</color>";

            return text;
        }

        private static List<int> _boostFloors = [0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 700, 850, 1150, int.MaxValue];
        private static List<DropItems> _itopodDrops = DropTable.BoostItems.Select(items => new DropItems(0.14f, 0.14f, items)).ToList();

        private static float getAverageBoostPerKillFromZone(int zoneId, out bool charmed)
        {
            var character = Plugin.Character;

            if (zoneId >= 1000) //itopod
            {
                var currentFloor = character.adventureController.itopodLevel;
                var itopodBoostIndex = _boostFloors.FindLastIndex(i => currentFloor >= i);
                var boostDrops = _itopodDrops[itopodBoostIndex];

                return getAverageBoostPerKillFromDropItems([boostDrops], out charmed);
            }

            else //non itopod
            {
                var zone = DropTable.Zones[zoneId];
                var boostDrops = zone.NormalDrops.Items.Where(item => item.ItemIds[0] > 0 && item.ItemIds[0] < 40);
                var avgBoostPerKill = getAverageBoostPerKillFromDropItems(boostDrops, out charmed);

                var enemies = character.adventureController.enemyList[zoneId];
                float nCount = enemies.Count(e => e.enemyType == enemyType.normal);
                var nRatio = nCount / enemies.Count;

                return avgBoostPerKill * nRatio;
            }
        }

        private static float getAverageBoostPerKillFromDropItems(IEnumerable<DropItems> boostDrops, out bool charmed)
        {
            var character = Plugin.Character;
            var zoneId = character.adventureController.zone;
            var rooted = zoneId >= 20;
            var playerDcMulti = getPlayerDCMulti(rooted, out charmed);

            var totalWeightedBoost = 0f;
            var boostCount = 0;

            foreach (var drop in boostDrops)
            {
                // if any of the boosts are filtered, the number of potential drops is reduced and needs to be accounted for
                var unfilteredBoostsMulti = drop.ItemIds.Count(boostUnfiltered) / (float)drop.ItemIds.Length;
                var moddedDC = drop.BaseDC * playerDcMulti + drop.BonuseDC;
                var dc = Math.Min(moddedDC, drop.MaxDC) * unfilteredBoostsMulti;

                var boostIndex = (drop.ItemIds[0] - 1) % 13;
                var boostBonus = character.allItemList.boostBonus();
                var recycleChance = character.totalRecycleBonus();
                var avgBoost = dc * getAverageRecycledBoost(boostIndex, boostBonus, recycleChance);
                if (avgBoost > 0)
                {
                    totalWeightedBoost += avgBoost;
                    boostCount++;
                }
            }

            var avgBoostPerKill = boostCount > 0 ? totalWeightedBoost : 0f;
            return avgBoostPerKill;
        }

        private static float _rootedCharm = Mathf.Pow(2f, 1f / 3f);
        private static float _rootedCharmWithBlueHeart = Mathf.Pow(2.2f, 1f / 3f);

        private static float getPlayerDCMulti(bool rooted, out bool charmed)
        {
            charmed = false;

            var character = Plugin.Character;
            var isCharmActive = character.arbitrary.lootcharm1Time.totalseconds > 0.0;

            var dcMulti = rooted ? character.lootFactorRooted() : character.lootFactor();
            if (!Plugin.ShiftIsDown && character.arbitrary.lootcharm1Time.totalseconds == 0.0)
                return dcMulti;

            var blueHeartComplete = character.inventory.itemList.blueHeartComplete;
            var charmMulti = 1f;

            if (rooted)
                charmMulti = blueHeartComplete ? _rootedCharmWithBlueHeart : _rootedCharm;
            else
                charmMulti = blueHeartComplete ? 2.2f : 2f;

            dcMulti *= charmMulti;
            charmed = true;

            return dcMulti;
        }

        // doesn't check titan filter since titan drops are not currently being counted
        private static bool boostUnfiltered(int itemId)
        {
            var character = Plugin.Character;
            var settings = character.settings;

            if (character.arbitrary.lootFilter && character.inventory.itemList.itemFiltered[itemId])
                return false;

            if (!character.purchases.hasFilter || !settings.filterOn)
                return true;

            if (itemId < 15)
                return !settings.filterBoostAtk;

            if (itemId < 27)
                return !settings.filterBoostDef;

            return !settings.filterBoostSpec;
        }
    }
}
