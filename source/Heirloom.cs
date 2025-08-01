using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Heirloom
    {
        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            GameObject.Find("Canvas/End Panel (6)/Button")
                .AddComponent<ClickHandlerComponent>()
                .OnRightClick(resetGame);

            Plugin.OnGameStart += (o, e) => Plugin.Character.endFinish();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "showEndSequence")]
        private static bool Character_showEndSequence_prefix(int id, Character __instance)
        {
            foreach (GameObject endPanel in __instance.endPanels)
            {
                endPanel.transform.localPosition = new Vector3(-5000f, -5000f);
                endPanel.SetActive(false);
            }

            __instance.endPanels[id].transform.localPosition = new Vector3(0f, 0f);
            __instance.endPanels[id].SetActive(true);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "endFinish")]
        private static bool Character_endFinish_prefix(Character __instance)
        {
            foreach (GameObject endPanel in __instance.endPanels)
            {
                endPanel.transform.localPosition = new Vector3(-5000f, -5000f);
                endPanel.SetActive(false);
            }

            return false;
        }

        private static void resetGame(PointerEventData d)
        {
            var character = Plugin.Character;

            // hold ref to trashed item (the heirloom) to add later as hardReset() will clear it
            var trashed = character.inventory.trash;

            // holding a ref to check for existing pack purchases after everything has been reset
            var oldArbitrary = character.arbitrary;

            character.hardReset();

            // some things that aren't done in hardReset()
            character.cards = new();
            character.cooking = new();
            character.cookingController.assignNewDish();
            character.arbitrary = new();

            // while achievements are reset in hardReset(), the controller needs to update itself for the AP bonus, before adding packs
            character.allAchievements.checkAchievements();

            // clearing the mod data before adding packs so the AP gained will get tracked properly
            ModSave.Data.Values.Clear();
            TrackAPGained.Reset();
            TrackBaseAdvPowerGained.Reset();
            TrackCubeBoosts.Reset();
            TrackResourcesGained.Reset();

            if (oldArbitrary.boughtNewbiePack)
                character.steamAPI.consumeNewPlayerPurchase();

            if (oldArbitrary.boughtAscendedNewbiePack)
                character.steamAPI.consumeAscendedNewbiePurchase();

            if (oldArbitrary.boughtAscendedNewbiePack2)
                character.steamAPI.consumeAscendedNewbiePurchase2();

            if (oldArbitrary.boughtAscendedNewbiePack3)
                character.steamAPI.consumeAscendedNewbiePurchase3();

            if (oldArbitrary.boughtAscendedNewbiePack4)
                character.steamAPI.consumeAscendedNewbiePurchase4();

            if (oldArbitrary.boughtFashionPack1)
                character.steamAPI.consumeFashionPack1();

            if (oldArbitrary.boughtRes3Pack)
                character.steamAPI.consumeRes3Purchase();

            for (var x = 0; x < oldArbitrary.nameSlotsBought; x++)
                character.steamAPI.consumeITOPODNamePack();

            // finally, add the heirloom item to inventory
            if (trashed != null)
                character.itemInfo.addLoot(trashed);

            // and close THE END popup
            character.endFinish();
        }
    }
}
