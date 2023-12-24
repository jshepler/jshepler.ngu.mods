using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PartialCaps
    {
        private static PartialCapsPopup _popup = new();
        private static BloodMagicController _controller;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            _popup = new PartialCapsPopup();
            _popup.Closed += PopupClosed;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BloodMagicController), "cap")]
        private static bool BloodMagicController_cap_prefix(BloodMagicController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            var cap = __instance.capValue();
            
            var caps = new List<long>();
            for (var x = 0; x < 9; x++)
                caps.Add(cap / (x + 2));

            _controller = __instance;
            _popup.OpenAt(Event.current.mousePosition, caps);

            return false;
        }

        private static void PopupClosed(object sender, PopupClosedEventArgs<long?> e)
        {
            if (!e.Value.HasValue)
                return;

            var amount = e.Value.Value;
            var character = _controller.character;
            var ritual = character.bloodMagic.ritual[_controller.id];
            var oldAmount = ritual.magic;

            character.magic.idleMagic += ritual.magic;
            ritual.magic = 0;

            if (character.magic.idleMagic < amount)
            {
                character.magic.idleMagic -= oldAmount;
                ritual.magic += oldAmount;
                return;
            }

            character.magic.idleMagic -= amount;
            ritual.magic += amount;

            _controller.updateBloodMagicText();
        }
    }
}
