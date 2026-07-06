using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Res3ColorEnterRGB
    {
        private static Popups.Res3ColorPopup _popup;
        private static CUIColorPicker _picker;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            _picker = __instance.allSettings.res3ColourPicker;

            _popup = new Popups.Res3ColorPopup();
            _popup.Closed += OnPopupClosed;

            _picker.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e => _popup.OpenAt(e.pressPosition));
        }

        private static void OnPopupClosed(object o, Popups.PopupClosedEventArgs<Color?> e)
        {
            if (e.Value.HasValue)
            {
                var character = Plugin.Character;
                character.res3.res3R = e.Value.Value.r;
                character.res3.res3G = e.Value.Value.g;
                character.res3.res3B = e.Value.Value.b;

                _picker.Color = e.Value.Value;
            }
        }
    }
}
