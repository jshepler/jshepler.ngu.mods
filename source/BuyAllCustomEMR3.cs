using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BuyAllCustomEMR3
    {
        private static EnergyPurchases _energyPurchases;
        private static bool _shiftDown = false;
        private static bool _ctrlDown = false;
        private static long _customAllAllCost;

        [HarmonyPostfix, HarmonyPatch(typeof(EnergyPurchases), "Start")]
        private static void EnergyPurchases_Start_postfix(EnergyPurchases __instance)
        {
            _energyPurchases = __instance;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!_energyPurchases.character.InMenu(Menu.EXP_Energy)) return;

                var doRefresh = false;

                if (Input.GetKey(KeyCode.LeftShift) != _shiftDown)
                {
                    _shiftDown = !_shiftDown;
                    doRefresh = true;
                }

                if (Input.GetKey(KeyCode.LeftControl) != _ctrlDown)
                {
                    _ctrlDown = !_ctrlDown;
                    doRefresh = true;
                }

                if (doRefresh)
                    _energyPurchases.refresh();
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnergyPurchases), "updateEnergyPurchases")]
        private static void EnergyPurchases_updateEnergyPurchases_postfix(EnergyPurchases __instance)
        {
            if (_energyPurchases == null) _energyPurchases = __instance;

            var character = __instance.character;
            if (!character.InMenu(Menu.EXP_Energy) || !_shiftDown) return;
            
            var energyPurchases = character.energyPurchases;
            var magicPurchases = character.magicPurchases;
            var res3Purchases = character.res3Purchases;

            var customEnergyAllCost = energyPurchases.customAllCost();
            var customMagicAllCost = magicPurchases.customAllCost();
            var customRes3AllCost = res3Purchases.customAllCost();

            var magicUnlocked = character.magic.capMagic >= 10000;
            var res3Unlocked = _ctrlDown && character.res3.capRes3 >= 10000;

            _customAllAllCost = customEnergyAllCost
                + (magicUnlocked ? customMagicAllCost : 0)
                + (res3Unlocked ? customRes3AllCost : 0);

            var text = $"Buy ALL Custom {(_ctrlDown ? "E/M/R3" : "E/M")} Purchases for {NumberOutput.expPrint(_customAllAllCost)} EXP";
            __instance.buyAllCustom.GetComponentInChildren<Text>().text = text;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EnergyPurchases), "buyCustomAll")]
        private static bool EnergyPurchases_buyCustomAll_prefix(EnergyPurchases __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.EXP_Energy) || !_shiftDown) return true;
            if (character.realExp < _customAllAllCost) return false;

            var customEnergyPowerAmount = character.settings.customPowerAmount;
            var customEnergyCapAmount = character.settings.customCapAmount;
            var customEnergyBarAmount = character.settings.customBarAmount;

            var magicUnlocked = character.magic.capMagic >= 10000;
            var customMagicPowerAmount = character.settings.customMagicPowerAmount;
            var customMagicCapAmount = character.settings.customMagicCapAmount;
            var customMagicBarAmount = character.settings.customMagicBarAmount;

            var res3Unlocked = _ctrlDown && character.res3.capRes3 >= 10000;
            var customRes3PowerAmount = character.settings.customRes3PowerAmount;
            var customRes3CapAmount = character.settings.customRes3CapAmount;
            var customRes3BarAmount = character.settings.customRes3BarAmount;

            if (_customAllAllCost < 0
                || customEnergyBarAmount < 0 || customEnergyCapAmount < 0 || customEnergyPowerAmount < 0
                || (magicUnlocked && (customMagicBarAmount < 0 || customMagicCapAmount < 0 || customMagicPowerAmount < 0))
                || (res3Unlocked && (customRes3BarAmount < 0 || customRes3CapAmount < 0 || customRes3PowerAmount < 0)))
            {
                return false;
            }

            var hardCap = character.hardCap();
            var hardCapPowBar = character.hardCapPowBar();

            character.realExp -= _customAllAllCost;

            character.energyPower = Math.Min(character.energyPower + customEnergyPowerAmount, hardCapPowBar);
            character.capEnergy = Math.Min(character.capEnergy + customEnergyCapAmount, hardCap);
            character.energyBars = Math.Min(character.energyBars + customEnergyBarAmount, hardCapPowBar);

            if (magicUnlocked)
            {
                character.magic.magicPower = Math.Min(character.magic.magicPower + customMagicPowerAmount, hardCapPowBar);
                character.magic.capMagic = Math.Min(character.magic.capMagic + customMagicCapAmount, hardCap);
                character.magic.magicPerBar = Math.Min(character.magic.magicPerBar + customMagicBarAmount, hardCapPowBar);
            }

            if (res3Unlocked)
            {
                character.res3.res3Power = Math.Min(character.res3.res3Power + customRes3PowerAmount, hardCapPowBar);
                character.res3.capRes3 = Math.Min(character.res3.capRes3 + customRes3CapAmount, hardCap);
                character.res3.res3PerBar = Math.Min(character.res3.res3PerBar + customRes3BarAmount, hardCapPowBar);
            }

            __instance.refresh();
            EnergyPurchases_updateEnergyPurchases_postfix(__instance);

            return false;
        }
    }
}
