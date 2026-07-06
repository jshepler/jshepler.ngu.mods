using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
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

            string text;

            if (Plugin.ShiftIsDown)
            {
                Plugin.SetTooltipFont(Fonts.LiberationMono_Regular);
                text = getShiftText();
            }

            else
            {
                Plugin.ResetTooltipFont();
                text = Plugin.AltIsDown ? getAltText() : getNormalText();
            }

            Plugin.ShowTooltip(text);
            return false;
        }

        private static string getNormalText()
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            // per frame, cardSpawnTimer.totalseconds += Time.deltaTime * cardSpeed, so totalSeconds is scaled time, not real time
            var cardSpeed = controller.totalCardSpeed();

            // so to get real time, need to /cardSpeed
            var cardSpawnTime = controller.cardSpawnTime() / cardSpeed;
            var timeToNextCard = cardSpawnTime - character.cards.cardSpawnTimer.totalseconds / cardSpeed;

            // cardSpawnTime is already in real time, no need to /cardSpeed again
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
                text += $"\n  {x+1} - {controller.getBonusName(tags[x])}";

            var nextCard = getNextCard(false, out var fromTag);
            var mayoString = $"{nextCard.manaCosts.Sum()} mayo";
            var rarityString = $"{controller.getRarityColorTag(nextCard.cardRarity)}{controller.getRarityNameShort(nextCard.cardRarity)}</color>";
            var bonusTypeString = controller.getShortBonusName(nextCard.bonusType);

            var fromTagString = string.Empty;
            if (tags.Contains(nextCard.bonusType))
                fromTagString = fromTag ? "(from tag)" : "(not from tag)";

            text += "\n\n<b>Next Card:</b>"
                + $"\n  {rarityString} {mayoString} {bonusTypeString} {fromTagString}";

            if (character.cardsController.unlockedChonkers())
            {
                nextCard = getNextCard(true, out fromTag);
                mayoString = $"{nextCard.manaCosts.Sum()} mayo";
                rarityString = $"{controller.getRarityColorTag(nextCard.cardRarity)}{controller.getRarityNameShort(nextCard.cardRarity)}</color>";
                bonusTypeString = controller.getShortBonusName(nextCard.bonusType);

                fromTagString = string.Empty;
                if (tags.Contains(nextCard.bonusType))
                    fromTagString = fromTag ? "(from tag)" : "(not from tag)";

                text += "\n\n<b>Next Chonker:</b>"
                    + $"\n  {rarityString} {mayoString} {bonusTypeString} {fromTagString}";
            }

            return text;
        }

        private static Card getNextCard(bool isChonker, out bool fromTag)
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            // from generateCard()
            // mostly identical but with no side effects or unnecessary ops since this is just for display
            var currentState = UnityEngine.Random.state;
            UnityEngine.Random.state = isChonker ? character.cards.chonkerState : character.cards.cardState;

            // need these to advance the rng state
            var nounID = controller.getNounID();
            var cardName = controller.generateCardName(nounID);

            fromTag = false;

            // from generateBonusType() - need to do this ourselves to capture if the bonusType is from being tagged or not
            int max = Enum.GetNames(typeof(cardBonus)).Length;
            float value = UnityEngine.Random.value;
            cardBonus bonusType = (cardBonus)UnityEngine.Random.Range(1, max);
            int a = controller.curTagCount();
            for (int i = 0; i < Mathf.Min(a, controller.maxTagSize()); i++)
            {
                if (value < controller.tagEffect() * (i + 1))
                {
                    bonusType = character.cards.taggedBonuses[i];
                    fromTag = true; // <-- this is why we couldn't just call generateBonusType()
                    break;
                }
            }

            // don't need this, as tier isn't displayed, but I might change my mind
            //var cardTier = controller.generateCardTier(bonusType);
            //if (character.arbitrary.cardTierUpperCount > 0)
            //    cardTier += 2;

            var manaCosts = isChonker ?
                controller.generateManaCosts(controller.minChonkerMana(), controller.maxChonkerMana() + 1)
                : controller.generateManaCosts(controller.minCardMana(), controller.maxCardMana() + 1);

            // only used for cardEffect below, but not doing that so no need to do this, keeping in case needed in the future
            //var totalCost = manaCosts.Sum();

            var variance = controller.generateVariance();
            if (isChonker)
                variance = controller.getMaxVariance();

            var rarity = controller.generateRarity(variance, isChonker);
            //var cardEffect = controller.generateCardEffect(bonusType, cardTier, totalCost, variance, isChonker);
            //var cardType = controller.generateCardType();

            var card = new Card(0, nounID, cardName, 0f, cardType.normal, bonusType, rarity, manaCosts[0], manaCosts[1], manaCosts[2], manaCosts[3], manaCosts[4], manaCosts[5]);

            UnityEngine.Random.state = currentState;
            return card;
        }

        private static string getShiftText()
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            (var tagCount, var tagEffect, var singleUntaggedChance, var singleTaggedChance) = getBonusChances();
            var rarityChances = getRarityChances(controller.getMinVariance(), controller.getMaxVariance());

            var sb = new StringBuilder();

            var tt = new TextTable(defaultAlignment: TextTable.TextAlign.Right, spacing: 1);
            tt.AddRow(new TextTable.Cell("<b>Tag Slots Used:</b>")
                , new TextTable.Cell($"{tagCount}", alignment: TextTable.TextAlign.Left));
            tt.AddRow("<b>Tag Effect:</b>", $"{tagEffect * 100f:0.00}%");
            //tt.AddRow("<b>Tag Slots Used:</b>", $"{tagCount}");
            tt.AddRow("<b>Tag Slot:</b>", $"{tagEffect * tagCount * 100f:0.00}%");
            tt.AddRow("<b>No Tag Slot:</b>", $"{(1f - tagEffect * tagCount) * 100f:0.00}%");
            tt.AddRow("<b>Fallback/Random:</b>", $"{1f / 14f * 100f:0.00}%");
            sb.AppendLine($"{tt}");

            tt = new TextTable(defaultAlignment: TextTable.TextAlign.Right, spacing: 1);
            tt.AddRow("<b>Specific Untagged:</b>", $"{singleUntaggedChance * 100F:0.00}%");
            tt.AddRow("<b>Specific Tagged:</b>", $"{singleTaggedChance * 100f:0.00}%");
            sb.AppendLine($"\n{tt}");

            tt = buildSingleBonusTable(tagEffect, singleUntaggedChance, singleTaggedChance, rarityChances);
            sb.AppendLine($"{tt}");

            var anyUntaggedChance = (14 - tagCount) * singleUntaggedChance;
            var anyTaggedChance = tagCount * singleTaggedChance;
            tt = new TextTable(defaultAlignment: TextTable.TextAlign.Right, spacing: 1);
            tt.AddRow("<b>Any Untagged:</b>", $"{anyUntaggedChance * 100F:0.00}%");
            tt.AddRow("<b>Any Tagged:</b>", $"{anyTaggedChance * 100f:0.00}%");
            sb.AppendLine($"\n{tt}");

            tt = buildAnyBonusTable(tagCount, singleUntaggedChance, singleTaggedChance, rarityChances);
            sb.Append($"{tt}");

            return sb.ToString();
        }

        private static TextTable buildSingleBonusTable(float tagEffect, float singleUntaggedChance, float singleTaggedChance, Dictionary<rarity, float> rarityChances)
        {
            var controller = Plugin.Character.cardsController;

            var tt = new TextTable(defaultAlignment: TextTable.TextAlign.Right);
            tt.AddRow("<b>Rarity</b>", "<b>Base</b>", "<b>Untagged</b>", "<b>Tagged</b>");
            tt.AddSeparatorRow();

            foreach (var kvp in rarityChances)
            {
                var rarity = kvp.Key;
                var rarityName = controller.getRarityName(rarity);
                var rarityChance = kvp.Value;
                var color = controller.getRarityColorTag(rarity);

                var combinedUntaggedChance = rarityChance * singleUntaggedChance;
                var combinedTaggedChance = rarityChance * singleTaggedChance;

                tt.AddRow($"{color}{rarityName}</color>", $"{rarityChance * 100f:0.00}%", $"{combinedUntaggedChance * 100f:0.00}%", $"{combinedTaggedChance * 100f:0.00}%");
            }

            if (Options.Cards.AutoYeetMode.Value == CardYeetMode.Rarity)
            {
                var minCast = Options.Cards.MaxYeetRarity.Value + 1;
                var minCastName = controller.getRarityName(minCast);
                var minCastColor = controller.getRarityColorTag(minCast);

                var cumulative = rarityChances.Values.Skip((int)minCast).Sum();
                var cumUntagged = cumulative * singleUntaggedChance;
                var cumTagged = cumulative * singleTaggedChance;

                tt.AddSeparatorRow();
                tt.AddRow($"{minCastColor}{minCastName}</color>+", $"{cumulative * 100f:0.00}%", $"{cumUntagged * 100f:0.00}%", $"{cumTagged * 100f:0.00}%");
            }

            return tt;
        }

        private static TextTable buildAnyBonusTable(int tagCount, float singleUntaggedChance, float singleTaggedChance, Dictionary<rarity, float> rarityChances)
        {
            var controller = Plugin.Character.cardsController;

            var anyUntaggedChance = (14 - tagCount) * singleUntaggedChance;
            var anyTaggedChance = tagCount * singleTaggedChance;

            var tt = new TextTable(defaultAlignment: TextTable.TextAlign.Right);
            tt.AddRow("<b>Rarity</b>", "<b>Base</b>", "<b>Untagged</b>", "<b>Tagged</b>");
            tt.AddSeparatorRow();

            foreach (var kvp in rarityChances)
            {
                var rarity = kvp.Key;
                var rarityName = controller.getRarityName(rarity);
                var rarityChance = kvp.Value;
                var color = controller.getRarityColorTag(rarity);

                var combinedUntaggedChance = rarityChance * anyUntaggedChance;
                var combinedTaggedChance = rarityChance * anyTaggedChance;

                tt.AddRow($"{color}{rarityName}</color>", $"{rarityChance * 100f:0.00}%", $"{combinedUntaggedChance * 100f:0.00}%", $"{combinedTaggedChance * 100f:0.00}%");
            }

            if (Options.Cards.AutoYeetMode.Value == CardYeetMode.Rarity)
            {
                var minCast = Options.Cards.MaxYeetRarity.Value + 1;
                var minCastName = controller.getRarityName(minCast);
                var minCastColor = controller.getRarityColorTag(minCast);

                var cumulative = rarityChances.Values.Skip((int)minCast).Sum();
                var cumUntagged = cumulative * anyUntaggedChance;
                var cumTagged = cumulative * anyTaggedChance;

                tt.AddSeparatorRow();
                tt.AddRow($"{minCastColor}{minCastName}</color>+", $"{cumulative * 100f:0.00}%", $"{cumUntagged * 100f:0.00}%", $"{cumTagged * 100f:0.00}%");
            }

            return tt;
        }

        /*
            P(tag) = probability that this bonus is selected by its tag (e.g. 10%)
            P(random | no tag) = probability that this bonus is selected if no tag triggers = 1/number of bonus types (14)
            P(no tag) = probability that no tag triggers = 1 - totalTagChance (tagChance * number of tag slots)

            The total probability of getting this bonus is:
                P(bonus)=P(tag)+P(no tag)×P(random | no tag)
         */
        private static (int, float, float, float) getBonusChances()
        {
            var character = Plugin.Character;
            var controller = character.cardsController;

            var tagCount = controller.curTagCount();
            var maxTags = controller.maxTagSize();

            // P(tag)
            var tagChance = controller.tagEffect();

            // P(random | no tag)
            var baseBonusChance = 1f / 14f;// Enum.GetNames(typeof(cardBonus)).Length;

            // P(no tag)
            var totalTagChance = Mathf.Min(tagCount, maxTags) * tagChance;
            var noTagChance = Mathf.Max(1f - totalTagChance, 0f);

            var untaggedChance = noTagChance * baseBonusChance;
            var taggedChance = tagChance + untaggedChance;

            return (tagCount, tagChance, untaggedChance, taggedChance);
        }

        private static Dictionary<rarity, float> getRarityChances(float minVariance, float maxVariance)
        {
            float totalRange = maxVariance - minVariance;

            Dictionary<rarity, float> chances = new Dictionary<rarity, float>();

            float[] thresholds = { 0.9f, 1.0f, 1.08f, 1.14f, 1.17f, 1.19f, 1.2f };
            //rarity[] rarities = { rarity.Crappy, rarity.Bad, rarity.Meh, rarity.Okay, rarity.Good, rarity.Great, rarity.Damn };

            float lower = minVariance;
            for (int i = 0; i < 7; i++)
            {
                float upper = Mathf.Min(thresholds[i], maxVariance);
                float width = Mathf.Max(upper - lower, 0f);
                chances[(rarity)i] = width / totalRange;
                lower = thresholds[i];
            }

            return chances;
        }
    }
}
