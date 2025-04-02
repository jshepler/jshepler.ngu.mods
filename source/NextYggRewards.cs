using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class NextYggRewards
    {
        private static WaitForSeconds _wait = new WaitForSeconds(1f);
        private static HoverTooltip _tooltip;
        private static AllYggdrasil _controller;
        private static Coroutine _coroutine;

        [HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "Start")]
        private static void AllYggdrasil_Start_postfix(AllYggdrasil __instance)
        {
            _tooltip = __instance.character.tooltip;
            _controller = __instance;

            foreach (var fc in __instance.fruits)
            {
                fc.gameObject.transform.Find("../../Activate Button").gameObject
                    .AddComponent<PointerHandlerComponent>()
                    .OnPointerEnter(OnPointerEnter(fc))
                    .OnPointerExit(OnPointerExit);
            }
        }

        private static Action<PointerEventData> OnPointerEnter(FruitController fc)
        {
            return pd =>
            {
                if (_coroutine != null)
                    _controller.StopCoroutine(_coroutine);

                if (fc.character.yggdrasil.fruits[fc.id].maxTier > 0)
                    _coroutine = _controller.StartCoroutine(ShowTooltip(fc));
            };
        }

        private static void OnPointerExit(PointerEventData pd)
        {
            if (_coroutine != null)
            {
                _controller.StopCoroutine(_coroutine);
                _coroutine = null;
                _tooltip.hideTooltip();
            }
        }

        private static IEnumerator ShowTooltip(FruitController fc)
        {
            while (true)
            {
                _tooltip.showTooltip(GetText(fc));
                yield return _wait;
            }
        }

        private static string GetText(FruitController fc)
        {
            var character = fc.character;
            var fruit = character.yggdrasilController.fruitName[fc.id];
            string text;

            if (character.yggdrasil.fruits[fc.id].eatFruit == false)
                text = $"If harvested now, {fruit} will give:\n\n{Harvest(fc)}";

            else if (fc.id > 14)
                text = $"If eaten now, {fruit} will give:\n\n{Mayo(fc)}";

            else
            {
                var rewards = fc.id switch
                {
                    0 => FoG(fc),
                    1 => FoPa(fc),
                    2 => FoA(fc),
                    3 => FoK(fc),
                    4 => POM(fc),
                    5 => FoL(fc),
                    6 => FoPb(fc),
                    7 => FoAP(fc),
                    8 => FoN(fc),
                    9 => FoR(fc),
                    10 => Guff1(fc),
                    11 => FoPd(fc),
                    12 => Watermelon(fc),
                    13 => Guff2(fc),
                    14 => FoQ(fc),
                    _ => null
                };

                text = $"If eaten now, {fruit} will give:\n\n{rewards}";
            }

            return text;
        }

        private static string Harvest(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(fc.id));
            var poopMulti = poopBonus(fc.id);
            var seeds = fc.harvestSeedReward(fc.id, tierFactor, poopMulti);

            return $"+{fc.character.display(seeds)} seeds";
        }

        private static string Mayo(FruitController fc)
        {
            var tierFactor = Mathf.Pow(fc.harvestTier(fc.id), 1.1f);
            var poopMulti = poopBonus(fc.id);
            var seeds = fc.seedReward(fc.id, (int)tierFactor, poopMulti);
            var mayo = tierFactor * 0.025f * fc.character.cardsController.totalMayoSpeed() * poopMulti;
            var name = fc.character.cardsController.getManaName(fc.id - 15);

            return $"+{mayo:0.##} {name}\n+{fc.character.display(seeds)} seeds";
        }

        private static string FoG(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(0));
            var poopMulti = poopBonus(0);
            var seeds = fc.seedReward(0, tierFactor, poopMulti);

            var character = fc.character;
            var gold = tierFactor * poopMulti * 1800.0 * character.grossGoldPerSecond() * character.adventureController.itopod.totalHarvestBonus(0);

            return $"+{character.display(gold)} gold"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoPa(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(1));
            var poopMulti = poopBonus(1);
            var seeds = fc.seedReward(1, tierFactor, poopMulti);

            var character = fc.character;
            var gainLevels = (long)Mathf.Ceil(tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(1));
            var oldBonus = character.yggdrasil.totalStatBonus();
            var newBonus = Math.Pow(character.yggdrasil.fruits[1].totalLevels + gainLevels, 1.5);

            return $"+{character.display(gainLevels)} levels gained"
                + $"\n+{character.display((newBonus - oldBonus) * 100f)}% Attack/Defense bonus,"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoA(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(2));
            var poopMulti = poopBonus(2);
            var seeds = fc.seedReward(2, tierFactor, poopMulti);

            var character = fc.character;
            var power = Math.Max(0, (int)(Mathf.Pow(character.adventure.defense, 0.2f) * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(2)));
            var toughness = Math.Max(0, (int)(Mathf.Pow(character.adventure.defense, 0.2f) * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(2)));
            var health = power * 3f;
            var regen = toughness * 0.03f;

            return $"+{character.display(power)} Power"
                + $"\n+{character.display(toughness)} Toughness"
                + $"\n+{character.display(health)} Max Health"
                + $"\n+{character.display(regen)} Health Regen"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoK(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(3));
            var poopMulti = poopBonus(3);
            var seeds = fc.seedReward(3, tierFactor, poopMulti);

            var character = fc.character;
            var exp = (int)Mathf.Ceil(5 * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(3));

            if (character.adventure.itopod.perkLevel[19] >= 1)
                exp *= 3;

            if (character.adventure.itopod.perkLevel[20] >= 1)
                exp *= 3;

            var modified = character.checkExpAdded(exp);

            return $"+{character.display(modified)} EXP"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string POM(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(4));
            var poopMulti = poopBonus(4);
            var seeds = fc.harvestSeedReward(4, tierFactor, poopMulti);

            return $"+{fc.character.display(seeds)} seeds";
        }

        private static string FoL(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(5));
            var poopMulti = poopBonus(5);
            var seeds = fc.seedReward(5, tierFactor, poopMulti);

            var character = fc.character;
            var gainLevels = (long)Mathf.Ceil(character.yggdrasilController.baseSeedReward[5] * 0.7f * (float)tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(5));
            var oldBonus = 1f + character.yggdrasil.totalLuck * 0.0005f;
            var newBonus = 1f + (character.yggdrasil.totalLuck + gainLevels) * 0.0005f;

            return $"+{character.display(gainLevels)} levels gained"
                + $"\n+{(newBonus - oldBonus) * 100f:#,##0.##}% Drop Chance Bonus"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoPb(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(6));
            var poopMulti = poopBonus(6);
            var seeds = fc.seedReward(6, tierFactor, poopMulti);

            var character = fc.character;
            var gainLevels = (long)Mathf.Ceil(character.yggdrasilController.baseSeedReward[6] * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(6));
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus, 2) * 0.0005;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus + gainLevels, 2) * 0.0005;

            return $"+{character.display(gainLevels)} levels"
                + $"\n+{character.display((newBonus - oldBonus) * 100.0)}% Attack/Defense bonus"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoAP(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(7));
            var poopMulti = poopBonus(7);
            var seeds = fc.seedReward(7, tierFactor, poopMulti);

            var character = fc.character;
            var ap = (long)Mathf.Ceil(15 * tierFactor * poopMulti * character.adventureController.itopod.totalHarvestBonus(7));
            var modded = character.checkAPAdded(ap);

            return $"+{character.display(modded)} AP"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoN(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(8));
            var poopMulti = poopBonus(8);
            var seeds = fc.seedReward(8, tierFactor, poopMulti);

            var character = fc.character;
            var gainLevels = (long)Mathf.Ceil(character.yggdrasilController.baseSeedReward[8] * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(8));
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermNumberBonus, 1.3) * 0.0005;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermNumberBonus + gainLevels, 1.3) * 0.0005;

            return $"+{character.display(gainLevels)} levels"
                + $"\n+{character.display((newBonus - oldBonus) * 100f)}% NUMBER Bonus"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoR(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(9));
            var poopMulti = poopBonus(9);
            var seeds = fc.seedReward(9, tierFactor, poopMulti);

            var character = fc.character;
            var ppp = (long)Mathf.Ceil(60000 * tierFactor * poopMulti * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalPPBonus(usePills: false) * character.adventureController.itopod.totalHarvestBonus(9));
            var pp = character.adventureController.itopod.progressToPP(ppp);
            ppp = character.adventureController.itopod.progressToRemainder(ppp);

            return $"+{character.display(pp)} PP"
                + $"\n+{character.display(ppp)} progress to next PP"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string Guff1(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(10));
            var poopMulti = poopBonus(10);
            var seeds = fc.seedReward(10, tierFactor, poopMulti);

            var character = fc.character;
            var levels = (long)Mathf.Ceil(tierFactor * 0.5f * poopMulti * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(10) * character.wishesController.totalFruitGuffbonus());
            var capped = levels > int.MaxValue ? int.MaxValue : (int)levels;
            var isRandom = character.wishes.wishes[25].level <= 0;

            return $"+{character.display(capped)} levels to {(isRandom ? "a random" : "your first")} equipped MacGuffin"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoPd(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(11));
            var poopMulti = poopBonus(11);
            var seeds = fc.seedReward(11, tierFactor, poopMulti);

            var character = fc.character;
            var gainLevels = (long)Mathf.Ceil(character.yggdrasilController.baseSeedReward[11] * tierFactor * poopMulti * character.NGUController.yggdrasilBonus() * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(11));
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus2, 1.3) * 1E-06;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus2 + gainLevels, 1.3) * 1E-06;

            return $"+{character.display(gainLevels)} levels"
                + $"\n+{(newBonus - oldBonus) * 100f:#,##0.##}% Attack/Defense bonus"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string Watermelon(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(12));
            var poopMulti = poopBonus(12);
            var seeds = fc.harvestSeedReward(12, tierFactor, poopMulti);

            return $"+{fc.character.display(seeds)} seeds";
        }

        private static string Guff2(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(13));
            var poopMulti = poopBonus(13);
            var seeds = fc.seedReward(13, tierFactor, poopMulti);

            var character = fc.character;
            var levels = (long)Mathf.Ceil(tierFactor * 0.1f * poopMulti * character.yggdrasilYieldBonus() * character.adventureController.itopod.totalHarvestBonus(13));
            var capped = levels > int.MaxValue ? int.MaxValue : (int)levels;

            return $"+{character.display(capped)} levels to ALL equipped MacGuffins"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static string FoQ(FruitController fc)
        {
            var tier = fc.harvestTier(14);
            var poopMulti = poopBonus(14);
            var seeds = fc.seedReward(14, tier, poopMulti);

            var character = fc.character;
            var qpFactor = character.beastQuestController.questRewardFactor();
            if (character.beastQuest.usedButter)
                qpFactor /= character.allArbitrary.butterModifier();

            var qp = (long)Mathf.Ceil(3 * tier * poopMulti * character.yggdrasilYieldBonus() * qpFactor * character.adventureController.itopod.totalHarvestBonus(14));

            return $"+{character.display(qp)} QP"
                + $"\n+{character.display(seeds)} seeds";
        }

        private static float poopBonus(int fruitId)
        {
            var character = _controller.character;
            var fruit = character.yggdrasil.fruits[fruitId];

            if (!fruit.usePoop)
                return 1f;

            var currentTier = character.yggdrasilController.fruits[fruitId].harvestTier(fruitId);
            if (character.settings.poopOnlyMaxTier && currentTier < fruit.maxTier)
                return 1f;

            return character.allArbitrary.poopModifier();
        }
    }
}
