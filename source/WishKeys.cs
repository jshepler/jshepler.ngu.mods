using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishKeys
    {
        private const int PODSPERPAGE = 21;
        private const int PODSPERROW = 7;

        private enum MoveDirection { Up, Down, Left, Right };
        private static WishesController _controller;

        // (bugfix) the change page code doesn't update the pageID field
        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "changePage")]
        private static void WishesController_changePage_prefix(int newPage, WishesController __instance)
        {
            __instance.pageID = newPage;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            _controller = __instance;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!e.Character.InMenu(Menu.Wishes))
                    return;

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    MoveSelectedWish(MoveDirection.Left);

                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    MoveSelectedWish(MoveDirection.Right);

                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    MoveSelectedWish(MoveDirection.Up);

                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    MoveSelectedWish(MoveDirection.Down);

                // these are now handled in NumberKeysChangePage.cs
                //else if (Input.GetKeyDown(KeyCode.Alpha1))
                //    ChangePage(0);

                //else if (Input.GetKeyDown(KeyCode.Alpha2))
                //    ChangePage(1);

                //else if (Input.GetKeyDown(KeyCode.Alpha3))
                //    ChangePage(2);

                //else if (Input.GetKeyDown(KeyCode.Alpha4))
                //    ChangePage(3);

                //else if (Input.GetKeyDown(KeyCode.Alpha5))
                //    ChangePage(4);

                //else if (Input.GetKeyDown(KeyCode.Alpha6))
                //    ChangePage(5);

                //else if (Input.GetKeyDown(KeyCode.Alpha7))
                //    ChangePage(6);

                //else if (Input.GetKeyDown(KeyCode.Alpha8))
                //    ChangePage(7);

                //else if (Input.GetKeyDown(KeyCode.Alpha9))
                //    ChangePage(8);
            };
        }

        private static void MoveSelectedWish(MoveDirection direction)
        {
            var list = _controller.curValidUpgradesList;
            var curPage = _controller.pageID;
            var firstIndex = curPage * PODSPERPAGE;
            
            var lastIndex = firstIndex + PODSPERPAGE - 1;
            if (lastIndex >= list.Count)
                lastIndex = list.Count - 1;

            var curWish = _controller.curSelectedWish;
            var curIndex = list.IndexOf(curWish);
            var newIndex = firstIndex;
            
            if (curIndex >= firstIndex && curIndex <= lastIndex)
            {
                var curRow = curIndex % PODSPERPAGE / PODSPERROW;
                var lastRow = lastIndex % PODSPERPAGE / PODSPERROW;

                var newIndexFromRow = (int row) =>
                {
                    var rowIndex = curIndex % PODSPERPAGE % PODSPERROW;
                    var newIndex = firstIndex + (row * PODSPERROW) + rowIndex;

                    // if there is no wish on the new row at the same row index, don't change rows
                    if (newIndex >= list.Count)
                        return curIndex;

                    return newIndex;
                };

                newIndex = direction switch
                {
                    MoveDirection.Left => curIndex == firstIndex ? lastIndex : curIndex - 1,
                    MoveDirection.Right => curIndex == lastIndex ? firstIndex : curIndex + 1,
                    MoveDirection.Up => curRow == 0 ? newIndexFromRow(lastRow) : newIndexFromRow(curRow - 1),
                    MoveDirection.Down => curRow == lastRow ? newIndexFromRow(0) : newIndexFromRow(curRow + 1),
                    _ => 0
                };
            }

            // shouldn't be possible to happen, but just in case...
            if (newIndex < 0)
                newIndex = 0;
            else if (newIndex >= list.Count)
                newIndex = list.Count - 1;

            _controller.selectNewWish(list[newIndex]);
        }

        //private static void ChangePage(int newPage)
        //{
        //    var lastPage = _controller.curValidUpgradesList.Count / PODSPERPAGE;
        //    if (newPage <= lastPage)
        //        _controller.changePage(newPage);
        //}
    }
}
