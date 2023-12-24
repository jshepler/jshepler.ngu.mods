using System;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShowTotalWork
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EquipmentDisplay), "updateDisplay")]
        private static void EquipmentDisplay_updateDisplay_postfix(Character ___character, Text ___equipStats)
        {
            if (!___character.InMenu(Menu.Inventory)) return;

            var eCap = ___character.totalCapEnergy();
            var ePow = ___character.totalEnergyPower();
            ___equipStats.text += $"\n\n<b>(eCap * ePow):</b> {___character.display(eCap * ePow)}";

            //var eBars = ___character.totalEnergyBar();
            //___equipStats.text +=
            //    $"\n\n<b>Energy Cap:</b> {___character.display(eCap)}"
            //    + $"\n<b>Energy Pow:</b> {___character.display(ePow)}"
            //    + $"\n<b>(eCap*ePow):</b> {___character.display(eCap * ePow)}"
            //    + $"\n<b>Energy Bars:</b> {___character.display(eBars)}"
            //    + $"\n<b>Base eBeards:</b> {___character.display(eBars * Math.Sqrt(ePow))}";

            var mCap = ___character.totalCapMagic();
            var mPow = ___character.totalMagicPower();
            if (mCap > 1) ___equipStats.text += $"\n<b>(mCap * mPow):</b> {___character.display(mCap * mPow)}";

            //var mBars = ___character.totalMagicBar();
            //if (mCap > 1) ___equipStats.text +=
            //    $"\n\n<b>Magic Cap:</b> {___character.display(mCap)}"
            //    + $"\n<b>Magic Pow:</b> {___character.display(mPow)}"
            //    + $"\n<b>(mCap*mPow):</b> {___character.display(mCap * mPow)}"
            //    + $"\n<b>Magic Bars:</b> {___character.display(mBars)}"
            //    + $"\n<b>Base mBearss:</b> {___character.display(mBars * Math.Sqrt(mPow))}";

            var r3Cap = ___character.totalCapRes3();
            var r3Pow = ___character.totalRes3Power();
            if (r3Cap > 1) ___equipStats.text += $"\n<b>(r3Cap * r3Pow):</b> {___character.display(r3Cap * r3Pow)}";
                //$"\n\n<b>Res3 Cap:</b> {___character.display(r3Cap)}"
                //+ $"\n<b>Res3 Pow:</b> {___character.display(r3Pow)}"
                //+ $"\n<b>(r3Cap*r3Pow):</b> {___character.display(r3Cap * r3Pow)}";
        }
    }
}
