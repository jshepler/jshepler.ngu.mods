using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class SearchInventory
    {
        private static Rect _designRect = new Rect(326f, 278f, 220f, 28f);
        private static GUIStyle _windowStyle;
        private static Rect _window;
        private static bool _setFocus;

        private static bool _showInput = false;
        private static string _searchString;

        private static Action updateMenu = () => Plugin.Character.inventoryController.updateInventory();

        [HarmonyPatch, HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += OnUpdate;
            Plugin.onGUI += onGUI;
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (!Plugin.Character.InMenu(Menu.Inventory))
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

                updateMenu();
            }

            else if (_showInput && Event.current.keyCode == KeyCode.Escape)
            {
                _showInput = false;
                updateMenu();
            }
        }

        private static void onGUI(object sender, EventArgs e)
        {
            if (!_showInput || !Plugin.Character.InMenu(Menu.Inventory))
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

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "updateItem")]
        private static void ItemController_updateItem_postfix(ItemController __instance)
        {
            if (!_showInput || __instance.id >= __instance.character.inventory.inventory.Count)
                return;

            var itemId = Plugin.Character.inventory.GetItem(__instance.id).id;
            __instance.image.color = hasMatch(itemId) ? Color.white : Color.gray;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutController), "updateItem")]
        private static void LoadoutController_updateItem_postfix(LoadoutController __instance)
        {
            var id = __instance.id;
            if (!_showInput || id >= 100000 || id <= -100)
                return;

            var inv = Plugin.Character.inventory;
            if (id >= 10000 && (id - 10000) >= inv.accs.Count)
                return;

            var itemId = Plugin.Character.inventory.GetItem(id).id;
            __instance.image.color = hasMatch(itemId) ? Color.white : Color.gray;
        }

        private static MethodInfo _getEffectName = typeof(InventoryController).GetMethod("effectName", BindingFlags.Instance | BindingFlags.NonPublic);
        private static string getEffectName(specType t) => (string)_getEffectName.Invoke(Plugin.Character.inventoryController, [t]);
        private static bool specHasMatch(specType t, string search) => getEffectName(t).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        private static ItemNameDesc _data;

        private static bool hasMatch(int itemId)
        {
            if (_data == null)
                _data = Plugin.Character.itemInfo;

            if (string.IsNullOrWhiteSpace(_searchString))
                return false;

            if (_data.itemName[itemId].IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (specHasMatch(_data.specType1[itemId], _searchString))
                return true;

            if (specHasMatch(_data.specType2[itemId], _searchString))
                return true;

            if (specHasMatch(_data.specType3[itemId], _searchString))
                return true;

            return false;
        }
    }
}
