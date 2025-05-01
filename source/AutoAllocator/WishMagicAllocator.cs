using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class WishMagicAllocator : BaseAllocator
    {
        internal static WishMagicAllocator Instance = new();
        internal Text TextComponent { set { TextComponents[0] = value; } }
        private static WishesController _controller;

        private static bool _shiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static bool _altHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private static bool _ctrlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        //private static bool _allSlotsUsed => _controller.numAllocatedWishes() >= _controller.curWishSlots();
        private static bool _allSlotsUsed => Math.Max(_controller.numAllocatedWishes(), Instance.EnabledIDs.Count()) >= _controller.curWishSlots();

        public WishMagicAllocator() : base(231, 1)
        {
            Allocators.Magic.Add(Allocators.Feature.WishMagic, this);
        }

        internal override void Allocate(int id, long amount)
        {
            var wish = Wishes.AllWishes[id];
            var was0 = wish.Magic == 0;

            wish.Magic += amount;
            _controller.updateMagicPodText();

            if(was0 || wish.Magic == 0)
                _controller.updatebyID(id);
        }

        internal override long CalcCapDelta(int id)
        {
            return long.MaxValue;
        }

        internal override bool IsTargetReached(int id)
        {
            var wish = Wishes.AllWishes[id];
            return wish.Level >= wish.MaxLevel || WishList.WishTargetReached(wish);
        }

        protected override Text GetTextComponent(int id)
        {
            return TextComponents[0];
        }

        protected override void UpdateButton(int id)
        {
            TextComponents[0].text = id == _controller.curSelectedWish && this[id] ? "++" : "+";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _controller = __instance;

            List<int> wasEnabled = new();
            Plugin.OnSaveLoaded2 += (o, e) =>
            {
                wasEnabled = Instance.EnabledIDs.ToList();
            };

            Plugin.OnOfflineProgressionComplete += (o, e) =>
            {
                var nowEnabled = Instance.EnabledIDs.ToList();
                if (nowEnabled.Count == wasEnabled.Count)
                    return;

                var toEnable = Wishes.RunningWishes
                    .Select(w => w.Id)
                    .Except(nowEnabled)
                    .Take(wasEnabled.Count - nowEnabled.Count);

                toEnable.Do(i => Instance[i] = true);
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "updateMagicPodText")]
        private static void WishesController_updateMagicPodText_postfix(WishesController __instance)
        {
            Instance.UpdateButton(__instance.curSelectedWish);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "selectNewWish")]
        private static void WishesController_selectNewWish_postfix(int id)
        {
            Instance.UpdateButton(id);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "addMagic", typeof(int))]
        private static bool WishesController_addMagic_prefix(int id)
        {
            if (!_shiftHeld)
                return true;

            if (Wishes.AllWishes[id].IsLocked(out var message))
            {
                Plugin.ShowOverrideNotification(message);
                return false;
            }

            if (_altHeld && _ctrlHeld)
            {
                toggleAll();
                WishEnergyAllocator.toggleAll();
                WishRes3Allocator.toggleAll();
            }

            else if (_altHeld)
                toggleAll();

            else
                toggleOne(id);

            Instance.UpdateButton(id);
            return false;
        }

        internal static void toggleAll()
        {
            foreach (var wish in Wishes.PartiallyRunningWishes)
                toggleOne(wish.Id);
        }

        private static void toggleOne(int wishId)
        {
            if (!Instance[wishId] && !Wishes.AllWishes[wishId].HasAllocations && _allSlotsUsed)
                return;

            Instance[wishId] = !Instance[wishId];
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(WishesController), "removeMagic", typeof(int)),
            HarmonyPatch(typeof(WishesController), "removeAllMagic", typeof(int))]
        private static void WishesController_removeMagic_postfix(int id)
        {
            Instance[id] = false;
        }
    }
}
