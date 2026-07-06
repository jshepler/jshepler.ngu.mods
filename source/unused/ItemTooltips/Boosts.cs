using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jshepler.ngu.mods.GameData;
using UnityEngine;

namespace jshepler.ngu.mods.ItemTooltips
{
    internal record PTS (float Power, float Toughness, float Special);

    internal static class Boosts
    {
        internal static int GetValue(int itemId) => _values[_boostIndex(itemId)];
        private static int _boostIndex(int itemId) => (itemId - 1) % 13;
        private static List<int> _values = [1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000];

        private static List<int> _boostFloors = [0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 700, 850, 1150, int.MaxValue];
        private static List<DropItems> _itopodDrops = DropTable.BoostItems.Select(items => new DropItems(0.14f, 0.14f, items)).ToList();

        private static float _rootedCharm = Mathf.Pow(2f, 1f / 3f);
        private static float _rootedCharmWithBlueHeart = Mathf.Pow(2.2f, 1f / 3f);

        internal static PTS GetBoostsNeeded(Equipment item)
        {
            var levelMulti = 1f + item.level / 100f;
            var missingPower = Mathf.Floor(item.capAttack * levelMulti) - item.curAttack;
            var missingToughness = Mathf.Floor(item.capDefense * levelMulti) - item.curDefense;
            var missingSpecial1 = Mathf.Floor(item.spec1Cap * levelMulti) - item.spec1Cur;
            var missingSpecial2 = Mathf.Floor(item.spec2Cap * levelMulti) - item.spec2Cur;
            var missingSpecial3 = Mathf.Floor(item.spec3Cap * levelMulti) - item.spec3Cur;

            return new PTS(missingPower, missingToughness, missingSpecial1 + missingSpecial2 + missingSpecial3);
        }

        internal static string BuildEstBoostTimes(Equipment item)
        {
            var boosts = GetBoostsNeeded(item);
            var totalBoostsMissing = boosts.Power + boosts.Toughness + boosts.Special;
            if (totalBoostsMissing <= 0)
                return string.Empty;

            var sb = new StringBuilder("\n\n<b>Avg. Boosts per Kill:</b> ");
            var zoneId = Plugin.Character.adventureController.zone;
            if (zoneId == -1 || Zones.TitanZoneIds.Contains(zoneId))
                sb.Append("n/a");

            else
            {
                var expectedBoostsPerKill = getAverageBoostPerKillFromZone(zoneId, out var charmed);
                var color = charmed ? "blue" : "black";
                sb.Append($"<color={color}>{expectedBoostsPerKill:#,##0.#}</color>");
            }

            sb.Append("\n<b>Boosts Remaining:</b>");

            var seconds = calcSecondsRemaining(boosts);
            var sp = seconds.Power == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(seconds.Power);
            sb.Append($"\n  <b>Power:</b> {boosts.Power:#,##0} ({sp})");

            var st = seconds.Toughness == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(seconds.Toughness);
            sb.Append($"\n  <b>Tough:</b> {boosts.Toughness:#,##0} ({st})");

            var ss = seconds.Special == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(seconds.Special);
            sb.Append($"\n  <b>Special:</b> {boosts.Special:#,##0} ({ss})");

            //var totalSeconds = seconds.Power + seconds.Toughness + seconds.Special;
            //var ts = totalSeconds == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(totalSeconds);
            //sb.Append($"\n\n<b>Total:</b> {totalBoostsMissing} ({ts})");

            return sb.ToString();
        }

        internal static string BuildEstBoostTimeSummary()
        {
            var boosts = getBoostablePTS();
            var sb = new StringBuilder("Total estimated times to boost all items equipped or in merge slots. If a boost type is being auto-transformed to a different type, \"n/a\" will be shown.");

            var power = boosts.Sum(b => b.Power);
            var sp = power == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(power);
            sb.Append($"\n\n<b>Power:</b> {sp}");

            var tough = boosts.Sum(b => b.Toughness);
            var st = tough == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(tough);
            sb.Append($"\n<b>Tough:</b> {st}");

            var spec = boosts.Sum(b => b.Special);
            var ss = spec == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(spec);
            sb.Append($"\n<b>Special:</b> {ss}");

            //var total = power + tough + spec;
            //var ts = total == float.PositiveInfinity ? "n/a" : NumberOutput.timeOutput(total);
            //sb.Append($"\n\n<b>Total:</b> {ts}");

            return sb.ToString();
        }

