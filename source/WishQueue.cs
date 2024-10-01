using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishQueue
    {
        internal static List<int> Queue = new();
        private static WishesController _controller;
        private static WishQueuePopup _popup;

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _controller = __instance;
            _popup = new WishQueuePopup(__instance.character);

            Plugin.OnSaveLoaded += (o, e) => Queue = ModSave.Data.WishQueue ?? new();
            Plugin.OnPreSave += (o, e) => ModSave.Data.WishQueue = Queue;

            Plugin.OnOfflineProgressionComplete += OnOfflineProgressionComplete;

            Plugin.OnUpdate += (o, e) =>
            {
                if (Plugin.Character.InMenu(Menu.Wishes) && Input.GetKeyDown(KeyCode.Q))
                {
                    _popup.Toggle();
                }
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "selectNewWish")]
        private static bool WishesController_selectNewWish_pretfix(int id, WishesController __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            if (Queue.Contains(id))
            {
                Queue.Remove(id);
                Plugin.ShowOverrideNotification($"removed wishId {id} from queue", 1);
            }
            else
            {
                Queue.Add(id);
                Plugin.ShowOverrideNotification($"added wishId {id} to queue", 1);
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishPodUIController), "updateIcon")]
        private static void WishPodUIController_updateIcon_prefix(WishPodUIController __instance)
        {
            if (__instance.character.menuID != 53 || __instance.invalidID())
                return;

            if (Queue.Contains(__instance.id))
                __instance.wishBorder.sprite = __instance.character.wishesController.goldBorder;
        }

        // in WishesController.updateAllWishes(), when a wish reaches max level, removeAllResources is called;
        // this transpiler replaces the call to removeAllResources with a call to StartNextWish below
        [HarmonyTranspiler, HarmonyPatch(typeof(WishesController), "updateAllWishes")]
        private static IEnumerable<CodeInstruction> WishesController_updateAllWishes_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var removeAllResourcesMethod = typeof(WishesController).GetMethod("removeAllResources", [typeof(int)]);

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, removeAllResourcesMethod));

            if (cm.IsInvalid)
            {
                Plugin.LogInfo("call to removeAllResources not found, not patching");
                return instructions;
            }

            cm.Advance(-2)
                .RemoveInstruction() // removes ldarg.0
                .Advance(1)
                .SetInstruction(Transpilers.EmitDelegate(StartNextWish));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static void OnOfflineProgressionComplete(object sender, EventArgs e)
        {
            if (Options.WisheQueue.Enabled.Value == false || Queue.Count == 0)
                return;

            var wishes = Plugin.Character.wishes.wishes;
            var runningWishes = wishes.Where(w => w.energy > 0 && w.magic > 0 && w.res3 > 0).ToList();
            var maxWishes = Plugin.Character.wishesController.curWishSlots();

            if (runningWishes.Count >= maxWishes)
                return;

            while (runningWishes.Count < maxWishes)
            {
                var nextWishId = GetNextWishId();
                if (nextWishId == -1)
                    break;

                runningWishes.Add(wishes[nextWishId]);
            }

            WishSplit.SplitResources(runningWishes);
        }

        private static void StartNextWish(int wishId)
        {
            var nextWishId = Options.WisheQueue.Enabled.Value ? GetNextWishId() : -1;

            if (nextWishId == -1)
            {
                _controller.removeAllResources(wishId);
                WishSplit.SplitResources();
                _controller.updateText();

                return;
            }

            var current = _controller.character.wishes.wishes[wishId];
            var next = _controller.character.wishes.wishes[nextWishId];

            next.energy += current.energy;
            next.magic += current.magic;
            next.res3 += current.res3;

            current.energy = 0;
            current.magic = 0;
            current.res3 = 0;

            WishSplit.RedistributeR3();

            _controller.updateText();
            _controller.updatebyID(nextWishId);
        }

        private static int GetNextWishId()
        {
            var wishes = _controller.character.wishes.wishes;
            var props = _controller.properties;
            var nextWishId = -1;

            while (Queue.Count > 0)
            {
                var index = Queue[0];
                Queue.RemoveAt(0);

                if (wishes[index].level < props[index].maxLevel)
                {
                    nextWishId = index;
                    break;
                }
            }

            if (nextWishId == -1)
            {
                _controller.constructList();
                for (var x = 0; x < _controller.curValidUpgradesList.Count; x++)
                {
                    var index = _controller.curValidUpgradesList[x];
                    var wish = wishes[index];
                    if (wish.level < props[index].maxLevel && wish.energy == 0 && wish.magic == 0 && wish.res3 == 0)
                    {
                        nextWishId = index;
                        break;
                    }
                }
            }

            return nextWishId;
        }
    }
}
