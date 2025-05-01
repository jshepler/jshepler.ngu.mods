using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class WishEnergyAllocator : BaseAllocator
    {
        internal static WishEnergyAllocator Instance = new();
        private static WishesController _controller;

        private static bool _shiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static bool _altHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private static bool _ctrlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        //private static bool _allSlotsUsed => _controller.numAllocatedWishes() >= _controller.curWishSlots();
        private static bool _allSlotsUsed => Math.Max(_controller.numAllocatedWishes(), Instance.EnabledIDs.Count()) >= _controller.curWishSlots();

        internal WishEnergyAllocator() : base(231, 1)
        {
            Allocators.Energy.Add(Allocators.Feature.WishEnergy, this);
        }

        internal override void Allocate(int id, long amount)
        {
            var wish = Wishes.AllWishes[id];
            var was0 = wish.Energy == 0;

            wish.Energy += amount;
            _controller.updateEnergyPodText();

            if(was0 || wish.Energy == 0)
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

            var texts = new List<Text>();
            var root = GameObject.Find("Canvas/Wishes Canvas /Wishes Menu/Inputs Pod/").transform;
            foreach (Transform child in root)
                if (child.name == "+ Button")
                    texts.Add(child.GetComponentInChildren<Text>());

            Instance.TextComponents[0] = texts[0];
            WishMagicAllocator.Instance.TextComponent = texts[1];
            WishRes3Allocator.Instance.TextComponent = texts[2];

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

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "updateEnergyPodText")]
        private static void WishesController_updateEnergyPodText_postfix(WishesController __instance)
        {
            Instance.UpdateButton(__instance.curSelectedWish);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "selectNewWish")]
        private static void WishesController_selectNewWish_postfix(int id)
        {
            Instance.UpdateButton(id);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "addEnergy", typeof(int))]
        private static bool WishesController_addEnergy_prefix(int id)
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
                WishMagicAllocator.toggleAll();
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
            HarmonyPatch(typeof(WishesController), "removeEnergy", typeof(int)),
            HarmonyPatch(typeof(WishesController), "removeAllEnergy", typeof(int))]
        private static void WishesController_removeEnergy_postfix(int id)
        {
            Instance[id] = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "clearSelectedWish")]
        private static void WishesController_clearSelectedWish_postfix(WishesController __instance)
        {
            if (_shiftHeld)
            {
                Instance.DisableAll();
                WishMagicAllocator.Instance.DisableAll();
                WishRes3Allocator.Instance.DisableAll();
            }
            else
            {
                var wishId = _controller.curSelectedWish;
                Instance[wishId] = false;
                WishMagicAllocator.Instance[wishId] = false;
                WishRes3Allocator.Instance[wishId] = false;
            }
        }
    }
}
