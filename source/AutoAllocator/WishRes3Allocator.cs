using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class WishRes3Allocator : BaseAllocator
    {
        internal static WishRes3Allocator Instance = new();
        internal Text TextComponent { set { TextComponents[0] = value; } }
        private static WishesController _controller;

        private static bool _shiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static bool _altHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private static bool _ctrlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        private static bool _allSlotsUsed => _controller.numAllocatedWishes() >= _controller.curWishSlots();

        public WishRes3Allocator() : base(231, 1)
        {
            Allocators.Res3.Add(Allocators.Feature.WishRes3, this);
        }

        internal override void Allocate(int id, long amount)
        {
            Wishes.AllWishes[id].Res3 += amount;
            _controller.updateRes3PodText();
        }

        internal override long CalcCapDelta(int id)
        {
            var cap = WishSplit.GetWishR3Cap(id);
            var current = Wishes.AllWishes[id].Res3;

            var delta = cap - current;
            if (current + delta < 0)
                delta = -current;

            return delta;
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

        protected override void UpdateText(int id)
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

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "updateRes3PodText")]
        private static void WishesController_updateRes3PodText_postfix(WishesController __instance)
        {
            Instance.UpdateText(__instance.curSelectedWish);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "selectNewWish")]
        private static void WishesController_selectNewWish_postfix(int id)
        {
            Instance.UpdateText(id);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "addRes3", typeof(int))]
        private static bool WishesController_addRes3_prefix(int id)
        {
            if (!_shiftHeld)
                return true;

            if (_altHeld && _ctrlHeld)
            {
                toggleAll();
                WishEnergyAllocator.toggleAll();
                WishMagicAllocator.toggleAll();
            }

            else if (_altHeld)
                toggleAll();

            else
                toggleOne(id);

            Instance.UpdateText(id);
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
            HarmonyPatch(typeof(WishesController), "removeRes3", typeof(int)),
            HarmonyPatch(typeof(WishesController), "removeAllRes3", typeof(int))]
        private static void WishesController_removeRes3_postfix(int id)
        {
            Instance[id] = false;
        }
    }
}
