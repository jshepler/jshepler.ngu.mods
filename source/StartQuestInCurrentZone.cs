using System.Linq.Expressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.TextCore;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StartQuestInCurrentZone
    {
        private static bool _getRandomQuest = false;

        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "startQuest")]
        private static void BeastQuestController_startQuest_prefix()
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                _getRandomQuest = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "startQuest")]
        private static void BeastQuestController_startQuest_postfix(BeastQuestController __instance)
        {
            _getRandomQuest = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "constructQuestList")]
        private static void BeastQuestController_constructQuestList_postfix(BeastQuestController __instance)
        {
            if (Options.Questing.AlwaysRandom.Value == true || _getRandomQuest)
                return;

            var character = __instance.character;
            if (character.beastQuest.idleMode)
                return;

            var currentZone = character.adventure.zone;
            if (!GameData.Quests.ZoneItemIDs.ContainsKey(currentZone))
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

            var questItemId = GameData.Quests.ZoneItemIDs[currentZone];
            var possibleQuests = __instance.possibleQuests;
            if (!possibleQuests.Contains(questItemId))
                return;
            
            possibleQuests.Clear();
            possibleQuests.Add(questItemId);
        }
    }
}
