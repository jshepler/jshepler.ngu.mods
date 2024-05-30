using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishSplit
    {
        private static HashSet<int> _selectedIds = new();
        private static int MaxWishes => Plugin.Character.wishesController.curWishSlots();

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

        private static Func<int, long> wishR3Cap => wishId =>
        {
            var wc = Plugin.Character.wishesController;

            var cap = Mathf.Ceil(
                Mathf.Pow(
                    wc.minimumWishTime()
                    * wc.wishSpeedDivider(wishId)
                    / wc.energyFactor(wishId)
                    / wc.magicFactor(wishId)
                    / wc.totalWishSpeedBonuses()
                    , 1.0f / 0.17f)
                / Plugin.Character.totalRes3Power());

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return (long)cap;
        };

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
            if (__instance.character.menuID != 53 || __instance.invalidID())
                return;

            var id = __instance.id;
            var wish = __instance.character.wishes.wishes[id];

            if(_selectedIds.Contains(id))
                __instance.wishIcon.color = Color.yellow;
            else if (wish.level == 0 && wish.progress > 0)
                __instance.wishIcon.color = Color.white;
        }

        [HarmonyPrefix
            , HarmonyPatch(typeof(WishesController), "addEnergy", [])
            , HarmonyPatch(typeof(WishesController), "addMagic", [])
            , HarmonyPatch(typeof(WishesController), "addRes3", [])]
        private static bool WishesController_addResource_prefix(WishesController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftAlt))
                return true;

            var wishes = __instance.character.wishes.wishes;
            var selected = _selectedIds.Select(i => wishes[i]).ToList();
            SplitResources(selected);

            ClearSelected();
            __instance.updateText();

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "doLevelupEffect")]
        private static void WishesController_doLevelupEffect_postfix(int id, int level, WishesController __instance)
        {
            if (level >= __instance.maxWishLevel(id))
                return; // the code that starts the next wish will handle it

            RedistributeR3();
            __instance.updateText();
        }

        internal static void SplitResources()
        {
            var runningWishes = Plugin.Character.wishes.wishes
                .Where(w => w.energy > 0 && w.magic > 0 && w.res3 > 0)
                .ToList();

            SplitResources(runningWishes);
        }

        internal static void SplitResources(List<Wish> wishes)
        {
            var count = wishes.Count;
            if (count == 0)
                return;

            var controller = Plugin.Character.wishesController;
            controller.removeAllResources();

            var eSplit = IdleEnergy / count;
            var mSplit = IdleMagic / count;
            var r3Split = IdleRes3 / count;

            foreach (var wish in wishes)
            {
                wish.energy = eSplit;
                wish.magic = mSplit;
                wish.res3 = r3Split;

                IdleEnergy -= eSplit;
                IdleMagic -= mSplit;
                IdleRes3 -= r3Split;
            }

            RedistributeR3();
        }

        internal static void RedistributeR3()
        {
            if (!Options.WishR3Cap.Enabled.Value)
                return;

            var character = Plugin.Character;

            var runningWishes = character.wishes.wishes
                .Select((w, i) => new { wish = w, cap = wishR3Cap(i) })
                .Where(w => w.wish.energy > 0 && w.wish.magic > 0 && w.wish.res3 > 0)
                .OrderBy(w => w.cap)
                .ToList();

            character.wishesController.removeAllRes3();
            var amountLeft = character.res3.idleRes3;

            while (runningWishes.Count > 0)
            {
                var rw = runningWishes[0];
                var amount = Math.Min(amountLeft / runningWishes.Count, rw.cap);
                if (amount > amountLeft)
                    amount = amountLeft;

                rw.wish.res3 += amount;
                amountLeft -= amount;
                runningWishes.RemoveAt(0);
            }

            character.res3.idleRes3 = amountLeft;
        }

        private static void ClearSelected()
        {
            _selectedIds.Clear();
            Plugin.Character.wishesController.updateAllPods();
        }
    }
}
