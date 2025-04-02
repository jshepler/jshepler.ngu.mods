using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class HackTargets
    {
        [HarmonyPrefix, HarmonyPatch(typeof(HackUIController), "setToNextMilestone")]
        private static bool HackUIController_setToNextMilestone_prefix(HackUIController __instance)
        {
            var id = __instance.id;
            if (id < 0 || id > __instance.character.hacks.hacks.Count)
                return true;

            var shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (shiftDown && altDown)
                for (var x = 0; x < 15; x++)
                    targetHardcap(x);

            else if (shiftDown)
                targetPrevMilestone(id);

            else if (altDown)
                targetHardcap(id);

            else
                return true;

            __instance.character.hacksController.refreshMenu();
            return false;
        }

        private static void targetHardcap(int id)
        {
            var character = Plugin.Character;
            var hardCap = character.hacksController.hardCapLevel(id);
            character.hacks.hacks[id].target = hardCap;
        }

        private static void targetPrevMilestone(int id)
        {
            var character = Plugin.Character;
            var hack = character.hacks.hacks[id];
            if (hack.level >= hack.target)
                return;

            var ms = (int)character.hacksController.milestoneThreshold(id);
            var target = (int)hack.target;
            target = (Mathf.CeilToInt(target / (float)ms) - 1) * ms;
            if (target < hack.level)
                target = (int)hack.level;

            character.hacks.hacks[id].target = target;
        }
    }
}
