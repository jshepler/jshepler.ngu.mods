using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishList
    {
        private static DateTime _lastAutoSave = DateTime.UtcNow;

        private static List<int> _wishList => Data.WishList;
        private static List<int> _wishTargets => Data.WishTargets;
        private static List<int> _lastRunning => Data.WishesLastRunning;

        private static WishesController _controller;
        private static WishListPopup _popup;

        private static bool _wishListEnabled => Options.WishList.Enabled.Value;
        private static bool _autoAdvance => Options.WishList.AutoAdvance.Value;
        private static bool _singleLevelMode => Options.WishList.SingleLevelMode.Value;
        private static bool _blacklistMode => Options.WishList.BlacklistMode.Value;
        private static int _maxWishSlots => Plugin.Character.wishesController.curWishSlots();

        private static int _offlineCount;

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _controller = __instance;
            _popup = new WishListPopup();

            Plugin.OnSaveLoaded += (o, e) =>
            {
                if (Data.Values.ContainsKey("WishQueue"))
                {
                    _wishList.Clear();
                    _wishList.AddRange((List<int>)Data.Values["WishQueue"]);
                    Data.Values.Remove("WishQueue");
                }

                if (_wishTargets.Count == _wishList.Count)
                    return;

                _wishTargets.Clear();
                _wishTargets.AddRange(_wishList.Select(id => 0));

                _offlineCount = Wishes.RunningWishes.Count();
            };

            Plugin.OnOfflineProgressionComplete += (o, e) =>
            {
                // select first visible wish when loading a save
                _controller.constructList();
                if (_controller.curValidUpgradesList.Count > 0)
                {
                    _controller.changePage(0);
                    _controller.selectNewWish(_controller.curValidUpgradesList[0]);
                }

                FillOpenWishSlots(_offlineCount);
            };

            Plugin.OnUpdate += (o, e) =>
            {
                if (Plugin.Character.InMenu(Menu.Wishes) && Input.GetKeyDown(KeyCode.F1))
                    _popup.Toggle();

                if (Wishes.RunningWishes.Any(w => w.Progress >= 0.999f)
                    && (DateTime.UtcNow - _lastAutoSave).TotalMinutes >= 3)
                {
                    AutoSaves.DoSave("Wishes_About_To_Level");
                    _lastAutoSave = DateTime.UtcNow;
                }
            };

            Plugin.OnLateUpdate += (o, e) => updateTracked();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "selectNewWish")]
        private static bool WishesController_selectNewWish_pretfix(int id)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;

            if (_wishList.Contains(id))
                removeWish(id);
            else
                addWish(id);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishPodUIController), "updateIcon")]
        private static void WishPodUIController_updateIcon_prefix(WishPodUIController __instance)
        {
            if (__instance.character.menuID != 53 || __instance.invalidID())
                return;

            if (_wishList.Contains(__instance.id))
            {
                if (_blacklistMode)
                    __instance.wishIcon.color = Plugin.ButtonColor_Red;
                else if (__instance.id != _controller.curSelectedWish)
                    __instance.wishBorder.sprite = __instance.character.wishesController.goldBorder;
            }
        }

        // in WishesController.updateAllWishes(), when a wish reaches max level, removeAllResources is called;
        // this transpiler replaces the call to removeAllResources with a call to ClearAndStartNextWish below
        [HarmonyTranspiler, HarmonyPatch(typeof(WishesController), "updateAllWishes")]
        private static IEnumerable<CodeInstruction> WishesController_updateAllWishes_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var removeAllResourcesMethod = typeof(WishesController).GetMethod("removeAllResources", [typeof(int)]);

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, removeAllResourcesMethod))
                .Advance(-2)
                .RemoveInstruction()
                .Advance(1)
                .SetInstruction(Transpilers.EmitDelegate(ClearAndStartNextWish));

            return cm.InstructionEnumeration();
        }

        internal static void ClearAndStartNextWish(int wishId)
        {
            // this is what the transpiler above replaces - still need to do it
            _controller.removeAllResources(wishId);

            // if wish is in list and hits max level or target, remove it
            var wish = Wishes.AllWishes[wishId];
            var qIndex = _wishList.IndexOf(wishId);

            if (qIndex >= 0)
            {
                if (wish.Level == wish.MaxLevel || WishTargetReached(wish.Id, wish.Level))
                    removeWishAt(qIndex);
                else
                    _popup.MoveBottom(qIndex);
            }

            // if a new wish isn't started, then split resources into remaining wishes
            if (!startNextWish())
                WishSplit.SplitResources();
        }

        internal static void FillOpenWishSlots(int maxSlots = 4)
        {
            if (_controller.character.wishes.wishesOn == false || !_wishListEnabled)
                return;

            var c = _controller.character;
            if (c.idleEnergy == 0 || c.magic.idleMagic == 0 || c.res3.idleRes3 == 0)
                return;

            var slotsToFill = Math.Min(maxSlots, _maxWishSlots);
            var running = Wishes.RunningWishes.Count();
            if (running >= slotsToFill)
                return;

            for(var x = running; x < slotsToFill; x++)
                if (!startNextWish())
                    break;

            _controller.updateMenu();
        }

        internal static void ResumeWishes()
        {
            _controller.removeAllResources();
            var tracked = _lastRunning.Select(id => Wishes.AllWishes[id]).ToList();
            WishSplit.SplitResources(tracked);
            _controller.updateMenu();
        }

        private static bool startNextWish()
        {
            if (!_autoAdvance)
                return false;

            var nextWish = getNextWish();
            if (nextWish == null)
                return false;

            var running = Wishes.RunningWishes.ToList();

            // this should never happen, but just in case...
            if (running.Count >= _maxWishSlots)
                return false;

            running.Add(nextWish);
            WishSplit.SplitResources(running);

            _controller.selectNewWish(nextWish.Id);
            _controller.updateMenu();

            return true;
        }

        private static WishWrapper getNextWish()
        {
            _controller.constructList();

            if (!_wishListEnabled)
                return null;

            var wish = getNextWishFromList();
            if (wish == null)
                wish = Wishes.CurValidUpgradesList.FirstOrDefault(w => w.Level < w.MaxLevel && !w.IsRunning && (!_blacklistMode || !_wishList.Contains(w.Id)));

            return wish;
        }

        private static WishWrapper getNextWishFromList()
        {
            if (_blacklistMode || _wishList.Count == 0)
                return null;

            return _wishList.Select(i => Wishes.AllWishes[i]).FirstOrDefault(w => !w.IsRunning);
        }

        private static WishWrapper getNextWishFromList_old2(int startIndex)
        {
            if (_blacklistMode || _wishList.Count == 0)
                return null;

            var list = _wishList.Select(i => Wishes.AllWishes[i]).ToList();
            
            // if all are running, no new wish to start
            if (list.All(w => w.IsRunning))
                return null;

            // this could be true if the wish that leveled was last in the wish,
            // the code just passes index + 1 w/o checking for end of list
            if (startIndex >= list.Count)
                startIndex = 0;

            // if no wishes are running, start the one passed
            if (list.All(w => !w.IsRunning))
                return list[startIndex];

            // starting at startIndex, find index of last running wish and start the next one
            // ignore the ones before startIndex, unless last running wish is the last wish in the list...
            var lastIndex = list.Count - 1;
            var lastRunningIndex = list.FindLastIndex(lastIndex, lastIndex - startIndex + 1, w => w.IsRunning);

            // if none, then start the one passed
            if (lastRunningIndex == -1)
                return list[startIndex];

            // if not last wish in list, start the one after it
            if (lastRunningIndex < lastIndex)
                return list[lastRunningIndex + 1];

            // ... last running wish is last in list
            // if startIndex is 0, then the whole list was just checked, start first wish
            if (startIndex == 0)
                return list[0];

            // otherwise, do a new search starting from the top
            // should never be -1 as that would mean no wish is running and that was checked above
            lastRunningIndex = list.FindLastIndex(w => w.IsRunning);

            if (lastRunningIndex < lastIndex)
                return list[lastRunningIndex + 1];

            return list[0];
        }

        // getting the next wish when only have a single wish slot is simple,
        // but gets more complicated as more wish slots are unlocked - more wishes running
        // it will be expected to work down the list and there are a number of scenarios to account for
        // e.g if indexes 0 and 1 are running and 1 finishes, 2 starts, then when 0 ends, need to skip
        // over 1 and 2 to start 3 as that would be the next wish in the list to run
        // so basically find the last running wish and start the one after
        // but also, when the end is reached, start back at the top and skip over any running wishes to
        // get to the first non-running wish and start that
        // so need to look at wishes >= startIndex and > last running wish
        // if no more wishes in list, then start back at top
        private static WishWrapper getNextWishFromList_old1(int startIndex)
        {
            if (_blacklistMode || _wishList.Count == 0)
                return null;

            var list = _wishList.Select(i => Wishes.AllWishes[i]).ToList();
            if (list.All(w => w.IsRunning))
                return null;

            if (startIndex >= list.Count)
                startIndex = 0;

            // if wish at startIndex is running, find the next non-running one
            var firstNotRunningIndex = list.FindIndex(startIndex, w => !w.IsRunning);
            if (firstNotRunningIndex == -1 && startIndex > 0)
                firstNotRunningIndex = list.FindIndex(w => !w.IsRunning);

            // should never be -1 at this point as that would indicate all wishes in list are running,
            // which was already checked above so this is a "just in case" thing
            if (firstNotRunningIndex == -1)
                return null;

            // new startIndex will be the first non-running wish
            startIndex = firstNotRunningIndex;

            // the basic idea is to find the last running wish and start the one after it
            // if the last wish in the list is running, don't assume to start list[0] - it might be running
            for (var lastIndex = list.Count - 1; lastIndex >= 0; lastIndex--)
            {
                //var lastIndex = list.Count - x - 1;
                var lastRunningIndex = list.FindLastIndex(lastIndex, lastIndex + 1, w => w.IsRunning);

                // keep reducing lastIndex until the last running wish isn't the last wish
                if (lastRunningIndex == lastIndex)
                    continue;

                if (lastRunningIndex == -1)
                {
                    if (startIndex > lastIndex)
                        return list[0];

                    return list[firstNotRunningIndex];
                }

                return list[lastRunningIndex + 1];
            }

            // should never reach this unless all wishes in the list are running, which was checked earlier
            // but need this so the compiler doesn't complain about "not all code paths return a value"
            return null;
        }

        private static void addWish(int wishId)
        {
            _wishList.Add(wishId);
            _wishTargets.Add(0);
        }

        private static void removeWish(int wishId)
        {
            var index = _wishList.IndexOf(wishId);

            if (index >= 0)
                removeWishAt(index);
        }

        private static void removeWishAt(int index)
        {
            _wishList.RemoveAt(index);
            _wishTargets.RemoveAt(index);
        }

        // Tracked is used to resume wishes after being stopped for whatever reason
        // which means don't want to stop tracking wishes just because they're no longer running
        // starting a new wish will replace a tracked wish that's no longer running, but also need to
        // account for when max wish slots increase - new wish can be added instead of replacing
        private static void updateTracked()
        {
            var nowRunning = Wishes.RunningWishes.Select(w => w.Id).ToList();
            var notTracked = nowRunning.Where(nr => !_lastRunning.Contains(nr)).ToList();

            while (_lastRunning.Count < _maxWishSlots && notTracked.Count > 0)
            {
                _lastRunning.Add(notTracked[0]);
                notTracked.RemoveAt(0);
            }

            while (notTracked.Count > 0)
            {
                for (var x = 0; x < _lastRunning.Count; x++)
                {
                    if (!nowRunning.Contains(_lastRunning[x]))
                    {
                        _lastRunning[x] = notTracked[0];
                        notTracked.RemoveAt(0);
                        break;
                    }
                }
            }
        }

        internal static bool WishTargetReached(int wishId, int level)
        {
            var index = _wishList.IndexOf(wishId);
            if (index == -1 || _wishTargets[index] == 0 || level < _wishTargets[index])
                return false;

            return true;
        }
    }
}
