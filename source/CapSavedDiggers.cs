using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CapSavedDiggers
    {
        [HarmonyPrefix, HarmonyPatch(typeof(AllGoldDiggerController), "applyDiggerLoadout")]
        private static bool AllGoldDiggerController_applyDiggerLoadout_prefix(AllGoldDiggerController __instance)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
                return true;

            ApplyDiggerLoadout(__instance);

            Plugin.ShowNotification("Saved diggers have been capped!", 2f);
            __instance.refreshMenu();

            return false;
        }

        internal static void ApplyDiggerLoadout(AllGoldDiggerController controller, List<long> softCaps = null)
        {
            var character = controller.character;
            var diggers = character.diggers.diggers;

            var activeDiggers = character.diggers.activeDiggers;
            activeDiggers.Do(id => diggers[id].active = false);
            activeDiggers.Clear();

            var loadoutDiggers = character.diggers.loadoutDiggers;
            foreach (var id in loadoutDiggers)
            {
                diggers[id].curLevel = 0;
                diggers[id].active = true;
            }

            var grossGps = character.grossGoldPerSecond();
            var totalDrain = 0d;

            while (true)
            {
                var cheapestId = -1;
                var cheapestDrain = double.MaxValue;

                for (var x = 0; x < loadoutDiggers.Count; x++)
                {
                    var id = loadoutDiggers[x];

                    // ignore those >= soft cap
                    if (softCaps != null && x < softCaps.Count && diggers[id].curLevel >= softCaps[x])
                        continue;

                    // ignore those at max
                    if (diggers[id].curLevel >= diggers[id].maxLevel)
                        continue;

                    var drain = controller.drain(id, 1) - controller.drain(id);
                    if (totalDrain + drain > grossGps)
                        continue;

                    if (drain < cheapestDrain)
                    {
                        cheapestId = id;
                        cheapestDrain = drain;
                    }
                }

                if (cheapestId == -1)
                    break;

                diggers[cheapestId].curLevel++;
                totalDrain += cheapestDrain;
            }

            foreach (var id in loadoutDiggers)
                if (diggers[id].curLevel == 0)
                    diggers[id].active = false;

            activeDiggers.AddRange(loadoutDiggers.Where(i => diggers[i].active));
        }
    }
}
