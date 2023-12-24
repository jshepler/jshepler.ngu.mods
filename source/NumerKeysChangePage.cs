using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class NumerKeysChangePage
    {
        private const int WISH_PODS_PER_PAGE = 21;

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "Update")]
        private static bool Character_Update_prefix(Character __instance)
        {
            int numberKey;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                numberKey = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                numberKey = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                numberKey = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                numberKey = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                numberKey = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                numberKey = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                numberKey = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                numberKey = 8;
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                numberKey = 9;
            else
                return true;

            switch (__instance.CurrentMenu())
            {
                case Menu.Yggdrasil:
                    if (numberKey < 4)
                    {
                        __instance.yggdrasilController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.GoldDiggers:
                    if (numberKey < 4)
                    {
                        __instance.allDiggers.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.Beards:
                    if (numberKey < 8)
                    {
                        __instance.allBeards.beard.changeID(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.Hacks:
                    if (numberKey < 3)
                    {
                        __instance.hacksController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                case Menu.Wishes:
                    var lastPage = __instance.wishesController.curValidUpgradesList.Count / WISH_PODS_PER_PAGE;
                    if (numberKey - 1 <= lastPage)
                    {
                        __instance.wishesController.changePage(numberKey - 1);
                        return false;
                    }

                    return true;


                default:
                    return true;
            }
        }
    }
}
