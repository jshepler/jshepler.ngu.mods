using System;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedCardsStatus
    {

        [HarmonyPrefix, HarmonyPatch(typeof(ButtonShower), "showCardStatus")]
        private static bool ButtonShower_showCardStatus_prefix(ButtonShower __instance)
        {
            var character = __instance.character;
            if (!character.cards.cardsOn)
                return false;

            var text = Plugin.AltIsDown ? getAltText() : getNormalText();
            __instance.tooltip.showTooltip(text);

            return false;
        }

        private static string getNormalText()
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            var cardSpeed = controller.totalCardSpeed();
            var cardSpawnTime = controller.cardSpawnTime() / cardSpeed;
            var timeToNextCard = cardSpawnTime - character.cards.cardSpawnTimer.totalseconds / cardSpeed;
            var cardsPerDay = 86400f / cardSpawnTime;

            var text = $"<b>Card Spawn Time:</b> {NumberOutput.timeOutput(cardSpawnTime)}"
                + $"\n<b>Time to Next Card:</b> {NumberOutput.timeOutput(timeToNextCard)}"
                + $"\n<b>Cards per Day:</b> {cardsPerDay:#,##0.#}";

            if (character.cardsController.unlockedChonkers())
            {
                var chonkerSpawnTime = controller.chonkerSpawnTime() / cardSpeed;
                var timeToNextChonker = chonkerSpawnTime - character.cards.chonkerSpawnTimer.totalseconds / cardSpeed;

                text += $"\n\n<b>CHONKER Spawn Time:</b> {NumberOutput.timeOutput(chonkerSpawnTime)}"
                    + $"\n<b>Time to Next CHONKER:</b> {NumberOutput.timeOutput(timeToNextChonker)}";
            }

            text += $"\n\n<b>Total Cards Generated:</b> {character.cards.cardsGenerated:#,##0}";

            return text;
        }

        private static string getAltText()
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            var text = "<b>Tag Order:</b>";
            var tags = character.cards.taggedBonuses;
            for (var x = 0; x < tags.Count; x++)
                text += $"\n  {x+1} - {controller.getShortBonusName(tags[x])}";

            var nextCardBonus = getNextCardBonus(false, out var fromTag);

            var fromTagString = string.Empty;
            if (tags.Contains(nextCardBonus))
                fromTagString = fromTag ? "(from tag)" : "(not from tag)";

            text += "\n\n<b>Next Card:</b>"
                + $"\n   {controller.getShortBonusName(nextCardBonus)} {fromTagString}";

            var nextChonkerBonus = getNextCardBonus(true, out fromTag);

            fromTagString = string.Empty;
            if(tags.Contains(nextChonkerBonus))
                fromTagString = fromTag ? "(from tag)" : "(not from tag)";

            text += "\n\n<b>Next Chonker:</b>"
                + $"\n   {controller.getShortBonusName(nextChonkerBonus)} {fromTagString}";

            return text;
        }

        private static cardBonus getNextCardBonus(bool isChonker, out bool fromTag)
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            fromTag = false;

            // from generateCard()
            var currentState = UnityEngine.Random.state;
            if (isChonker)
            {
                UnityEngine.Random.state = character.cards.chonkerState;
            }
            else
            {
                UnityEngine.Random.state = character.cards.cardState;
            }

            // only doing these to use up the same amount of randoms
            var nounID = controller.getNounID();
            var cardName = controller.generateCardName(nounID);

            // from generateBonusType()
            int max = Enum.GetNames(typeof(cardBonus)).Length;
            float value = UnityEngine.Random.value;
            cardBonus result = (cardBonus)UnityEngine.Random.Range(1, max);
            int a = controller.curTagCount();
            for (int i = 0; i < Mathf.Min(a, controller.maxTagSize()); i++)
            {
                if (value < controller.tagEffect() * (i + 1))
                {
                    result = character.cards.taggedBonuses[i];
                    fromTag = true;
                    break;
                }
            }

            UnityEngine.Random.state = currentState;
            return result;
        }
    }
}
