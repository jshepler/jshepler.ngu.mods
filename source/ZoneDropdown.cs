using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using UnityEngine;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ZoneDropdown
    {
        private static int _currentZoneValue;

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "constructDropdown")]
        private static bool AdventureController_constructDropdown_prefix(AdventureController __instance)
        {
            var zoneId = __instance.zone;
            var character = __instance.character;
            var diff = character.settings.rebirthDifficulty;

            var highestBossDefeated = diff switch
            {
                difficulty.normal => character.highestBoss,
                difficulty.evil => character.highestHardBoss,
                _ => character.highestSadisticBoss
            };

            var maxZoneIdInDifficulty = diff switch
            {
                difficulty.normal => 20,
                difficulty.evil => 31,
                _ => 45
            };

            var list = new List<string>();

            if (zoneId == 1000)
                list.Add("<b>THE I.T.O.P.O.D</b>");

            if (zoneId == -1)
                list.Add("<b>Safe Zone: Awakening Site</b>");
            else
                list.Add("Safe Zone: Awakening Site");

            var maxUnlockedZone = getMaxUnlockedZone();
            var maxUnlockedZoneId = maxUnlockedZone == null ? -1 : maxUnlockedZone.id;

            for (var x = 0; x <= maxZoneIdInDifficulty; x++)
            {
                var zone = Zones.Zone[x];
                var line = $"{zone.bossId}: {zone.name}";

                if ((diff == difficulty.evil && x < 21) || (diff == difficulty.sadistic && x < 32))
                    line = zone.name;

                else if (zone.bossId > highestBossDefeated
                    || (x == 45 && !character.adventure.ratTitanDefeated)) // tippi killed required to unlock traitor's zone
                    line = $"<color=grey>{zone.bossId}: ?????</color>";

                else if (zone.id > maxUnlockedZoneId)
                    line = $"<color=grey>{zone.bossId}: {zone.name}</color>";

                if (x == zoneId)
                    line = $"<b>{line}</b>";

                list.Add(line);
            }

            _currentZoneValue = zoneId switch
            {
                1000 => 0, // if current zone itopod, then the first item in the list is itodod
                -1 => 0,   // otherwise first time in the list is safe zone (-1)
                _ => zoneId + 1 // don't need to add 1 because there's an extra entry for safe zone
            };

            __instance.zoneDropdown.ClearOptions();
            __instance.zoneDropdown.AddOptions(list);
            __instance.zoneDropdown.SetValueWithoutNotify(_currentZoneValue);

            __instance.zoneDropdown.itemText.fontSize = 11;
            __instance.zoneDropdown.captionText.text = $"<b>{__instance.zoneName(zoneId)}</b>";

            return false;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "updateMenu")]
        private static IEnumerable<CodeInstruction> AdventureController_updateMenu_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var zoneDropdown = typeof(AdventureController).GetField("zoneDropdown");
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, zoneDropdown))
                .RemoveInstructions(10);

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(ZoneSelector), "changeZone")]
        private static IEnumerable<CodeInstruction> ZoneSelected_changeZone_prefix(IEnumerable<CodeInstruction> instructions)
        {
            var dropdownField = typeof(ZoneSelector).GetField("dropdown");
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, dropdownField))
                .RemoveInstructions(12)

                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, dropdownField))
                .RemoveInstructions(17)

                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, dropdownField))
                .Advance(-1)
                .RemoveInstructions(11);

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ZoneSelector), "selectZone")]
        private static bool ZoneSelector_selectZone_prefix(ref int zone, ZoneSelector __instance)
        {
            if (__instance.ac.zone == 1000)
                zone--;

            if (zone > 0)
            {
                var z = Zones.Zone[zone - 1];
                if (__instance.character.effectiveBossID() < z.effectiveBossId
                    || (zone == 45 && !__instance.character.adventure.ratTitanDefeated)) // tippi kill required to unlock/select traitor's zone
                {
                    __instance.dropdown.SetValueWithoutNotify(_currentZoneValue);
                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ZoneForwardClick), "goToMaxZone", [])]
        private static bool ZoneForwardClick_goToMaxZone_prefix(ZoneForwardClick __instance)
        {
            if (Plugin.Character.arbitrary.advAdvancerBought
                && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return ZoneForwardClick_goToMaxZone_int_prefix(Plugin.Character.arbitrary.advAdvancerZone, __instance);

            return ZoneForwardClick_goToMaxZone_int_prefix(Zones.MAXZONEID, __instance);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ZoneForwardClick), "goToMaxZone", [typeof(int)])]
        private static bool ZoneForwardClick_goToMaxZone_int_prefix(int cap, ZoneForwardClick __instance)
        {
            if (cap < 0)
                cap = 0;

            var maxZone = getMaxUnlockedZone();
            if (maxZone == null)
                return false; // is possible just after rebirth before any bosses are fought

            var newZoneId = maxZone.id;
            while (newZoneId > cap || Zones.TitanZoneIds.Contains(newZoneId))
                newZoneId--;

            __instance.ac.zoneSelector.changeZone(newZoneId);

            return false;
        }

        private static ZoneData getMaxUnlockedZone()
        {
            var effBossId = Plugin.Character.effectiveBossID();
            var maxZone = Zones.Zone.LastOrDefault(z => z.effectiveBossId <= effBossId);

            return maxZone;
        }

        internal static int GetMaxReachableZone(bool includeTitans = false)
        {
            var maxZone = getMaxUnlockedZone();
            if (maxZone == null)
                return -1; // is possible just after rebirth before any bosses are fought

            var maxZoneId = maxZone.id;
            while (Zones.TitanZoneIds.Contains(maxZoneId))
                maxZoneId--;

            return maxZoneId;
        }
    }
}
