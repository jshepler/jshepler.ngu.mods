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
    // But input fields aren't the only selectable and this game has over 1k selectables in the scene, even
    // if only displaying a portion of them. The method above works until viewing different feature pages, then
    // it would start selecting fields in pages other than the current page. The code started getting pretty messy,
    // especially the wrap-around code when tabbing past the first/last field, so I put it on hold and tried a different approach.
    //
    // Building a list of fields as they are created, sort them in order of transform.position.y, allowed me to
    // be very specific in the fields intead of relying on Unity to find the next/previous field on the screen.

    [HarmonyPatch]
    internal class TabNavigation
    {
        private static EventSystem _system;
        private static Dictionary<Menu, List<InputField>> _fields = new();
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
            if (!_fields.ContainsKey(Menu.Augments))
                _fields.Add(Menu.Augments, new());

            _fields[Menu.Augments].Add(__instance.augmentTarget);
            _fields[Menu.Augments].Add(__instance.upgradeTarget);

            __instance.augmentTarget.navigation = Navigation.defaultNavigation;
            __instance.upgradeTarget.navigation = Navigation.defaultNavigation;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "Start")]
        private static void AdvancedTrainingController_Start_postfix(AdvancedTrainingController __instance)
        {
            if (!_fields.ContainsKey(Menu.AdvancedTraining))
                _fields.Add(Menu.AdvancedTraining, new());

            _fields[Menu.AdvancedTraining].Add(__instance.target);
            __instance.target.navigation = Navigation.defaultNavigation;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "Start")]
        private static void NGUController_Start_postfix(NGUController __instance)
        {
            if (!_fields.ContainsKey(Menu.NGU_Energy))
                _fields.Add(Menu.NGU_Energy, new());

            _fields[Menu.NGU_Energy].Add(__instance.target);
            __instance.target.navigation = Navigation.defaultNavigation;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "Start")]
        private static void NGUMagicController_Start_postfix(NGUMagicController __instance)
        {
            if (!_fields.ContainsKey(Menu.NGU_Magic))
                _fields.Add(Menu.NGU_Magic, new());

            _fields[Menu.NGU_Magic].Add(__instance.magicTarget);
            __instance.magicTarget.navigation = Navigation.defaultNavigation;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            _fields.Add(Menu.TimeMachine, [__instance.speedTarget, __instance.multiTarget]);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllGoldDiggerController), "Start")]
        private static void AllGoldDiggerController_Start_postfix(AllGoldDiggerController __instance)
        {
            _fields.Add(Menu.GoldDiggers, __instance.pods.Select(p => p.diggerLevelInput).ToList());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HacksController), "Start")]
        private static void HacksController_Start_postfix(HacksController __instance)
        {
            _fields.Add(Menu.Hacks, __instance.pods.Select(p => p.target).ToList());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnergyPurchases), "Start")]
        private static void EnergyPurchaes_Start_postfix(EnergyPurchases __instance)
        {
            _fields.Add(Menu.EXP_Energy, [__instance.powerInput, __instance.capInput, __instance.barInput]);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MagicPurchases), "Start")]
        private static void MagicPurchases_Start_postfix(MagicPurchases __instance)
        {
            _fields.Add(Menu.EXP_Magic, [__instance.powerInput, __instance.capInput, __instance.barInput]);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Resource3Purchases), "Start")]
        private static void Resource3Purchases_Start_postfix(Resource3Purchases __instance)
        {
            _fields.Add(Menu.EXP_R3, [__instance.powerInput, __instance.capInput, __instance.barInput]);
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (_system == null || _system.currentSelectedGameObject == null || !Input.GetKeyDown(KeyCode.Tab))
                return;

            var current = _system.currentSelectedGameObject.GetComponent<Selectable>();
            if (current == null)
                return;

            if (!_sorted) sortLists();

            //var list = Plugin.Character.CurrentMenu() switch
            //{
            //    Menu.Augments => _augFields,
            //    Menu.AdvancedTraining => _atFields,
            //    Menu.TimeMachine => _tmFields,
            //    Menu.NGU_Energy => _eNguFields,
            //    Menu.NGU_Magic => _mNguFields,
            //    Menu.GoldDiggers => _diggerFields,
            //    Menu.Hacks => _hacksFields,
            //    _ => null
            //};

            //if (list == null)
            //    return; // not in a supported menu

            var menu = Plugin.Character.CurrentMenu();
            if (!_fields.ContainsKey(menu))
                return;

            var list = _fields[menu];

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
            // this works, just experimenting with different ways to sort
            //var comparer = new FieldOrderComparer();
            //_augFields = _augFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_atFields = _atFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_eNguFields = _eNguFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_mNguFields = _mNguFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_diggerFields = _diggerFields.OrderBy(i => i.transform.position, comparer).ToList();
            //_hacksFields = _hacksFields.OrderBy(i => i.transform.position, comparer).ToList();

            //_augFields.Sort(CompareFieldPosition);
            //_atFields.Sort(CompareFieldPosition);
            //_eNguFields.Sort(CompareFieldPosition);
            //_mNguFields.Sort(CompareFieldPosition);
            //_diggerFields.Sort(CompareFieldPosition);
            //_hacksFields.Sort(CompareFieldPosition);

            foreach (var l in _fields.Values)
                l.Sort(CompareFieldPosition);

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
