using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TitanVersions
    {
        private static TitanVersionsPopup _popup;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                if(_popup == null)
                    _popup = new TitanVersionsPopup(e.Character);
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "Update")]
        private static bool Character_Update_prefix(Character __instance)
        {
            if(!Input.GetKeyDown(KeyCode.F2))
                return true;

            _popup.Toggle();

            return false;
        }

    }
}
