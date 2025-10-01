using System;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishSearch
    {
        private static Rect _designRect = new Rect(769f, 248f, 220f, 28f);
        private static GUIStyle _windowStyle;
        private static Rect _window;
        private static bool _setFocus;

        private static bool _showInput = false;
        private static string _searchString;

        private static Action updateMenu = () => Plugin.Character.wishesController.updateMenu();

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += onUpdate;
            Plugin.onGUI += onGUI;
        }

        private static bool excludeWish(int wishId)
        {
            var p = Plugin.Character.wishesController.properties[wishId];

            return p.wishName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) == -1
                && p.WishDesc.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) == -1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "constructFullList")]
        private static void WishesController_constructFullList_postfix(WishesController __instance)
        {
            if (!_showInput || string.IsNullOrWhiteSpace(_searchString))
                return;

            var wishIds = __instance.curValidUpgradesList;
            for (var x = wishIds.Count - 1; x >= 0; x--)
                if (excludeWish(wishIds[x]))
                    wishIds.RemoveAt(x);
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (!Plugin.Character.InMenu(Menu.Wishes))
                return;

            if (!_showInput && Input.GetKeyDown(KeyCode.S) && !Plugin.InputFieldHasFocus)
            {
                _window = _designRect;

                var additionalScaling = Options.Experimental.ModPopupScaling.Value;
                _window.x /= additionalScaling;
                _window.y /= additionalScaling;

                _showInput = true;
                _setFocus = true;
                _searchString = string.Empty;
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    if (_showInput)
                    {
                        _showInput = false;
                        updateMenu();
                    }
                    break;
            }
        }

        private static void onGUI(object sender, EventArgs e)
        {
            if (!_showInput || !Plugin.Character.InMenu(Menu.Wishes))
                return;

            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(_window, new Color32(30, 30, 30, 255));
            }

            UIScaler.Begin();
            GUILayout.BeginArea(_window, _windowStyle);
            GUILayout.BeginHorizontal();

            GUILayout.Label("Search: ", GUILayout.ExpandWidth(false));

            GUI.SetNextControlName("searchInput");
            _searchString = GUILayout.TextField(_searchString, GUILayout.ExpandWidth(true));

            if (_setFocus)
            {
                GUI.FocusControl("searchInput");
                _setFocus = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            UIScaler.End();

            if (GUI.changed)
                updateMenu();
        }
    }
}
