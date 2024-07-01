using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestItemDropChance
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static void BeastQuestController_updateText_postfix(BeastQuestController __instance)
        {
            __instance.questStats.text += $"\n<b>Quest Item Drop Chance:</b> {__instance.questDropChance() * 100: #,##0.##}%";
        }
    }
}
