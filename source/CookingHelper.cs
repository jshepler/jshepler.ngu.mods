using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CookingHelper
    {
        const int INGREDIENT_MAX_LEVEL = 20;
        private static bool _firstLoad = false;

        private static DateTime CookingReadyAtUtc;

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

            Plugin.OnSaveLoaded += (o, e) =>
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
            if (!_firstLoad)
                return;

            var index = __instance.ingredientDataIndex;
            var pairIndex = pairs.FindIndex(p => p.i1Index == index || p.i2Index == index);
            var iTarget = Plugin.Character.cooking.ingredients[index].targetLevel;
            var pTarget = pairs[pairIndex].pairTarget;

            __instance.nameText.text = $"P{pairIndex + 1}: {__instance.nameText.text} ({iTarget}:{pTarget})";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CookingController), "updateDishUI")]
        private static void CookingController_updateDishUI_postfix(CookingController __instance)
        {
            var eatRate = __instance.eatRate();
            var cookTimer = Plugin.Character.cooking.cookTimer;
            var readyAt = DateTime.Now.AddSeconds(eatRate - cookTimer);

            var daysDiff = readyAt.Day - DateTime.Now.Day;
            var dayString = daysDiff switch
            {
                1 => "tomorrow",
                0 => "today",
                -1 => "yesterday",
                _ => $"{daysDiff} days ago"
            };
            
            __instance.nextdishTimerText.text += $"\nReady {dayString} at: {readyAt:h:mm:ss tt}";
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

            internal int pairTarget;

            internal IngredientPair(int pair)
            {
                var cooking = Plugin.Character.cooking;
                var controller = Plugin.Character.cookingController;

                switch (pair)
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

                var i1Unlocked = controller.ingredientUnlocked(i1Index);
                var i2Unlocked = controller.ingredientUnlocked(i2Index);

                var maxScore = 0f;
                for (var i1Level = 0; i1Level <= INGREDIENT_MAX_LEVEL; i1Level++)
                {
                    for (var i2Level = 0; i2Level <= INGREDIENT_MAX_LEVEL; i2Level++)
                    {
                        var score = 0f;

                        if (i1Unlocked)
                            score += controller.getLocalScore(i1Index, i1Level) + controller.getLocalScore(i2Index, i1Level);

                        if (i2Unlocked)
                            score += controller.getLocalScore(i1Index, i2Level) + controller.getLocalScore(i2Index, i2Level);

                        if (i1Unlocked && i2Unlocked)
                            score += controller.getPairedScore(pair, i1Level + i2Level);

                        if (score > maxScore)
                        {
                            maxScore = score;
                            i1OptimalLevel = i1Level;
                            i2OptimalLevel = i2Level;
                        }
                    }
                }
            }
        }
    }
}
