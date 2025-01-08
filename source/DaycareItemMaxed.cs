using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DaycareItemMaxed
    {
        private static Button _dkButton;
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);

        [HarmonyPostfix, HarmonyPatch(typeof(AllDaycareController), "Start")]
        private static void AllDaycareController_Start_postfix(AllDaycareController __instance)
        {
            _dkButton = __instance.button;
            Plugin.BeginCoroutine(watchForMaxedItem());
        }

        private static IEnumerator watchForMaxedItem()
        {
            var character = Plugin.Character;
            var maxedItems =
                from c in character.inventoryController.daycares
                where c.id < character.inventory.daycare.Count
                let i = character.inventory.daycare[c.id]
                where i.id > 0 && !i.isMacGuffin() && i.level + c.levelsAdded() >= 100
                select i;

            while (true)
            {
                if (character.InMenu(Menu.Inventory))
                    _dkButton.image.color = maxedItems.Any() ? Plugin.ButtonColor_Green : Color.white;

                yield return _wait;
            }
        }
    }
}