        private static List<PTS> getBoostablePTS()
        {
            var character = Plugin.Character;
            List<PTS> itemBoosts = new List<PTS>();

            for (var slotId = -6; slotId < 0; slotId++)
            {
                var item = character.inventory.GetItem(slotId);
                if (item == null || item.id == 0)
                    continue;

                var boosts = GetBoostsNeeded(item);
                if (boosts.Power + boosts.Toughness + boosts.Special > 0f)
                    itemBoosts.Add(calcSecondsRemaining(boosts));
            }

            foreach (var acc in character.inventory.accs)
            {
                if (acc == null || acc.id == 0)
                    continue;

                var boosts = GetBoostsNeeded(acc);
                if (boosts.Power + boosts.Toughness + boosts.Special > 0f)
                    itemBoosts.Add(calcSecondsRemaining(boosts));
            }

            var mergeSlots = character.inventoryController.totalInvMergeSlots();
            if (mergeSlots == 0)
                return itemBoosts;

            for (var slotId = 0; slotId < mergeSlots; slotId++)
            {
                var item = character.inventory.GetItem(slotId);
                if (item == null || item.id == 0)
                    continue;

                var boosts = GetBoostsNeeded(item);
                if (boosts.Power + boosts.Toughness + boosts.Special > 0f)
                    itemBoosts.Add(calcSecondsRemaining(boosts));
            }

            return itemBoosts;
        }

        private static PTS calcSecondsRemaining(PTS boostsRemaining)
        {
            var character = Plugin.Character;

            var zoneId = character.adventureController.zone;
            if (zoneId == -1 || Zones.TitanZoneIds.Contains(zoneId))
                return new PTS(-1f, -1f, -1f);

            var expectedBoostsPerKill = getAverageBoostPerKillFromZone(zoneId, out var charmed);
            var autoTransform = character.settings.autoTransform;

            var power =
                boostsRemaining.Power == 0f ? 0f
                : autoTransform == 1 ? calcSecondsFromKills(boostsRemaining.Power / expectedBoostsPerKill)
                : autoTransform == 0 ? calcSecondsFromKills(boostsRemaining.Power / (expectedBoostsPerKill / 3f))
                : float.PositiveInfinity;

            var tough =
                boostsRemaining.Toughness == 0f ? 0f
                : autoTransform == 2 ? calcSecondsFromKills(boostsRemaining.Toughness / expectedBoostsPerKill)
                : autoTransform == 0 ? calcSecondsFromKills(boostsRemaining.Toughness / (expectedBoostsPerKill / 3f))
                : float.PositiveInfinity;

            var special =
                boostsRemaining.Special == 0f ? 0f
                : autoTransform == 3 ? calcSecondsFromKills(boostsRemaining.Special / expectedBoostsPerKill)
                : autoTransform == 0 ? calcSecondsFromKills(boostsRemaining.Special / (expectedBoostsPerKill / 3f))
                : float.PositiveInfinity;

            return new PTS(power, tough, special);
        }

        private static float calcSecondsFromKills(float killsNeeded)
        {
            var character = Plugin.Character;
            var respawnTime = character.adventureController.respawnTime();
            var idleAttackSpeed = character.adventure.attackSpeed;
            var secondsPerKill = respawnTime + idleAttackSpeed;
            var secondsRemaining = killsNeeded * secondsPerKill;

            return secondsRemaining;
        }

        internal static float GetAverageRecycledBoost(int itemId, float boostBonus, float recycleChance)
        {
            // should never be above 100%, but bug in vanilla's Character.totalRecycleBonus()
            // counts ALL n/e/s basic challenge completions, including any over the 5
            if (recycleChance > 1f)
                recycleChance = 1f;

            var totalBoost = 0f;
            var probability = 1f;

            for (var x = _boostIndex(itemId); x >= 0; x--)
            {
                totalBoost += probability * _values[x] * boostBonus;
                probability *= recycleChance;
            }

            return totalBoost;
        }

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
            var boostBonus = character.allItemList.boostBonus();
            var recycleChance = Plugin.Character.totalRecycleBonus();

            var totalWeightedBoost = 0f;
            foreach (var drop in boostDrops)
            {
                var avgRecycledBoost = GetAverageRecycledBoost(drop.ItemIds[0], boostBonus, recycleChance);

                var moddedDC = drop.BaseDC * playerDcMulti + drop.BonuseDC;
                var dc = Math.Min(moddedDC, drop.MaxDC);

                // if any of the boosts are filtered, the number of potential drops is reduced and needs to be accounted for
                var unfilteredBoostsMulti = drop.ItemIds.Count(boostUnfiltered) / (float)drop.ItemIds.Length;

                var avgBoost = avgRecycledBoost * dc * unfilteredBoostsMulti;
                if (avgBoost > 0)
                    totalWeightedBoost += avgBoost;
            }

            return totalWeightedBoost;
        }

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
