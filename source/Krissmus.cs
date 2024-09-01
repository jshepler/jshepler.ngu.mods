using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Krissmus
    {
        [HarmonyPatch, HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            // unlocks krissmuss ui theme from christmass 2019 event
            Plugin.OnSaveLoaded += (o, e) => Plugin.Character.settings.prizePicked = 6;

            // opens hidden "Krissmuss" screen from christmas 2020 event
            Plugin.OnUpdate += (o, e) =>
            {
                if (Input.GetKeyDown(KeyCode.X)
                    && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                    && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))))
                {
                    Plugin.Character.menuSwapper.swapMenu((int)Menu.Krissmuss);
                }
            };
        }
    }
}
