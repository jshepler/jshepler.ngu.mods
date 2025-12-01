using HarmonyLib;
using UnityEngine;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class UpgradeAllDiggers
    {
        private static AllGoldDiggerController _controller;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_start_postfix(ButtonShower __instance)
        {
            _controller = __instance.character.allDiggers;
            __instance.diggers.gameObject.AddComponent<ClickHandlerComponent>().OnRightClick(e => UpgradeAll());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postfix(ButtonShower __instance)
        {
            if (Options.DiggerUpgradeIndicator.Enabled.Value == false)
                return;

            var character = __instance.character;
            if (!character.settings.diggersOn || character.highestBoss < 30)
                return;

            var canUpgrade = CanUpgradeAnyDigger();
            __instance.diggers.image.color = canUpgrade ? Plugin.ButtonColor_Yellow : Color.white;
        }

        internal static bool CanUpgradeAnyDigger()
        {
            var character = Plugin.Character;
            var diggers = character.diggers.diggers;

            for (var x = 0; x < diggers.Count; x++)
                if (diggers[x].maxLevel > 0 && _controller.upgradeCost(x) <= character.realGold)
                    return true;

            return false;
        }

        private static void UpgradeAll()
        {
            var character = _controller.character;
            var diggers = character.diggers.diggers;

            while (character.realGold > 0)
            {
                var cheapestDigger = -1;
                var cheapestCost = double.MaxValue;

                for (var x = 0; x < diggers.Count; x++)
                {
                    var upgradeCost = _controller.upgradeCost(x);
                    if (upgradeCost > character.realGold) continue;

                    if (upgradeCost < cheapestCost)
                    {
                        cheapestCost = upgradeCost;
                        cheapestDigger = x;
                    }
                }

                if (cheapestDigger == -1) break;

                _controller.upgradeMaxLevel(cheapestDigger);
            }

            _controller.refreshMenu();
        }
    }
}
