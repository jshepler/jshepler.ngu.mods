using System;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class SearchQuirks
    {
        private static Rect _designRect = new Rect(724f, 105f, 220f, 28f);
        private static GUIStyle _windowStyle;
        private static Rect _window;
        private static bool _setFocus;

        private static bool _showInput = false;
        private static string _searchString;

        private static Action updateMenu = () => Plugin.Character.beastQuestPerkController.refreshMenu();

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += onUpdate;
            Plugin.onGUI += onGUI;
        }

        private static bool excludeQuirk(int quirkId)
        {
            var controller = Plugin.Character.beastQuestPerkController;
            var name = controller.quirkName[quirkId];
            var desc = controller.quirkDesc[quirkId];

            return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) == -1
                && desc.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) == -1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestPerkController), "constructList")]
        private static void BeastQuestPerkController_constructList_postfix(BeastQuestPerkController __instance)
        {
            if (!_showInput || string.IsNullOrWhiteSpace(_searchString))
                return;

            var quirkIds = __instance.curValidUpgradesList;
            for (var x = quirkIds.Count - 1; x >= 0; x--)
                if (excludeQuirk(quirkIds[x]))
                    quirkIds.RemoveAt(x);
        }

        private static void onUpdate(object sender, EventArgs e)
        {
            if (!Plugin.Character.InMenu(Menu.Quirks))
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

            else if (_showInput && Event.current.keyCode == KeyCode.Escape)
            {
                _showInput = false;
                updateMenu();
            }
        }

        private static void onGUI(object sender, EventArgs e)
        {
            if (!_showInput || !Plugin.Character.InMenu(Menu.Quirks))
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
