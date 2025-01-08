using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RatioCalc
    {
        private static RatioCalcPopup _popup = new RatioCalcPopup();

        internal static Dictionary<Menu, PCB> Ratios;

        [HarmonyPatch, HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                var pcb = ModSave.Data.PCBRatios;
                Ratios = new()
                {
                    { Menu.EXP_Energy, new(pcb[0]) },
                    { Menu.EXP_Magic, new(pcb[1]) },
                    { Menu.EXP_R3, new(pcb[2]) }
                };
            };

            Plugin.OnPreSave += (o, e) =>
            {
                ModSave.Data.PCBRatios = Ratios.Values.Select(v => new[] { (int)v.power, (int)v.cap, (int)v.bars }).ToArray();
            };

            Plugin.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (!Input.GetKeyDown(KeyCode.F1))
                return;

            var menu = Plugin.Character.CurrentMenu();
            if (menu != Menu.EXP_Energy && menu != Menu.EXP_Magic && menu != Menu.EXP_R3)
                return;

            if (_popup.IsOpen)
                _popup.Close();
            else
                _popup.Open(menu);
        }
    }

    internal class PCB
    {
        public PCB() { }

        public PCB(int[] ar)
        {
            power = ar[0];
            cap = ar[1];
            bars = ar[2];
        }

        public PCB(long p, long c, long b)
        {
            power = p;
            cap = c;
            bars = b;
        }

        internal long power;
        internal long cap;
        internal long bars;
    }
}
