using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class SearchInventory
    {
        private static bool _showInput = false;
        private static GUIStyle _windowStyle;
        private static Rect _window = new Rect(400, 370, 200, 30);
        private static string _searchString;
        private static string _lastString;
        private static bool _setFocus = false;
        private static Dictionary<specType, string> _effNames = new();

        [HarmonyPatch, HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += OnUpdate;
            Plugin.onGUI += OnGUI;

            Plugin.OnGameStart += (o, e) =>
            {
                var getEffectNameMethod = typeof(InventoryController).GetMethod("effectName", BindingFlags.Instance | BindingFlags.NonPublic);
                var getEffectName = (specType t) => (string)getEffectNameMethod.Invoke(Plugin.Character.inventoryController, [t]);
                var types = Enum.GetValues(typeof(specType)).Cast<specType>();
                _effNames = types.ToDictionary(t => t, t => getEffectName(t).ToLowerInvariant().Replace("butts", "{0}"));
            };
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (!Plugin.Character.InMenu(Menu.Inventory))
                return;

            if (!_showInput && Input.GetKeyDown(KeyCode.S)
                // ignore the s if entering a loadout name - currentSelectedGameObject won't be null in that case
                && EventSystem.current.currentSelectedGameObject == null)
            {
                var sf = Plugin.Character.tooltip.canvas.scaleFactor;
                _window = new Rect(324 * sf, 282 * sf, 200 * sf, 26);
                _showInput = true;
                _setFocus = true;
                _searchString = string.Empty;
                _lastString = string.Empty;
                Plugin.Character.inventoryController.updateInventory();
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    if (_showInput)
                    {
                        _showInput = false;
                        Plugin.Character.inventoryController.updateInventory();
                    }
                    break;
            }
        }

        private static void OnGUI(object sender, EventArgs e)
        {
            if (!_showInput || !Plugin.Character.InMenu(Menu.Inventory))
                return;

            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(_window, new Color32(30, 30, 30, 255));
            }

            GUILayout.BeginArea(_window, _windowStyle);
            GUILayout.BeginHorizontal();

            GUILayout.Label("Search: ", GUILayout.ExpandWidth(false));

            GUI.SetNextControlName("searchInput");
            _lastString = GUILayout.TextField(_lastString, GUILayout.ExpandWidth(true));

            if (_setFocus)
            {
                GUI.FocusControl("searchInput");
                _setFocus = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (_lastString != _searchString)
            {
                _searchString = _lastString;
                Plugin.Character.inventoryController.updateInventory();
            }
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
        private static bool specHasMatch(specType t, string search) => getEffectName(t).ToLowerInvariant().Contains(search);
        private static ItemNameDesc _data;

        private static bool hasMatch(int itemId)
        {
            if (_data == null)
                _data = Plugin.Character.itemInfo;

            if (_searchString == string.Empty)
                return false;

            var ss = _searchString.ToLowerInvariant();

            if (_data.itemName[itemId].ToLowerInvariant().Contains(ss))
                return true;

            if (specHasMatch(_data.specType1[itemId], ss))
                return true;

            if (specHasMatch(_data.specType2[itemId], ss))
                return true;

            if (specHasMatch(_data.specType3[itemId], ss))
                return true;

            return false;
        }
    }
}
