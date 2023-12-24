using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StartQuestInCurrentZone
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "constructQuestList")]
        private static void BeastQuestController_constructQuestList_postfix(BeastQuestController __instance)
        {
            var character = __instance.character;
            var quest = character.beastQuest;
            if (quest.idleMode)
                return;

            var currentZone = character.adventure.zone;
            if (!GameData.ZoneQuestItemIDs.ContainsKey(currentZone))
                return;

            // from BeastQuestController.constructQuestList()
            var itemList = character.inventory.itemList;
            var setUnlocked = currentZone switch
            {
                1 => true,
                2 => itemList.forestComplete,
                5 => itemList.HSBComplete,
                9 => itemList.twoDComplete,
                12 => itemList.gaudyComplete,
                13 => itemList.megaComplete,
                15 => itemList.beardverseComplete,
                20 => itemList.chocoComplete,
                21 => itemList.edgyComplete,
                22 => itemList.prettyComplete,
                _ => false
            };

            if (!setUnlocked)
                return;

            var questItemId = GameData.ZoneQuestItemIDs[currentZone];
            var possibleQuests = __instance.possibleQuests;
            if (!possibleQuests.Contains(questItemId))
                return;
            
            possibleQuests.Clear();
            possibleQuests.Add(questItemId);
        }
    }
}
