using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    // started out trying to use Selectable::FindSelectableOnUp/Down
    // based on code from: https://forum.unity.com/threads/tab-between-input-fields.263779/
    //
    // But input fields aren't the only selectable and this games has over 1k selectables in the scene, even
    // if only displaying a portion of them. The method above works until viewing different feature pages, then
    // it would start seleting fields in pages other than the current one. The code started getting pretty messy,
    // especially the wrap-around code when tabbing past the first/last field, so I put it on hold and tried a different approach.
    //
    // Building a list of fields as they are created, sort them in order of transform.position.y, allowed me to
    // be very specific in the fields intead of relying on Unity to find the next/previous field on the screen.

    [HarmonyPatch]
    internal class TabNavigation
    {
        private static EventSystem _system;
        private static List<InputField> _augFields = new();
        private static List<InputField> _atFields = new();
        private static List<InputField> _tmFields = new();
        private static List<InputField> _eNguFields = new();
        private static List<InputField> _mNguFields = new();
        private static List<InputField> _diggerFields = new();
        private static List<InputField> _hacksFields = new();

        //private static Dictionary<Menu, List<InputField>> _menuFields;

        private static bool _sorted = false;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original == null)
            {
                _system = EventSystem.current;
                Plugin.OnUpdate += OnUpdate;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AugmentController), "Start")]
        private static void AugmentController_Start_postfix(AugmentController __instance)
        {
            __instance.augmentTarget.navigation = Navigation.defaultNavigation;
            _augFields.Add(__instance.augmentTarget);
            
            __instance.upgradeTarget.navigation = Navigation.defaultNavigation;
            _augFields.Add(__instance.upgradeTarget);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "Start")]
        private static void AdvancedTrainingController_Start_postfix(AdvancedTrainingController __instance)
        {
            __instance.target.navigation = Navigation.defaultNavigation;
            _atFields.Add(__instance.target);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "Start")]
        private static void NGUController_Start_postfix(NGUController __instance)
        {
            __instance.target.navigation = Navigation.defaultNavigation;
            _eNguFields.Add(__instance.target);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "Start")]
        private static void NGUMagicController_Start_postfix(NGUMagicController __instance)
        {
            __instance.magicTarget.navigation = Navigation.defaultNavigation;
            _mNguFields.Add(__instance.magicTarget);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            _tmFields.Add(__instance.speedTarget);
            _tmFields.Add(__instance.multiTarget);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "Start")]
        private static void AllGoldDiggerController_Start_postfix(AllGoldDiggerController __instance)
        {
            _diggerFields = __instance.pods.Select(p => p.diggerLevelInput).ToList();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _hacksFields = __instance.pods.Select(p => p.target).ToList();
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (_system == null || _system.currentSelectedGameObject == null || !Input.GetKeyDown(KeyCode.Tab))
                return;

            var current = _system.currentSelectedGameObject.GetComponent<Selectable>();
            if (current == null)
                return;

            if (!_sorted) sortLists();

            var list = Plugin.Character.CurrentMenu() switch
            {
                Menu.Augments => _augFields,
                Menu.AdvancedTraining => _atFields,
                Menu.TimeMachine => _tmFields,
                Menu.NGU_Energy => _eNguFields,
                Menu.NGU_Magic => _mNguFields,
                Menu.GoldDiggers => _diggerFields,
                Menu.Hacks => _hacksFields,
                _ => null
            };

            if (list == null)
                return; // not in a supported menu

            var index = list.IndexOf((InputField)current);
            if (index == -1)
                return; // not in a supported field

            var goUp = Input.GetKey(KeyCode.LeftShift);
            index += goUp ? -1 : 1;

            if (index < 0) index = list.Count - 1;
            else if (index >= list.Count) index = 0;

            // hacks page 2 only has 7 hacks, skip over 8th slot
            if(Plugin.Character.InMenu(Menu.Hacks)
                && Plugin.Character.hacksController.curPage == 1
                && index == 7)
            {
                index = goUp ? 6 : 0;
            }

            list[index].Select();
        }

        private static void sortLists()
        {
            var comparer = new FieldOrderComparer();

            // this works, just experimenting with different ways to sort
            //_augFields = _augFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_atFields = _atFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_eNguFields = _eNguFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_mNguFields = _mNguFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_diggerFields = _diggerFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_hacksFields = _hacksFields.OrderBy(i => i.transform.position, comparer).ToList();

            _augFields.Sort(CompareFieldPosition);
            _atFields.Sort(CompareFieldPosition);
            _eNguFields.Sort(CompareFieldPosition);
            _mNguFields.Sort(CompareFieldPosition);
            _diggerFields.Sort(CompareFieldPosition);
            _hacksFields.Sort(CompareFieldPosition);

            _sorted = true;
        }

        private static int CompareFieldPosition(InputField a, InputField b)
        {
            var posA = a.transform.position;
            var posB = b.transform.position;

            if (posA.y == posB.y)
                return (int)posA.x - (int)posB.x;

            return (int)posB.y - (int)posA.y;
        }

        class FieldOrderComparer : IComparer<Vector3>
        {
            public int Compare(Vector3 x, Vector3 y)
            {
                if (x.y == y.y)
                    return (int)x.x - (int)y.x;

                return (int)y.y - (int)x.y;
            }
        }
    }
}
