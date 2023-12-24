using HarmonyLib;

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
