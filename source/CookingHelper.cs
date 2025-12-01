using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CookingHelper
    {
        const int INGREDIENT_MAX_LEVEL = 20;
        private static bool _firstLoad = false;

        private static DateTime CookingReadyAtUtc;
        private static bool _altIsDown = false;
        private static bool _lastAltIsDown = false;

        private static List<IngredientPair> _pairs;
        private static List<IngredientPair> pairs
        {
            get
            {
                _pairs ??= [new(1), new(2), new(3), new(4)];
                return _pairs;
            }
        }

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnOfflineProgressionComplete += (o, e) =>
            {
                _pairs = null;
                _firstLoad = true;

                CookingReadyAtUtc = CalcReadyTimeUtc();
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CookingController), "Update")]
        private static bool CookingController_Update_prefix(CookingController __instance)
        {
            if (!_firstLoad)
                return true;

            var eatRate = __instance.eatRate();
            var maxBankedTime = __instance.maxBankedtime();
            ref float cookTimer = ref Plugin.Character.cooking.cookTimer;

            if (cookTimer <= maxBankedTime)
            {
                if (cookTimer < eatRate)
                    cookTimer = eatRate - (float)(CookingReadyAtUtc - DateTime.UtcNow).TotalSeconds;
                else
                    cookTimer = eatRate + (float)(DateTime.UtcNow - CookingReadyAtUtc).TotalSeconds;
            }
            else
            {
                cookTimer = maxBankedTime;
            }

            __instance.refreshTimer += Time.deltaTime;
            if (__instance.refreshTimer > 1f)
            {
                __instance.refreshTimer -= 1f;
                __instance.updateDishUI();
            }

            _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (_altIsDown != _lastAltIsDown)
            {
                _lastAltIsDown = _altIsDown;
                __instance.updateIngredientPods();
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CookingController), "consumeDish")]
        private static void CookingController_consumeDish_postfix(CookingController __instance)
        {
            CookingReadyAtUtc = CalcReadyTimeUtc();
            __instance.updateMenu();
        }

        internal static DateTime CalcReadyTimeUtc()
        {
            var eatRate = Plugin.Character.cookingController.eatRate();
            var cookTimer = Plugin.Character.cooking.cookTimer;
            var currentReadyTime = cookTimer <= eatRate
                ? DateTime.UtcNow + TimeSpan.FromSeconds(eatRate - cookTimer)
                : DateTime.UtcNow - TimeSpan.FromSeconds(cookTimer - eatRate);

            //Plugin.LogInfo($"eatRate: {eatRate}, cookTimer: {cookTimer}, currentReadyTime: {currentReadyTime}");

            return currentReadyTime;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CookingController), "assignNewDish")]
        private static void CookingController_assignNewDish_postfix(CookingController __instance)
        {
            ClearIngredientLevels();
            _pairs = null;
            __instance.updateMenu();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(IngredientPodUI), "updatePod")]
        private static void IngredientPodUI_updatePod_postfix(IngredientPodUI __instance)
        {
            var cooking = Plugin.Character.cooking;

            if (!_firstLoad)
                return;

            var index = __instance.ingredientDataIndex;
            var pairIndex = pairs.FindIndex(p => p.i1Index == index || p.i2Index == index);
            var pair = pairs[pairIndex];

            var iTarget = cooking.ingredients[index].targetLevel;
            var pTarget = pair.pairTarget;

            __instance.nameText.text = $"P{pairIndex + 1}: {__instance.nameText.text} ({iTarget}|{pTarget})";
            __instance.nameText.resizeTextForBestFit = true;

            var i1Level = cooking.ingredients[pair.i1Index].curLevel;
            var i2Level = cooking.ingredients[pair.i2Index].curLevel;
            var curScore = pair.GetPairScore(i1Level, i2Level);
            //var isMaxScore = curScore == pair.maxScore;
            //var isMaxScore = Mathf.Abs(curScore - pair.maxScore) <= float.Epsilon;
            var isMaxScore = curScore.ToString() == pair.maxScore.ToString();

            //Plugin.LogInfo($"[{index}] P{pairIndex}, PS: {curScore}, MS: {pair.maxScore}");

            __instance.nameText.color = _altIsDown && isMaxScore ? Plugin.ButtonColor_Green : Color.black;

            var propIndex = cooking.ingredients[index].propertyIndex;
            var multi = __instance.character.cookingController.ingredientProperties[propIndex].unitMultiplier;
            var curLevel = cooking.ingredients[index].curLevel;
            if (multi > 1)
                __instance.ingredientUnitText.text = $"[{curLevel}] {__instance.ingredientUnitText.text}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CookingController), "updateDishUI")]
        private static void CookingController_updateDishUI_postfix(CookingController __instance)
        {
            var eatRate = __instance.eatRate();
            var cookTimer = Plugin.Character.cooking.cookTimer;
            var readyAt = DateTime.Now.AddSeconds(eatRate - cookTimer);
            var daysDiff = (readyAt.Date - DateTime.Now.Date).Days;

            var dayString = daysDiff switch
            {
                > 1 => $"in {daysDiff} days",
                1 => "tomorrow",
                0 => "today",
                -1 => "yesterday",
                _ => $"{-daysDiff} days ago"
            };
            
            __instance.nextdishTimerText.text += $"\nReady {dayString} at: {readyAt:h:mm:ss tt}";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CookingController), "showDishInfo")]
        private static bool CookingController_showDishInfo_prefix(CookingController __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Cooking))
                return false;

            var curDishIndex = character.cooking.curDishIndex;
            var dishProperties = __instance.dishProperties;
            if (curDishIndex < 0 || curDishIndex >= dishProperties.Count)
                return false;

            var props = dishProperties[curDishIndex];
            var message = $"<b>{props.dishName}</b>\n\"{props.dishDesc}\"";

            var cookingItemCount = GetCookingItemCount();
            var cookingItemMulti = Mathf.Pow(1.03f, cookingItemCount);
            message += $"\n\n   Cooking Items ({cookingItemCount}): x{cookingItemMulti}";

            if (character.inventory.itemList.spaceComplete)
                message += "\n   Space Set Completion: x1.1";

            if (character.cooking.ingredients[6].unlocked)
                message += "\n   Ingredient 7 Unlocked: x1.2";

            if (character.cooking.ingredients[7].unlocked)
                message += "\n   Ingredient 8 Unlocked: x1.2";

            var totalCookingMulti = __instance.totalCookingBonuses();
            message += $"\n<b>Total Cooking Multiplier:</b> x{totalCookingMulti}";

            var curExpBonus = character.cooking.expBonus - 1f;
            message += $"\n\n<b>Current Exp Bonus:</b> {curExpBonus}";

            if (curExpBonus <= 0.8f)
            {
                var baseExpGain = 1f - Mathf.Pow(curExpBonus, 2);
                message += "\n<b>Base Bonus Gain</b> (Total Exp Gain <= 180%)"
                    + "\n   = 1 - ([exp bonus] ^ 2)"
                    + $"\n   = {baseExpGain}";
            }

            else
                message += "\n<b>Base Bonus Gain</b> (Total Exp Gain > 180%)\n   = 0.36";

            var baseExpBonus = __instance.baseExpBonusPerDish();
            message += "\n\n<b>Exp Bonus Gain</b> (before meal efficiency):"
                + "\n   = 0.005 × [base gain] × [cooking multi]"
                + $"\n   = {baseExpBonus} (clamped)";

            var max = totalCookingMulti * 0.005f;
            var min = max * 0.36f;
            //message += $"\n(clamped: {min} to {max})";
            message += $"\n\n(max = [cooking multi] × 0.005 = {max})"
                + $"\n(min = [max] * 0.36 = {min})";

            var totalBonusGain = baseExpBonus * __instance.getCurPercentofMaxScore();
            message += $"\n\n   ... × [efficiency] = {totalBonusGain} (additive)"
                + $"\n<b>New Total Exp Gain:</b> {(curExpBonus + totalBonusGain + 1f) * 100f:###.##}%";
            character.tooltip.showOverrideTooltip(message);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IngredientPodUI), "raiseIngredient")]
        private static bool IngredientPodUI_raiseIngredient_prefix()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            SetOptimalLevels();
            Plugin.Character.cookingController.updateMenu();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IngredientPodUI), "lowerIngredient")]
        private static bool IngredientPodUI_lowerIngredient_prefix()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return true;

            ClearIngredientLevels();
            Plugin.Character.cookingController.updateMenu();

            return false;
        }

        private static int GetCookingItemCount()
        {
            var character = Plugin.Character;
            var count = 0;

            if (character.inventory.head.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.head.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.head.spec3Type == specType.Cooking)
                count++;

            if (character.inventory.chest.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.chest.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.chest.spec3Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec3Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.legs.spec3Type == specType.Cooking)
                count++;

            if (character.inventory.boots.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.boots.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.boots.spec3Type == specType.Cooking)
                count++;

            if (character.inventory.weapon.spec1Type == specType.Cooking)
                count++;

            if (character.inventory.weapon.spec2Type == specType.Cooking)
                count++;

            if (character.inventory.weapon.spec3Type == specType.Cooking)
                count++;

            for (int i = 0; i < character.inventory.accs.Count; i++)
            {
                if (character.inventory.accs[i] != null)
                {
                    if (character.inventory.accs[i].spec1Type == specType.Cooking)
                        count++;

                    if (character.inventory.accs[i].spec2Type == specType.Cooking)
                        count++;

                    if (character.inventory.accs[i].spec3Type == specType.Cooking)
                        count++;

                }
            }

            return count;
        }

        private static void ClearIngredientLevels()
        {
            Plugin.Character.cooking.ingredients.ForEach(i => i.curLevel = 0);
        }

        private static void SetOptimalLevels()
        {
            Plugin.Character.cooking.ingredients.Do((i, x) => i.curLevel = getOptimalLevel(x));
        }

        private static int getOptimalLevel(int index)
        {
            var pair = pairs.FirstOrDefault(p => p.i1Index == index);
            if (pair != null)
                return pair.i1OptimalLevel;

            pair = pairs.FirstOrDefault(p => p.i2Index == index);
            if (pair != null)
                return pair.i2OptimalLevel;

            Plugin.LogInfo($"CookingHelper: failed to find pair with index {index}");
            return -1;
        }

        private class IngredientPair
        {
            internal int i1Index;
            internal int i2Index;

            internal int i1OptimalLevel = 0;
            internal int i2OptimalLevel = 0;
            internal float maxScore = 0f;

            internal int pairTarget;

            private Cooking cooking;
            private CookingController controller;
            private int pairNumber;

            internal IngredientPair(int pair)
            {
                pairNumber = pair;
                cooking = Plugin.Character.cooking;
                controller = Plugin.Character.cookingController;

                switch (pairNumber)
                {
                    case 1:
                        i1Index = cooking.pair1[0];
                        i2Index = cooking.pair1[1];
                        pairTarget = cooking.pair1Target;
                        break;

                    case 2:
                        i1Index = cooking.pair2[0];
                        i2Index = cooking.pair2[1];
                        pairTarget = cooking.pair2Target;
                        break;

                    case 3:
                        i1Index = cooking.pair3[0];
                        i2Index = cooking.pair3[1];
                        pairTarget = cooking.pair3Target;
                        break;

                    case 4:
                        i1Index = cooking.pair4[0];
                        i2Index = cooking.pair4[1];
                        pairTarget = cooking.pair4Target;
                        break;
                }

                //var maxScore = 0f;
                for (var i1Level = 0; i1Level <= INGREDIENT_MAX_LEVEL; i1Level++)
                {
                    for (var i2Level = 0; i2Level <= INGREDIENT_MAX_LEVEL; i2Level++)
                    {
                        var score = GetPairScore(i1Level, i2Level);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            i1OptimalLevel = i1Level;
                            i2OptimalLevel = i2Level;
                        }
                    }
                }
            }

            internal float GetPairScore(int i1Level, int i2Level)
            {
                var score = 0f;

                var i1Unlocked = controller.ingredientUnlocked(i1Index);
                var i2Unlocked = controller.ingredientUnlocked(i2Index);

                if (i1Unlocked)
                    score += controller.getLocalScore(i1Index, i1Level) + controller.getLocalScore(i2Index, i1Level);

                if (i2Unlocked)
                    score += controller.getLocalScore(i1Index, i2Level) + controller.getLocalScore(i2Index, i2Level);

                if (i1Unlocked && i2Unlocked)
                    score += controller.getPairedScore(pairNumber, i1Level + i2Level);

                return score;
            }
        }
    }
}
