using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BasicTrainingBarsFadedUntilMaxCapReduction
    {
        const float FADED = 0.25f;
        private static Image[] _offenseFills = new Image[6];
        private static Image[] _defenseFills = new Image[6];

        [HarmonyPostfix, HarmonyPatch(typeof(OffenseTraining), "Start")]
        private static void OffenseTraining_Start_postfix(OffenseTraining __instance)
        {
            _offenseFills[__instance.id] = __instance.gameObject.transform.Find("Train Progress/Fill Area/Fill").GetComponent<Image>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DefenseTraining), "Start")]
        private static void DefenseTraining_Start_postfix(DefenseTraining __instance)
        {
            _defenseFills[__instance.id] = __instance.gameObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OffenseTraining), "levelUp")]
        private static void OffenseTraining_levelUp_postfix(OffenseTraining __instance)
        {
            var id = __instance.id;
            var training = __instance.character.training;

            var curLevel = training.attackTraining[id];
            var curCap = training.attackCaps[id];
            var maxReduction = curCap / 10 + 1;
            var maxReductionLevel = maxReduction < 1 ? 0L : (long)(Mathf.Pow((maxReduction - 1f) / (curCap / 1000f) * 500f, 1f / 1.2f) + (500f * id));

            var color = _offenseFills[id].color;
            color.a = curLevel < maxReductionLevel ? FADED : 1.0f;
            _offenseFills[id].color = color;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DefenseTraining), "levelUp")]
        private static void DefenseTraining_levelUp_postfix(DefenseTraining __instance)
        {
            var id = __instance.id;
            var training = __instance.character.training;

            var curLevel = training.defenseTraining[id];
            var curCap = training.defenseCaps[id];
            var maxReduction = curCap / 10 + 1;
            var maxReductionLevel = maxReduction < 1 ? 0L : (long)(Mathf.Pow((maxReduction - 1f) / (curCap / 1000f) * 500f, 1f / 1.2f) + (500f * id));

            var color = _defenseFills[id].color;
            color.a = curLevel < maxReductionLevel ? FADED : 1.0f;
            _defenseFills[id].color = color;
        }
    }
}
