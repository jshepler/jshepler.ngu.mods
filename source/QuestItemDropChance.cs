using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestItemDropChance
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static void BeastQuestController_updateText_postfix(BeastQuestController __instance)
        {
            __instance.questStats.text += $"\n<b>Quest Item Drop Chance:</b> {__instance.questDropChance() * 100: #,##0.##}%";
            return;

#pragma warning disable CS0162 // Unreachable code detected
            var character = __instance.character;
#pragma warning restore CS0162 // Unreachable code detected
            var quest = character.beastQuest;
            if (!quest.inQuest)
                return;

            var qp = new QP(character);

            var text = $"Fetch {quest.targetDrops} of <b>{__instance.questItemName()}</b>"
                     + $"\nFound in: {__instance.questItemLocation(quest.questID)}"
                     + $"\n\nThis is <b>{(quest.allActive ? "a manual" : "an idle")} {(quest.reducedRewards ? "Minor" : "Major")} Quest";

            //text += $"\n<b>Base QP:</b> {baseQP}"
            //      + $"\n <b>From Perks:</b> +{fromPerks}"
            //      + $"\n <b>From Wishes:</b> +{fromWishes}";
        }

        class QP
        {
            private Character character;

            internal int baseQP = 0;
            internal int addFromPerks = 0;
            internal int addFromWishes = 0;

            internal double multFromOrangeHeart = 1.0;
            internal double multFromGMset = 1.0;
            internal int questItemsMaxxed = 0;
            internal double multFromMaxxedQuestItems = 1.0;
            internal double multFromPerks = 1.0;
            internal double multFromHacks = 1.0;
            internal double multFromWishes = 1.0;
            internal double multFromFib = 1.0;

            internal double multForAllActive = 1.0;

            public QP(Character c)
            {
                character = c;

                if (character.beastQuest.reducedRewards)
                    setMinorBase();
                else
                    setMajorBase();

                setMultipliers();
            }

            private void setMinorBase()
            {
                baseQP = 10;
                addFromPerks = (int)character.adventure.itopod.perkLevel[87] * 2;
                addFromWishes = 0;

                if (character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    addFromPerks += (int)character.adventure.itopod.perkLevel[148];
                    addFromWishes += character.wishes.wishes[102].level;
                }
            }

            private void setMajorBase()
            {
                baseQP = 50;
                addFromPerks = 0;
                addFromWishes = 0;

                if (character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    addFromPerks += (int)character.adventure.itopod.perkLevel[147];
                    addFromWishes += character.wishes.wishes[101].level;
                }
            }

            private void setMultipliers()
            {
                multFromGMset = character.inventory.itemList.godmotherComplete ? 1.15 : 1.0;
                questItemsMaxxed = Enumerable.Range(278, 10).Count(i => character.inventory.itemList.itemMaxxed[i]);
                multFromMaxxedQuestItems = Mathf.Pow(1.02f, questItemsMaxxed);
                multFromPerks = character.adventureController.itopod.totalQPBonus();
                multFromHacks = character.hacksController.totalQPGainBonus();
                multFromWishes = character.wishesController.totalQPBonus();
                multFromOrangeHeart = character.inventory.itemList.orangeHeartComplete ? 1.2 : 1.0;
                multFromFib = character.adventure.itopod.perkLevel[94] >= 233 ? 1.1 : 1.0;
                multForAllActive = character.beastQuest.allActive ? (2.0 * character.wishesController.wishEffect(19) * character.wishesController.wishEffect(62)) : 1.0;
            }
        }
    }
}
