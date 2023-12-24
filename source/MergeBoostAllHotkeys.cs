using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class MergeBoostAllHotkeys
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Character), "Update")]
        private static bool Character_Update_prefix(Character __instance)
        {
            if (__instance.InMenu(Menu.Inventory) == false)
                return true;

            if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftShift))
            {
                __instance.inventoryController.autoBoost();
                return false;
            }

            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftShift))
            {
                __instance.inventoryController.autoMerge();
                return false;
            }

            return true;
        }
    }
}
