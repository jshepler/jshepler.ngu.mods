using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using jshepler.ngu.mods.GameData.DropConditions;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ZoneDropsTooltip
    {
        private static int _zoneId;
        private static float _dcMulti;

        private static Func<double, string> _number = n => Plugin.Character.display(n);
        private static Func<float, string> _dcP = dc => $"{dc * 100f:0.##}%";
        private static Func<float, string> _dcM = dc =>
        {
            if (dc < 1e+6)
                return dc.ToString("x#,##0.00");

            return "x" + Plugin.Character.display(dc);
        };

        private static Func<int, bool> _hasDropped = itemId => Plugin.Character.inventory.itemList.itemDropped[itemId];

        private static Func<int, string> _name = itemId =>
        {
            if (!_hasDropped(itemId) && Options.DropTableTooltip.UnknownItems.Value == Options.DropTableTooltip.UnknownItemDisplay.Blur)
                return "????";

            return itemId switch
            {
                (int)Items.Poop => "Poop",
                (int)Items.QP => "QP",
                (int)Items.PP => "PP",
                (int)Items.AP => "AP",
                (int)Items.Exp => "Exp",
                (int)Items.Unknown => "Unknown",
                _ => Plugin.Character.itemInfo.itemName[itemId]
            };
        };

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "zoneDescriptions")]
        private static bool AdventureController_zoneDescriptions_prefix(AdventureController __instance, ref string ___message)
        {
            _zoneId = __instance.zone;
            if (Options.DropTableTooltip.Enabled.Value == false
                || !Input.GetKey(KeyCode.LeftAlt)
                || _zoneId >= DropTable.Zones.Count)
                return true;

            var zone = DropTable.Zones[_zoneId];
            var rooted = _zoneId >= 20;
            _dcMulti = rooted ? __instance.character.lootFactorRooted() : __instance.character.lootFactor();

            var text = $"<b>Drop Table For {__instance.zoneName(_zoneId)}</b>"
                + $"\n\n<b>Total DC Modifier{(rooted ? " (rooted)" : string.Empty)}:</b> {_dcM(_dcMulti)}";

            if (zone.NormalDrops != null)
                text += $"\n\n<b>Normal Drops:</b>{DropsString(zone.NormalDrops)}";

            if (zone.BossDrops != null)
                text += $"\n\n<b>Boss Drops:</b>{DropsString(zone.BossDrops)}";

            if (zone.TitanV1Drops != null)
            {
                if (zone.TitanV2Drops == null)
                    text += $"\n\n<b>Titan Drops:</b>{DropsString(zone.TitanV1Drops, true)}";
                else
                    text += $"\n\n<b>Titan V1 Drops:</b>{DropsString(zone.TitanV1Drops, true)}"
                          + $"\n\n<b>Titan V2 adds:</b>{DropsString(zone.TitanV2Drops, true)}"
                          + $"\n\n<b>Titan V3 adds:</b>{DropsString(zone.TitanV3Drops, true)}"
                          + $"\n\n<b>Titan V4 adds:</b>{DropsString(zone.TitanV4Drops, true)}";
            }

            if (zone.EnemyDrops != null)
            {
                foreach (var ed in zone.EnemyDrops)
                {
                    if (ed.HasVisibleDrops() || !Options.DropTableTooltip.OnlyUnlocked.Value)
                    {
                        var name = Plugin.Character.adventureController.fetchEnemyNamebySpriteID(ed.EnemyId);
                        text += $"\n\n<b>Extra drops for {name}:</b>{DropsString(ed)}";
                    }
                }
            }


            if (zone.MacGuffinDrop != null
                && (!Options.DropTableTooltip.OnlyUnlocked.Value
                    || (EnemiesKilledDropCondition.Walerp5Killed.IsConditionMet()
                        && (zone.MacGuffinDrop.Condition == null || zone.MacGuffinDrop.Condition.IsConditionMet()))))
            {
                var killsRemaining = 1000 - Plugin.Character.adventureController.globalKillCounter % 1000;
                var name = _name((int)zone.MacGuffinDrop.MacGuffinItem);
                text += $"\n\n<b>MacGuffin:</b> ({killsRemaining} kills remaining)\n<b><color=green>100%</color></b> for {name}";
            }

            if (zone.QuestItemDrop != null
                && (!Options.DropTableTooltip.OnlyUnlocked.Value
                    || (Plugin.Character.settings.beastOn
                        && zone.QuestItemDrop.Condition.IsConditionMet())))
            {
                var dc = Plugin.Character.beastQuestController.questDropChance();
                var color = dc >= 1.0f ? "green" : "red";
                var name = _name((int)zone.QuestItemDrop.QuestItem).Substring(40);
                text += $"\n\n<b>Quest Item:</b>\n<b><color={color}>{_dcP(dc)}</color></b> for {name}";
            }

            // flubber has a custom DC that scales with highest boss killed in current rebirth and is not affected by DC modifiers
            if (_zoneId == 0 && (!Options.DropTableTooltip.OnlyUnlocked.Value || Plugin.Character.bossID > 58))
            {
                // bossID is 0-based, flubber is available after killing boss 59, so bossID = 58
                // boss will be 60-301, so bossID will be 59-300
                // 
                // game does: Random.Range(58, 301) <= character.bossID
                // 301-58 = 243 possible values (not inclusive of 301)
                // 
                // if boss = 59, then odds are 2 in 243 (58, 59) = 2/243 = 0.008230 = 0.82%
                // if boss = 60, then odds are 3 in 243 (58, 59, 60) = 3/243 = 0.01234567 = 1.23%

                var dc = Math.Min((Plugin.Character.bossID - 57) / 243f, 1f);
                var color = dc < 1f ? "red" : "green";
                var name = _name((int)Items.Tutorial_Flubber);
                text += $"\n\n<b>Secret Drop:</b>\n<b><color={color}>{_dcP(dc)}</color></b> for {name}";
            }

            ___message = $"<size=11>{text}</size>";
            __instance.tooltip.showTooltip(___message);
            return false;
        }

        private static string DropsString(DropGroup group, bool isTitan = false)
        {
            var text = string.Empty;

            if (group.BaseGold > 0)
            {
                var minGold = group.BaseGold * 4 * Plugin.Character.totalGoldbonus();
                var maxGold = group.BaseGold * 5 * Plugin.Character.totalGoldbonus();
                text = $"\n<b><color=green>100%</color></b> for {_number(minGold)} - {_number(maxGold)} gold";
            }

            foreach (var idc in group.Items.OrderByDescending(i => i.BaseDC))
            {
                if (idc.Condition != null && !idc.Condition.IsConditionMet() && Options.DropTableTooltip.OnlyUnlocked.Value)
                    continue;

                var moddedDC = idc.BaseDC * _dcMulti + idc.BonuseDC;
                var dc = Math.Min(moddedDC, idc.MaxDC);
                var color = dc == idc.MaxDC ? "green" : "red";
                var showMax = idc.MaxDC < 1f && dc < idc.MaxDC;
                text += $"\n<b><color={color}>{_dcP(dc)}</color></b>{(showMax ? " (max: " + _dcP(idc.MaxDC) + ")" : string.Empty)} for ";

                switch (idc.ItemIds[0])
                {
                    case (int)Items.Poop:
                        text += $"{idc.BaseAmount} POOP";
                        break;

                    case (int)Items.QP:
                        var qp = Evaluators.TitanQP(_zoneId);
                        text += $"{_number(qp)} QP ({_number(idc.BaseAmount)} base)";
                        break;

                    case (int)Items.PP:
                        var pp = Evaluators.TitanPP(_zoneId);
                        text += $"{_number(pp)} PP progress ({_number(idc.BaseAmount)} base)";
                        break;

                    case (int)Items.AP:
                        var ap = Evaluators.TitanAP(_zoneId);
                        text += $"{_number(ap)} AP ({_number(idc.BaseAmount)} base)";
                        break;

                    case (int)Items.Exp:
                        var exp = isTitan ? Evaluators.TitanExp(_zoneId) : Plugin.Character.checkExpAdded(idc.BaseAmount);
                        text += $"{_number(exp)} EXP ({_number(idc.BaseAmount)} base)";
                        break;

                    default:
                        if (idc.ItemIds.Length == 1)
                            text += $"{_name(idc.ItemIds[0])}";
                        else
                        {
                            text += "1 of the following:";
                            foreach (var id in idc.ItemIds.OrderBy(i => i))
                                text += $"\n    {_name(id)}";
                        }
                        break;
                }
            }

            return text;
        }

        #region bug fixes

        /* clock dimension is supposed to drop a busted copy of wandoos 98, but has a bug that results in 0% DC

changing:
	// if ((double)value < (double)num3 * 0.012 * (double)num2)
	ldloc.0
	conv.r8
	ldloc.3
	conv.r8
	ldc.r8 0.012
	mul
	ldloc.2
	conv.r8
	mul

to:
	// if (value < (num3 += 0.012f * num2))
	ldloc.0
	ldloc.3
	ldc.r4 0.012
	ldloc.2
	mul
	add
	dup
	stloc.3

         */
        [HarmonyTranspiler, HarmonyPatch(typeof(LootDrop), "zone7Drop")]
        private static IEnumerable<CodeInstruction> LootDrop_zone7Drop_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Conv_R8))
                .RemoveInstructions(8)
                .Insert(new CodeInstruction(OpCodes.Ldloc_3)
                    , new CodeInstruction(OpCodes.Ldc_R4, 0.012f)
                    , new CodeInstruction(OpCodes.Ldloc_2)
                    , new CodeInstruction(OpCodes.Mul)
                    , new CodeInstruction(OpCodes.Add)
                    , new CodeInstruction(OpCodes.Dup)
                    , new CodeInstruction(OpCodes.Stloc_3));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        #endregion
    }
}
