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
            
            Plugin.OnSaveLoaded += (o, e) =>
            {
                _enabled = _enabled && InManualQuest() && _character.settings.useMajorQuests;
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_start_postfix(ButtonShower __instance)
        {
            _character = __instance.character;

            __instance.beast.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (Input.GetKey(KeyCode.LeftShift))
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
            if (!_enabled || !InManualQuest())
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
            if (_enabled)
                StartManualMajorQuest();
        }

        private static void CollectQuestItems()
        {
            if (!InManualQuest())
                return;

            var quest = _character.beastQuest;
            _character.inventoryController.dumpAllIntoQuest(quest.questID);

            if (quest.curDrops >= quest.targetDrops)
                _character.beastQuestController.startOrCompleteQuest();
        }

        private static void StartManualMajorQuest()
        {
            var quest = _character.beastQuest;

            if (!_enabled
                || !_character.settings.beastOn
                || (quest.inQuest && !quest.idleMode))
                return;

            if (quest.curBankedQuests < 1)
            {
                if (_character.settings.useMajorQuests)
                    _character.beastQuestController.toggleMajorQuestUse();

                _character.beastQuestController.startQuest();

                if (!quest.idleMode)
                    _character.beastQuestController.toggleIdleMode();

                _enabled = false;
                return;
            }

            if (quest.inQuest)
                _character.beastQuestController.skipQuest();

            if (quest.idleMode)
                _character.beastQuestController.toggleIdleMode();

            if (!_character.settings.useMajorQuests)
                _character.beastQuestController.toggleMajorQuestUse();

            _character.beastQuestController.startQuest();
            _character.beastQuestController.refreshMenu();
        }

        private static bool InManualQuest()
        {
            return _character.settings.beastOn && _character.beastQuest.inQuest && !_character.beastQuest.idleMode;
        }
    }
}
