using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishSplit
    {
        private static HashSet<int> _selectedIds = new();
        private static int MaxWishes => Plugin.Character.wishesController.curWishSlots();
        private static bool _offlineInProgress = false;

        private static bool _wishListEnabled => Options.WishList.Enabled.Value;
        private static bool _wishListSingleLevelMode => Options.WishList.SingleLevelMode.Value;
        private static bool _wishR3CapEnabled => Options.WishR3Cap.Enabled.Value;

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

            if (cap < 1)
                return 1L;

            return (long)cap;
        };

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) => _offlineInProgress = true;
            Plugin.OnOfflineProgressionComplete += (o, e) => _offlineInProgress = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishPodUIController), "selectThisWish")]
        private static void WishPodUIController_selectThisWish_postfix(WishPodUIController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                ClearSelected();
                return;
            }

            var id = __instance.id;

            if (_selectedIds.Contains(id))
                _selectedIds.Remove(id);

            else if (_selectedIds.Count < MaxWishes)
                _selectedIds.Add(id);

            __instance.updateIcon();
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
            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                var wish = Wishes.AllWishes[__instance.curSelectedWish];
                if (wish.Level >= wish.MaxLevel)
                {
                    Plugin.ShowNotification("Wish is at max level, why are you allocating resources?");
                    return false;
                }

                return true;
            }

            IEnumerable<WishWrapper> wishes;

            if (_selectedIds.Count > 0)
                wishes = _selectedIds.Select(id => Wishes.AllWishes[id]);
            else
                wishes = Wishes.PartiallyRunningWishes;

            SplitResources(wishes.ToList());

            ClearSelected();
            __instance.updateText();

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "doLevelupEffect")]
        private static void WishesController_doLevelupEffect_postfix(int id, int level, WishesController __instance)
        {
            if (_offlineInProgress || level >= __instance.maxWishLevel(id)) // code in wishlist handles max level
                return;

            if (_wishListEnabled && (_wishListSingleLevelMode || WishList.WishTargetReached(id, level)))
            {
                WishList.ClearAndStartNextWish(id);
                return;
            }

            RedistributeR3();
            __instance.updateText();
        }

        internal static void SplitResources()
        {
            SplitResources(Wishes.RunningWishes.ToList());
        }

        internal static void SplitResources(List<WishWrapper> wishes)
        {
            var count = wishes.Count();
            if (count == 0)
                return;

            Plugin.Character.wishesController.removeAllResources();

            var eSplit = IdleEnergy / count;
            var mSplit = IdleMagic / count;
            var r3Split = IdleRes3 / count;

            foreach (var wish in wishes)
            {
                wish.Energy = eSplit;
                wish.Magic = mSplit;
                wish.Res3 = r3Split;

                IdleEnergy -= eSplit;
                IdleMagic -= mSplit;
                IdleRes3 -= r3Split;
            }

            RedistributeR3();
        }

        internal static void RedistributeR3()
        {
            if (!_wishR3CapEnabled)
                return;

            var runningWishes = Wishes.RunningWishes
                .Select(w => new { w, cap = wishR3Cap(w.Id) })
                .OrderBy(w => w.cap)
                .ToList();

            var character = Plugin.Character;
            character.wishesController.removeAllRes3();
            var amountLeft = character.res3.idleRes3;

            while (runningWishes.Count > 0)
            {
                var rw = runningWishes[0];
                var amount = Math.Min(amountLeft / runningWishes.Count, rw.cap);
                if (amount > amountLeft)
                    amount = amountLeft;

                rw.w.Res3 += amount;
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
