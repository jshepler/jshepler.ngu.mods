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

            // hold some refs to things that will be cleared but want to restore after hardReset()
            var heirloomItem = character.inventory.trash;
            var oldSettings = character.settings;
            var oldArbitrary = character.arbitrary;

            character.hardReset();
            character.nextRebirthDifficulty = difficulty.normal;

            // some things that aren't done in hardReset()
            character.cards = new();
            character.cooking = new();
            character.cookingController.assignNewDish();
            character.arbitrary = new();
            character.pit = new();
            character.portraits = new();
            character.bestiary = new();
            character.daily = new();

            // these states are not correctly initialized in hardReset() or in constructors
            character.beastQuest.questState = Random.state;
            character.cards.cardState = Random.state;
            character.cards.chonkerState = Random.state;
            character.daily.dailyRewardState = Random.state;
            character.pit.pitState = Random.state;

            // hardReset() keeps this as it was so couldn't get it again if already got it in previous save
            character.purchases.hasSpecialPrize1 = false;

            // Wandoos98Controller.Start() => enableWandoos() => sets installed to true
            // ImportExport.loadData() => Wandoos98.validateWandoos() => sets time to 86400.0
            character.wandoos98.installed = true;
            character.wandoos98.installTime.totalseconds = 86400.0;

            // while achievements are reset in hardReset(), the controller needs to update itself for the AP bonus, before adding packs
            character.allAchievements.checkAchievements();

            // copy over the stuff in the settings screen
            // lazy shifter isn't kept in settings, but that's ok since it's on the enter tower popup anyway
            character.settings.numberDisplay = oldSettings.numberDisplay;
            character.settings.themeID = oldSettings.themeID;
            character.settings.tooltipsOn = oldSettings.tooltipsOn;
            character.settings.timedTooltipsOn = oldSettings.timedTooltipsOn;
            character.settings.autoKillTitans = oldSettings.autoKillTitans;
            character.settings.checkForUpdates = oldSettings.checkForUpdates;
            character.settings.specialAdvHpBars = oldSettings.specialAdvHpBars;
            character.settings.antiFlickerBars = oldSettings.antiFlickerBars;
            character.settings.syncTraining = oldSettings.syncTraining;
            character.settings.filterOn = oldSettings.filterOn;
            character.settings.filterTitan = oldSettings.filterTitan;
            character.settings.autoboostRecycledBoosts = oldSettings.autoboostRecycledBoosts;
            character.settings.unassignWhenSwapping = oldSettings.unassignWhenSwapping;
            character.settings.expPopups = oldSettings.expPopups;
            character.settings.itopodConfirmation = oldSettings.itopodConfirmation;
            character.settings.shakeySales = oldSettings.shakeySales;
            character.settings.submitHighscores = oldSettings.submitHighscores;
            character.settings.beardPopup = oldSettings.beardPopup;
            character.settings.fancyYggBars = oldSettings.fancyYggBars;
            character.settings.simpleInvShortcuts = oldSettings.simpleInvShortcuts;
            character.settings.autoNukeOn = oldSettings.autoNukeOn;
            character.settings.assholeSetting = oldSettings.assholeSetting; // autosave timer
            character.settings.invAutoMergeOn = oldSettings.invAutoMergeOn;
            character.settings.invAutoBoostOn = oldSettings.invAutoBoostOn;
            character.settings.foilsOn = oldSettings.foilsOn;

            // re-enable the krissmiss 2019 rewards
            character.settings.prizePicked = 6; // krissmuss theme
            character.inventory.unlockedKittyArt[2] = true; // santa daycare kitty

            // clearing the mod data before adding packs so the AP gained will get tracked properly
            ModSave.Data.Values.Clear();
            TrackAPGained.Reset();
            TrackBaseAdvPowerGained.Reset();
            TrackCubeBoosts.Reset();
            TrackResourcesGained.Reset();

            // copy over purchased packs: the 5 newbie packs, the res3 pack, the portait pack, and ITOPOD pack(s)
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

            if (oldArbitrary.boughtRes3Pack)
                character.steamAPI.consumeRes3Purchase();

            if (oldArbitrary.boughtFashionPack1)
                character.steamAPI.consumeFashionPack1();

            for (var x = 0; x < oldArbitrary.nameSlotsBought; x++)
                character.steamAPI.consumeITOPODNamePack();


            if (heirloomItem != null)
                character.itemInfo.addLoot(heirloomItem);

            character.endFinish();

            foreach (var cr in character.introMenu.startMenu.GetComponentsInChildren<CanvasRenderer>())
                cr.SetAlpha(1f);

            character.mainMenu.startNewGame();
        }
    }
}
