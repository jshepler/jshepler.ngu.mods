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

            var character = __instance.character;
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

                foreach (var id in loadoutDiggers)
                {
                    var drain = __instance.drain(id, 1) - __instance.drain(id);
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
                if(diggers[id].curLevel == 0)
                    diggers[id].active = false;

            activeDiggers.AddRange(loadoutDiggers);

            __instance.tooltip.showTooltip("Saved diggers have been capped!", 2f);
            __instance.refreshMenu();

            return false;
        }
    }
}
