using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoManualMajorQuesting
    {
        private static Character _character;
        private static BeastQuestController _controller;
        
        private static bool _enabled
        {
            get => ModSave.Data.AutoQuestingEnabled;
            set => ModSave.Data.AutoQuestingEnabled = value;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;
            
            // after offline progression finishes, disable if was enabled and no longer in a manual major quest
            Plugin.OnOfflineProgressionComplete += (o, e) =>
            {
                _enabled = _enabled && InManualQuest() && !_character.beastQuest.reducedRewards; //_character.settings.useMajorQuests;
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_start_postfix(ButtonShower __instance)
        {
            _character = __instance.character;
            _controller = _character.beastQuestController;

            __instance.beast.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (!_character.settings.beastOn)
                        return;

                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        _enabled = !_enabled;
                        __instance.beast.image.color = _enabled ? Plugin.ButtonColor_LightBlue : Color.white;

                        StartManualMajorQuest();
                    }
                    else if (InManualQuest())
                    {
                        CollectQuestItems();
                    }
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons"), HarmonyPriority(Priority.LowerThanNormal)]
        private static void ButtonShower_updateButtons_postifx(ButtonShower __instance)
        {
            if (!_enabled)
                return;

            __instance.beast.image.color = Plugin.ButtonColor_LightBlue;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemNameDesc), "makeLoot", typeof(int))]
        private static void ItemNameDesc_makeLoot_postfix(int id)
        {
            if (!_enabled || id != _character.beastQuest.questID)
                return;

            CollectQuestItems();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "startQuest")]
        private static void BeastQuestController_startQuest_postfix(BeastQuestController __instance)
        {
            if (!_enabled || !InManualQuest() || !__instance.character.arbitrary.goToQuestZoneBought)
                return;

            var itemId = _character.beastQuest.questID;
            if (!GameData.Quests.ZoneItemIDs.ContainsValue(itemId))
                return;

            var questZone = GameData.Quests.ZoneItemIDs.First(kv => kv.Value == itemId).Key;
            if (_character.adventure.zone != questZone)
                _character.adventureController.zoneSelector.changeZone(questZone);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "completeQuest")]
        private static void BeastQuestController_completeQuest_postfix(BeastQuestController __instance)
        {
            if (!_enabled)
                return;

            // let's player indicate to not continue running more majors by unchecking "User Majors"
            if (!_character.settings.useMajorQuests)
            {
                stopAutoManualQuesting();
                return;
            }

            StartManualMajorQuest();
        }

        // if auto enabled and player switches to idle questing, disable auto
        [HarmonyPrefix, HarmonyPatch(typeof(BeastQuestController), "toggleIdleMode")]
        private static void BeastQuestController_toggleIdleMode_prefix(BeastQuestController __instance)
        {
            if (_enabled && !__instance.character.beastQuest.idleMode)
                _enabled = false;
        }

        private static void CollectQuestItems()
        {
            if (!InManualQuest())
                return;

            var quest = _character.beastQuest;
            _character.inventoryController.dumpAllIntoQuest(quest.questID);

            if (quest.curDrops >= quest.targetDrops)
                _controller.startOrCompleteQuest();
        }

        private static void StartManualMajorQuest()
        {
            var quest = _character.beastQuest;

            // safety check - should never be true
            if (!_enabled)
                return;

            // if already doing a major quest, no need to do anything other than check if it's idle
            if (quest.inQuest && !quest.reducedRewards)
            {
                if (quest.idleMode)
                    _controller.toggleIdleMode();

                return;
            }

            if (quest.curBankedQuests < 1)
            {
                stopAutoManualQuesting();
                return;
            }

            if (quest.inQuest)
                _controller.skipQuest();

            if (quest.idleMode)
                _controller.toggleIdleMode();

            if (!_character.settings.useMajorQuests)
                _controller.toggleMajorQuestUse();

            _controller.startQuest();
            _controller.refreshMenu();
        }

        private static void stopAutoManualQuesting()
        {
            _enabled = false;

            if (_character.settings.useMajorQuests)
                _controller.toggleMajorQuestUse();

            _controller.startQuest();

            if (!_character.beastQuest.idleMode)
                _controller.toggleIdleMode();

            _controller.refreshMenu();
            _character.adventureController.zoneSelector.changeZone(1000);
        }

        private static bool InManualQuest()
        {
            return _character.settings.beastOn && _character.beastQuest.inQuest && !_character.beastQuest.idleMode;
        }
    }
}
