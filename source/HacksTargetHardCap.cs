using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HacksTargetHardCap
    {
        [HarmonyPrefix, HarmonyPatch(typeof(HackUIController), "setToNextMilestone")]
        private static bool HackUIController_setToNextMilestone_prefix(HackUIController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            var id = __instance.id;
            if (id < 0 || id > __instance.character.hacks.hacks.Count)
                return true;

            var hardCap = __instance.character.hacksController.hardCapLevel(id);
            __instance.character.hacks.hacks[id].target = hardCap;
            __instance.updateInput();

            return false;
        }
    }
}
