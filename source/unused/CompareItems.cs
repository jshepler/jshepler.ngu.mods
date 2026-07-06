using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CompareItems
    {
        private static Toast _toast;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                    HideTooltip();
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemController), "Update")]
        private static void ItemControler_Update_postfix(ItemController __instance, string ___message)
        {
            if (!__instance.character.InMenu(Menu.Inventory))
                return;

            if (Input.GetKeyDown(KeyCode.C) && __instance.hovered && !__instance.inventoryController.midDrag)
                __instance.StartCoroutine(ShowTooltip(___message));
        }

        private static IEnumerator ShowTooltip(string text)
        {
            if (_toast == null)
                _toast = ToastNotifications.GetAvailableToast();

            _toast.Text = text;
            _toast.IsActive = true;
            yield return null;

            _toast.Position = new Vector3(0, Screen.height - (_toast.Height * Plugin.Character.tooltip.canvas.scaleFactor));
        }

        private static void HideTooltip()
        {
            if(_toast != null)
                _toast.IsActive = false;
        }
    }
}
