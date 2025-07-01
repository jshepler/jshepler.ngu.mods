using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardTagsPanel
    {
        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "updateTagText")]
        private static bool CardsController_updateTagText_prefix(int tagID, CardsController __instance)
        {
            if (!__instance.character.InMenu(Menu.Cards) || !__instance.tagPanelShown)
                return false;

            var bonusType = (cardBonus)tagID;
            var bonusName = __instance.getShortBonusName(bonusType);
            var tier = __instance.generateCardTier(bonusType);
            var maxTier = (tagID - 1) switch
            {
                6 => 10,
                12 => 15,
                13 => 13,
                _ => 17
            };

            __instance.tagUI[tagID - 1].tagText.text = $"  {bonusName} ({tier} / {maxTier})";
            __instance.tagUI[tagID - 1].tagText.alignment = UnityEngine.TextAnchor.MiddleLeft;

            return false;
        }
    }
}
