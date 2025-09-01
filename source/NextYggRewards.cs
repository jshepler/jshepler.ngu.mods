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
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static Coroutine _coroutine;

        private static bool _expDiggerActive => Plugin.Character.diggers.diggers[11].active;
        private static bool _ppDiggerActive => Plugin.Character.diggers.diggers[8].active;

        [HarmonyPostfix, HarmonyPatch(typeof(FruitController), "Start")]
        private static void FruitController_Start_postfix(FruitController __instance)
        {
            __instance.actionButton.gameObject.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter(__instance))
                .OnPointerExit(OnPointerExit);
        }

        private static Action<PointerEventData> OnPointerEnter(FruitController fc)
        {
            return pd =>
            {
                if (_coroutine != null)
                    Plugin.EndCoroutine(_coroutine);

                if (fc.character.yggdrasil.fruits[fc.id].maxTier > 0)
                    _coroutine = Plugin.BeginCoroutine(ShowTooltip(fc));
            };
        }

        private static void OnPointerExit(PointerEventData pd)
        {
            if (_coroutine != null)
            {
                Plugin.EndCoroutine(_coroutine);
                _coroutine = null;
                Plugin.HideTooltip();
            }
        }

        private static IEnumerator ShowTooltip(FruitController fc)
        {
            while (true)
            {
                Plugin.ShowTooltip(GetText(fc));
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

        private static Func<float, string> showMulti = value => $"<size=10>x</size>{value:#,##0.####}";

        private static string Seeds(FruitController fc, float tierFactor, float poop, bool harvest = false)
        {
            var character = fc.character;

            var baseSeeds = character.yggdrasilController.baseSeedReward[fc.id];
            var equip = 1f + character.inventoryController.bonuses[specType.Seeds];
            var perks = character.adventureController.itopod.totalSeedBonus();
            var quirks = character.beastQuestPerkController.totalSeedBonus();
            var ngu = character.NGUController.yggdrasilBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(fc.id);

            var text = $"\n\n<b>Base Seeds:</b> {baseSeeds}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (equip > 1.0f)
                text += $"\n<b>Equipment:</b> {showMulti(equip)}";

            if (perks > 1.0f)
                text += $"\n<b>Perks:</b> {showMulti(perks)}";

            if (quirks > 1.0f)
                text += $"\n<b>Quirks:</b> {showMulti(quirks)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (poop > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poop)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            if (harvest)
                text += $"\n<b>Harvest Seeds Bonus:</b> {showMulti(2f)}";

            return text;
        }

        private static string Harvest(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(fc.id));
            var poopMulti = poopBonus(fc.id);

            var seeds = fc.harvestSeedReward(fc.id, tierFactor, poopMulti);
            var text = $"+{fc.character.display(seeds)} seeds";

            if (Plugin.AltIsDown)
                text += Seeds(fc, tierFactor, poopMulti, true);

            return text;
        }

        private static string Mayo(FruitController fc)
        {
            var tierFactor = Mathf.Pow(fc.harvestTier(fc.id), 1.1f);
            var poopMulti = poopBonus(fc.id);
            var seeds = fc.seedReward(fc.id, (int)tierFactor, poopMulti);
            var mayoSpeed = fc.character.cardsController.totalMayoSpeed();
            var mayo = tierFactor * 0.025f * mayoSpeed * poopMulti;
            var name = fc.character.cardsController.getManaName(fc.id - 15);

            var text = $"+{mayo:0.##} {name}\n+{fc.character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base Mayo:</b> 0.025"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}"
                + $"\n<b>Mayo Speed:</b> {showMulti(mayoSpeed)}";

            if(poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoG(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(0));
            var poopMulti = poopBonus(0);
            var seeds = fc.seedReward(0, tierFactor, poopMulti);

            var character = fc.character;
            var gps = character.grossGoldPerSecond();
            var goldPerHour = gps * 1800.0;
            var fh = character.adventureController.itopod.totalHarvestBonus(0);
            var gold = tierFactor * poopMulti * goldPerHour * fh;

            var text = $"+{character.display(gold)} gold"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Gross GPS (1hr):</b> {character.display(goldPerHour)}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoPa(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(1));
            var poopMulti = poopBonus(1);
            var seeds = fc.seedReward(1, tierFactor, poopMulti);

            var character = fc.character;
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(1);
            var gainLevels = (long)Mathf.Ceil(tierFactor * poopMulti * ngu * ygg * fh);
            var oldBonus = character.yggdrasil.totalStatBonus();
            var newBonus = Math.Pow(character.yggdrasil.fruits[1].totalLevels + gainLevels, 1.5);

            var text = $"+{character.display(gainLevels)} levels gained"
                + $"\n+{character.display((newBonus - oldBonus) * 100f)}% Attack/Defense bonus,"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base Levels:</b> 1"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoA(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(2));
            var poopMulti = poopBonus(2);
            var seeds = fc.seedReward(2, tierFactor, poopMulti);

            var character = fc.character;
            var baseT = Mathf.Pow(character.adventure.defense, 0.2f);
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(2);
            var pt = Math.Max(0, (int)(baseT * tierFactor * poopMulti * ngu * ygg * fh));
            var health = pt * 3f;
            var regen = pt * 0.03f;

            var text = $"+{character.display(pt)} Power"
                + $"\n+{character.display(pt)} Toughness"
                + $"\n+{character.display(health)} Max Health"
                + $"\n+{character.display(regen)} Health Regen"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Base Tough (5th root):</b> {baseT}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoK(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(3));
            var poopMulti = poopBonus(3);
            var seeds = fc.seedReward(3, tierFactor, poopMulti);

            var character = fc.character;
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(3);

            var perks = 1;
            if (character.adventure.itopod.perkLevel[19] >= 1)
                perks *= 3;

            if (character.adventure.itopod.perkLevel[20] >= 1)
                perks *= 3;

            var exp = (int)Mathf.Ceil(5 * tierFactor * poopMulti * ngu * ygg * fh) * perks;
            var modified = character.checkExpAdded(exp);

            var text = $"+{character.display(modified)} EXP"
                + $"\n+{character.display(seeds)} seeds";

            if (!_expDiggerActive)
                text += "\n\n<b><color=red>EXP DIGGER IS NOT ACTIVE!</color></b>";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base EXP:</b> 5"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (perks > 1)
                text += $"\n<b>Perks:</b> {showMulti(perks)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            var globalExpMulti = character.checkExpAdded(10000L) / 10000f;
            text += $"\n<b>Global EXP Multi:</b> {showMulti(globalExpMulti)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string POM(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(4));
            var poopMulti = poopBonus(4);
            var seeds = fc.harvestSeedReward(4, tierFactor, poopMulti);

            var text = $"+{fc.character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += Seeds(fc, tierFactor, poopMulti, true);
            return text;
        }

        private static string FoL(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(5));
            var poopMulti = poopBonus(5);
            var seeds = fc.seedReward(5, tierFactor, poopMulti);

            var character = fc.character;
            var baseLevels = character.yggdrasilController.baseSeedReward[5] * 0.7f;
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(5);

            var gainLevels = (long)Mathf.Ceil(baseLevels * tierFactor * poopMulti * ngu * ygg * fh);
            var oldBonus = 1f + character.yggdrasil.totalLuck * 0.0005f;
            var newBonus = 1f + (character.yggdrasil.totalLuck + gainLevels) * 0.0005f;

            var text = $"+{character.display(gainLevels)} levels gained"
                + $"\n+{(newBonus - oldBonus) * 100f:#,##0.##}% Drop Chance Bonus"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Base Levels:</b> {baseLevels}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoPb(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(6));
            var poopMulti = poopBonus(6);
            var seeds = fc.seedReward(6, tierFactor, poopMulti);

            var character = fc.character;
            var baseLevels = character.yggdrasilController.baseSeedReward[6];
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(6);

            var gainLevels = (long)Mathf.Ceil(baseLevels * tierFactor * poopMulti * ngu * ygg * fh);
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus, 2) * 0.0005;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus + gainLevels, 2) * 0.0005;

            var text = $"+{character.display(gainLevels)} levels"
                + $"\n+{character.display((newBonus - oldBonus) * 100.0)}% Attack/Defense bonus"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Base Levels:</b> {baseLevels}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoAP(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(7));
            var poopMulti = poopBonus(7);
            var seeds = fc.seedReward(7, tierFactor, poopMulti);

            var character = fc.character;
            var fh = character.adventureController.itopod.totalHarvestBonus(7);
            var ap = (long)Mathf.Ceil(15 * tierFactor * poopMulti * fh);
            var modded = character.checkAPAdded(ap);

            var text = $"+{character.display(modded)} AP"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base AP:</b> 15"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            var globalAPMulti = character.checkAPAdded(10000L) / 10000f;
            text += $"\n<b>Global AP Multi:</b> {showMulti(globalAPMulti)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoN(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(8));
            var poopMulti = poopBonus(8);
            var seeds = fc.seedReward(8, tierFactor, poopMulti);

            var character = fc.character;
            var baseLevels = character.yggdrasilController.baseSeedReward[8];
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(8);

            var gainLevels = (long)Mathf.Ceil(baseLevels * tierFactor * poopMulti * ngu * ygg * fh);
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermNumberBonus, 1.3) * 0.0005;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermNumberBonus + gainLevels, 1.3) * 0.0005;

            var text = $"+{character.display(gainLevels)} levels"
                + $"\n+{character.display((newBonus - oldBonus) * 100f)}% NUMBER Bonus"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Base Levels:</b> {baseLevels}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoR(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(9));
            var poopMulti = poopBonus(9);
            var seeds = fc.seedReward(9, tierFactor, poopMulti);

            var character = fc.character;
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(9);
            var globalPPPMulti = character.adventureController.itopod.totalPPBonus(usePills: false);

            var ppp = (long)Mathf.Ceil(60000 * tierFactor * poopMulti * ygg * fh * globalPPPMulti);
            var pp = character.adventureController.itopod.progressToPP(ppp);
            ppp = character.adventureController.itopod.progressToRemainder(ppp);

            var text = $"+{character.display(pp)} PP"
                + $"\n+{character.display(ppp)} progress to next PP"
                + $"\n+{character.display(seeds)} seeds";

            if (!_ppDiggerActive)
                text += "\n\n<b><color=red>PP DIGGER IS NOT ACTIVE!</color></b>";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base PPP:</b> 60,000"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += $"\n<b>Global PPP Multi (w/o pills):</b> {showMulti(globalPPPMulti)}"
                + Seeds(fc, tierFactor, poopMulti);

            return text;
        }

        private static string Guff1(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(10));
            var poopMulti = poopBonus(10);
            var seeds = fc.seedReward(10, tierFactor, poopMulti);

            var character = fc.character;
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(10);
            var wish = character.wishesController.totalFruitGuffbonus();

            var levels = (long)Mathf.Ceil(tierFactor * 0.5f * poopMulti * ygg * fh * wish);
            var capped = levels > int.MaxValue ? int.MaxValue : (int)levels;
            var isRandom = character.wishes.wishes[25].level <= 0;

            var text = $"+{character.display(capped)} levels to {(isRandom ? "a random" : "your first")} equipped MacGuffin"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base Levels:</b> 0.5"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (wish > 1.0f)
                text += $"\n<b>Wish 60:</b> {showMulti(wish)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoPd(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(11));
            var poopMulti = poopBonus(11);
            var seeds = fc.seedReward(11, tierFactor, poopMulti);

            var character = fc.character;
            var baseLevels = character.yggdrasilController.baseSeedReward[11];
            var ngu = character.NGUController.yggdrasilBonus();
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(11);

            var gainLevels = (long)Mathf.Ceil(baseLevels * tierFactor * poopMulti * ngu * ygg * fh);
            var oldBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus2, 1.3) * 1E-06;
            var newBonus = 1.0 + Math.Pow(character.yggdrasil.totalPermStatBonus2 + gainLevels, 1.3) * 1E-06;

            var text = $"+{character.display(gainLevels)} levels"
                + $"\n+{(newBonus - oldBonus) * 100f:#,##0.##}% Attack/Defense bonus"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += $"\n\n<b>Base Levels:</b> {baseLevels}"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ngu > 1.0f)
                text += $"\n<b>NGU YGG:</b> {showMulti(ngu)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string Watermelon(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(12));
            var poopMulti = poopBonus(12);
            var seeds = fc.harvestSeedReward(12, tierFactor, poopMulti);

            var text = $"+{fc.character.display(seeds)} seeds";
            if (!Plugin.AltIsDown)
                return text;

            text += Seeds(fc, tierFactor, poopMulti, true);
            return text;
        }

        private static string Guff2(FruitController fc)
        {
            var tierFactor = fc.tierFactor(fc.harvestTier(13));
            var poopMulti = poopBonus(13);
            var seeds = fc.seedReward(13, tierFactor, poopMulti);

            var character = fc.character;
            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(13);
            var levels = (long)Mathf.Ceil(tierFactor * 0.1f * poopMulti * ygg * fh);
            var capped = levels > int.MaxValue ? int.MaxValue : (int)levels;

            var text = $"+{character.display(capped)} levels to ALL equipped MacGuffins"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base Levels:</b> 0.1"
                + $"\n<b>Tier Factor:</b> {showMulti(tierFactor)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += Seeds(fc, tierFactor, poopMulti);
            return text;
        }

        private static string FoQ(FruitController fc)
        {
            var tier = fc.harvestTier(14);
            var poopMulti = poopBonus(14);
            var seeds = fc.seedReward(14, tier, poopMulti);

            var character = fc.character;

            var globalQPMulti = character.beastQuestController.questRewardFactor();
            if (character.beastQuest.usedButter)
                globalQPMulti /= character.allArbitrary.butterModifier();

            var ygg = character.yggdrasilYieldBonus();
            var fh = character.adventureController.itopod.totalHarvestBonus(14);
            var qp = (long)Mathf.Ceil(3 * tier * poopMulti * ygg * globalQPMulti * fh);

            var text = $"+{character.display(qp)} QP"
                + $"\n+{character.display(seeds)} seeds";

            if (!Plugin.AltIsDown)
                return text;

            text += "\n\n<b>Base QP:</b> 3"
                + $"\n<b>Tier Factor:</b> {showMulti(tier)}";

            if (ygg > 1.0f)
                text += $"\n<b>YGG Yield:</b> {showMulti(ygg)}";

            if (poopMulti > 1.0f)
                text += $"\n<b>Poop:</b> {showMulti(poopMulti)}";

            if (fh > 1.0f)
                text += $"\n<b>First Harvest:</b> {showMulti(fh)}";

            text += $"\n<b>Global QP Multi (w/o butter):</b> {showMulti(globalQPMulti)}"
                + Seeds(fc, tier, poopMulti);

            return text;
        }

        private static float poopBonus(int fruitId)
        {
            var character = Plugin.Character;
            var fruit = character.yggdrasil.fruits[fruitId];

            if (!fruit.usePoop)
                return 1f;

            var currentTier = character.yggdrasilController.fruits[0].harvestTier(fruitId);
            if (character.settings.poopOnlyMaxTier && currentTier < fruit.maxTier)
                return 1f;

            return character.allArbitrary.poopModifier();
        }
    }
}
