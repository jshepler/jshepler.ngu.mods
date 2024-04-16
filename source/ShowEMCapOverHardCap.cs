using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShowEMCapOverHardCap
    {
        const float EM_HARDCAP = 9e+18f;

        [HarmonyTranspiler, HarmonyPatch(typeof(EquipmentDisplay), "updateDisplay")]
        private static IEnumerable<CodeInstruction> EquipmentDisplay_updateDisplay_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n\n<b>Special Bonuses:</b>"))
                .Advance(-1)
                .Insert(Transpilers.EmitDelegate(EMOverHardcaps));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static Text EMOverHardcaps(Text t)
        {
            var c = Plugin.Character;
            var ic = c.inventoryController;

            var eCapNoEquip = (double)c.capEnergy
                * c.adventureController.itopod.totalEnergyCapBonus()
                * c.inventory.macguffinBonuses[1]
                * c.beastQuestPerkController.totalEnergyCapBonus()
                * c.wishesController.totalEnergyCapBonus();

            var mCapNoEquip = (double)c.magic.capMagic
                * c.adventureController.itopod.totalMagicCapBonus()
                * c.inventory.macguffinBonuses[3]
                * c.beastQuestPerkController.totalMagicCapBonus()
                * c.wishesController.totalMagicCapBonus();

            //Plugin.LogInfo($"eCapNoEquip: {eCapNoEquip:e3}, mCapNoEquip: {mCapNoEquip:e3}");

            var eCapEquipBonus = ic.bonuses[specType.EnergyCap] + ic.bonuses[specType.AllCap] + ic.bonuses[specType.EnergyCap3];
            var mCapEquipBonus = ic.bonuses[specType.MagicCap] + ic.bonuses[specType.AllCap] + ic.bonuses[specType.MagicCap3];

            var eCapTotal = eCapNoEquip * (1.0 + eCapEquipBonus);
            var mCapTotal = mCapNoEquip * (1.0 + mCapEquipBonus);
            if (eCapTotal <= EM_HARDCAP && mCapTotal <= EM_HARDCAP)
                return t;

            t.text += "\n";

            if (eCapTotal > EM_HARDCAP)
            {
                t.text += "\neCap over hardcap: ";

                if (eCapNoEquip >= EM_HARDCAP)
                    t.text += $"{(eCapEquipBonus * 100f):#,##0.#}%";

                else
                {
                    var eCapBonusNeeded = (EM_HARDCAP - eCapNoEquip) / eCapNoEquip;
                    var eCapOver = eCapEquipBonus - eCapBonusNeeded;
                    t.text += $"{(eCapOver * 100f):#,##0.#}%";
                    //Plugin.LogInfo($"eCapEquipBonus: {eCapEquipBonus:#,##0.00}, eCapBonusNeeded: {eCapBonusNeeded:#,##0.00}");
                }
            }

            if (mCapTotal > EM_HARDCAP)
            {
                t.text += "\nmCap over hardcap: ";

                if (mCapNoEquip >= EM_HARDCAP)
                    t.text += $"{(mCapEquipBonus * 100f):#,##0.#}%";

                else
                {
                    var mCapBonusNeeded = (EM_HARDCAP - mCapNoEquip) / mCapNoEquip;
                    var mCapOver = mCapEquipBonus - mCapBonusNeeded;
                    t.text += $"{(mCapOver * 100f):#,##0.#}%";
                    //Plugin.LogInfo($"mCapEquipBonus: {mCapEquipBonus:#,##0.00}, mCapBonusNeeded: {mCapBonusNeeded:#,##0.00}");
                }
            }

            return t;
        }
    }
}
