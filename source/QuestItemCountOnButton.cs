using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestItemCountOnButton
    {
        private static Character _character;
        private static Text _questButtonTextComponenet;
        private static int _invItemCount = 0;

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null) return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                if (InManualQuest())
                    SetInvItemCount();
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _character = __instance.character;
            _questButtonTextComponenet = __instance.beast.GetComponentInChildren<Text>();
        }

        private static bool _questItemDropped = false;

        [HarmonyPostfix, HarmonyPatch(typeof(ItemNameDesc), "makeLoot", typeof(int))]
        private static void ItemNameDesc_makeLoot_postfix(int id, ItemNameDesc __instance)
        {
            if (InManualQuest() && id == _character.beastQuest.questID)
            {
                _questItemDropped = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemNameDesc), "addLoot")]
        private static void ItemNameDesc_addLoot_postfix(int __result)
        {
            if (_questItemDropped && __result >= 0)
            {
                if (_invItemCount < 1)
                    SetInvItemCount();
                else
                    _invItemCount++;
            }

            _questItemDropped = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Inventory), "deleteItem", typeof(int))]
        private static void Inventory_deleteItem_prefix(int id, Inventory __instance)
        {
            //Plugin.LogInfo($"slotId: {id}, InManualQuest: {InManualQuest()}, itemID: {__instance.inventory[id].id}, questID: {_character.beastQuest.questID}");
            //Plugin.LogInfo($"_invItemCount (before): {_invItemCount}");
            if (InManualQuest() && __instance.inventory[id].id == _character.beastQuest.questID)
            {
                _invItemCount--;
            }
            //Plugin.LogInfo($"_invItemCount (after): {_invItemCount}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "startQuest")]
        private static void BeastQuestController_startQuest_postfix()
        {
            if (InManualQuest())
                SetInvItemCount();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "clearQuest")]
        private static void BeastQuestController_clearQuest_postfix()
        {
            _invItemCount = 0;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons_postifx(ButtonShower __instance)
        {
            _questButtonTextComponenet.text = "Questing";
            
            var quest = _character.beastQuest;
            if (!InManualQuest() || quest.curDrops >= quest.targetDrops)
            {
                return;
            }

            var total = quest.curDrops + _invItemCount;
            var diff = quest.targetDrops - total;

            __instance.beast.image.color =
                diff > 5 ? Color.white
                : diff <= 0 ? Plugin.ButtonColor_Green
                : Plugin.ButtonColor_Yellow;

            _questButtonTextComponenet.text += $" {total}/{quest.targetDrops}";
        }

        private static void SetInvItemCount()
        {
            _invItemCount = _character.inventory.inventory.Sum(e => e.id == _character.beastQuest.questID ? 1 : 0);
        }

        private static bool InManualQuest()
        {
            return _character.settings.beastOn && _character.beastQuest.inQuest && !_character.beastQuest.idleMode;
        }
    }
}
