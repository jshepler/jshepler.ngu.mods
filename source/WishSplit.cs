using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishSplit
    {
        private static HashSet<int> _selectedIds = new();
        private static int MaxWishes => Plugin.Character.wishesController.curWishSlots();

        [HarmonyPostfix, HarmonyPatch(typeof(WishPodUIController), "selectThisWish")]
        private static void WishPodUIController_selectThisWish_postfix(WishPodUIController __instance)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                var id = __instance.id;
                if (_selectedIds.Contains(id))
                    _selectedIds.Remove(id);
                else if (_selectedIds.Count < MaxWishes)
                    _selectedIds.Add(id);

                __instance.updateIcon();
            }
            else
                ClearSelected();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishPodUIController), "updateIcon")]
        private static void WishPodUIController_updateIcon_prefix(WishPodUIController __instance)
        {
            var id = __instance.id;
            var wish = __instance.character.wishes.wishes[id];

            if (__instance.character.menuID != 53 || __instance.invalidID())
                return;

            if(_selectedIds.Contains(id))
                __instance.wishIcon.color = Color.yellow;
            else if (wish.level == 0 && wish.progress > 0)
                __instance.wishIcon.color = Color.white;
        }

        private static void ClearSelected()
        {
            _selectedIds.Clear();
            Plugin.Character.wishesController.updateAllPods();
        }


        [HarmonyPrefix
            , HarmonyPatch(typeof(WishesController), "addEnergy", new Type[] { })
            , HarmonyPatch(typeof(WishesController), "addMagic", new Type[] { })
            , HarmonyPatch(typeof(WishesController), "addRes3", new Type[] { })]
        private static bool WishesController_addResource_prefix(WishesController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftAlt))
                return true;

            var count = _selectedIds.Count;
            if (count == 0)
                return false;

            __instance.removeAllResources();
            var eSplit = IdleEnergy / count;
            var mSplit = IdleMagic / count;
            var r3Split = IdleRes3 / count;

            foreach (var id in _selectedIds)
            {
                var wish = Plugin.Character.wishes.wishes[id];
                wish.energy = eSplit;
                wish.magic = mSplit;
                wish.res3 = r3Split;

                IdleEnergy -= eSplit;
                IdleMagic -= mSplit;
                IdleRes3 -= r3Split;
            }

            ClearSelected();
            __instance.updateText();

            return false;
        }

        private static long IdleEnergy
        {
            get => Plugin.Character.idleEnergy;
            set => Plugin.Character.idleEnergy = value;
        }

        private static long IdleMagic
        {
            get => Plugin.Character.magic.idleMagic;
            set => Plugin.Character.magic.idleMagic = value;
        }

        private static long IdleRes3
        {
            get => Plugin.Character.res3.idleRes3;
            set => Plugin.Character.res3.idleRes3 = value;
        }
    }
}
