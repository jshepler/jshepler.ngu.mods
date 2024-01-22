using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;
using UnityEngine.TextCore;

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
        // this transpiler replaces the call to removeAllResources with a call to CheckQueue below
        [HarmonyTranspiler, HarmonyPatch(typeof(WishesController), "updateAllWishes")]
        private static IEnumerable<CodeInstruction> WishesController_updateAllWishes_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var removeAllResourcesMethod = typeof(WishesController).GetMethod("removeAllResources", new[] { typeof(int) });

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
                .SetInstruction(Transpilers.EmitDelegate(CheckQueue));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static void CheckQueue(int currentId)
        {
            if (Queue.Count == 0)
            {
                _controller.removeAllResources(currentId);
                return;
            }

            var nextId = Queue[0];
            Queue.RemoveAt(0);

            var current = _controller.character.wishes.wishes[currentId];
            var next = _controller.character.wishes.wishes[nextId];

            // if next is completed, skip it, but instead of a while loop, trying a recursion thing, just because
            if (next.level >= _controller.properties[nextId].maxLevel)
            {
                CheckQueue(currentId);
                return;
            }

            next.energy += current.energy;
            next.magic += current.magic;
            next.res3 += current.res3;
            _controller.updatebyID(nextId);

            current.energy = 0;
            current.magic = 0;
            current.res3 = 0;
            // no need to do updateByID for current, the patched method will do it after return
        }
    }
}
