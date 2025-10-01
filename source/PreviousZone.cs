using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PreviousZone
    {
        private static int _lastZone = -2;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += onUpdate;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ZoneSelector), "changeZone")]
        private static void ZoneSelector_changeZone_prefix(int zone)
        {
            var current = Plugin.Character.adventure.zone;
            if (zone != current)
                _lastZone = current;
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (Plugin.Character == null || !Plugin.Character.InMenu(Menu.Adventure) || !Input.GetKeyDown(KeyCode.F3) || _lastZone == -2)
                return;

            Plugin.Character.adventureController.zoneSelector.changeZone(_lastZone);
        }
    }
}
