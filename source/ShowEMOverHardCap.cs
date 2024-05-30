using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShowEMOverHardCap
    {
        const float EM_CAP_HARDCAP = 9e+18f;
        const float EM_POW_HARDCAP = 1e+18f;

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

            var ePowNoEquip = (double)c.energyPower
                * c.adventureController.itopod.totalEnergyPowerBonus()
                * c.inventory.macguffinBonuses[0]
                * c.beastQuestPerkController.totalEnergyPowerBonus()
                * c.wishesController.totalEnergyPowerBonus();


            var mCapNoEquip = (double)c.magic.capMagic
                * c.adventureController.itopod.totalMagicCapBonus()
                * c.inventory.macguffinBonuses[3]
                * c.beastQuestPerkController.totalMagicCapBonus()
                * c.wishesController.totalMagicCapBonus();

            var mPowNoEquip = c.magic.magicPower
                * c.adventureController.itopod.totalMagicPowerBonus()
                * c.inventory.macguffinBonuses[2]
                * c.beastQuestPerkController.totalMagicPowerBonus()
                * c.wishesController.totalMagicPowerBonus();

            var eCapEquipBonus = ic.bonuses[specType.EnergyCap] + ic.bonuses[specType.AllCap] + ic.bonuses[specType.EnergyCap3];
            var ePowEquipBonus = ic.bonuses[specType.EnergyPower] + ic.bonuses[specType.EnergyPower2] + ic.bonuses[specType.EnergyPower3] + ic.bonuses[specType.AllPower];

            var mCapEquipBonus = ic.bonuses[specType.MagicCap] + ic.bonuses[specType.AllCap] + ic.bonuses[specType.MagicCap3];
            var mPowEquipBonus = ic.bonuses[specType.MagicPower] + ic.bonuses[specType.MagicPower2] + ic.bonuses[specType.MagicPower3] + ic.bonuses[specType.AllPower];

            var eCapTotal = eCapNoEquip * (1.0 + eCapEquipBonus);
            var ePowTotal = ePowNoEquip * (1.0 + ePowEquipBonus);
            var mCapTotal = mCapNoEquip * (1.0 + mCapEquipBonus);
            var mPowTotal = mPowNoEquip * (1.0 + mPowEquipBonus);

            var sb = new StringBuilder();

            if (ePowTotal > EM_POW_HARDCAP)
            {
                sb.Append("\nePow over hardcap: ");

                if (ePowNoEquip >= EM_POW_HARDCAP)
                    sb.Append($"{(ePowEquipBonus * 100f):#,##0.#}%");

                else
                {
                    var ePowBonusNeeded = (EM_POW_HARDCAP - ePowNoEquip) / ePowNoEquip;
                    var ePowOver = ePowEquipBonus - ePowBonusNeeded;
                    sb.Append($"{(ePowOver * 100f):#,##0.#}%");
                }
            }

            if (eCapTotal > EM_CAP_HARDCAP)
            {
                sb.Append("\neCap over hardcap: ");

                if (eCapNoEquip >= EM_CAP_HARDCAP)
                    sb.Append($"{(eCapEquipBonus * 100f):#,##0.#}%");

                else
                {
                    var eCapBonusNeeded = (EM_CAP_HARDCAP - eCapNoEquip) / eCapNoEquip;
                    var eCapOver = eCapEquipBonus - eCapBonusNeeded;
                    sb.Append($"{(eCapOver * 100f):#,##0.#}%");
                }
            }

            if (mPowTotal > EM_POW_HARDCAP)
            {
                sb.Append("\nmPow over hardcap: ");

                if (mPowNoEquip >= EM_POW_HARDCAP)
                    sb.Append($"{(mPowEquipBonus * 100f):#,##0.#}%");

                else
                {
                    var mPowBonusNeeded = (EM_POW_HARDCAP - mPowNoEquip) / mPowNoEquip;
                    var mPowOver = mPowEquipBonus - mPowBonusNeeded;
                    sb.Append($"{(mPowOver * 100f):#,##0.#}%");
                }
            }

            if (mCapTotal > EM_CAP_HARDCAP)
            {
                sb.Append("\nmCap over hardcap: ");

                if (mCapNoEquip >= EM_CAP_HARDCAP)
                    sb.Append($"{(mCapEquipBonus * 100f):#,##0.#}%");

                else
                {
                    var mCapBonusNeeded = (EM_CAP_HARDCAP - mCapNoEquip) / mCapNoEquip;
                    var mCapOver = mCapEquipBonus - mCapBonusNeeded;
                    sb.Append($"{(mCapOver * 100f):#,##0.#}%");
                }
            }

            if (sb.Length > 0)
                t.text += $"\n{sb}";

            return t;
        }
    }
}
