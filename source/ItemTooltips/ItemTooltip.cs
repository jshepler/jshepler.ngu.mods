using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace jshepler.ngu.mods.ItemTooltips
{
    internal class ItemTooltip
    {
        private int _slotId;
        private bool _showDaycareLevel;
        private bool _showEstBoostTimes;
        private bool _showEmptySlotMessage;

        private Action _updateMessage;
        private Func<string> _getMessage;

        private Coroutine _cor = null;

        internal ItemTooltip(
            int slotId
            , Action updateMessage
            , Func<string> getMessage
            , bool showDaycareLevel = false
            , bool showEstBoostTimes = false
            , bool showEmptySlotMessage = true)
        {
            _slotId = slotId;
            _updateMessage = updateMessage;
            _getMessage = getMessage;
            _showDaycareLevel = showDaycareLevel;
            _showEstBoostTimes = showEstBoostTimes;
            _showEmptySlotMessage = showEmptySlotMessage;
        }

        internal void Show()
        {
            if (_cor != null)
                Plugin.EndCoroutine(_cor);

            _cor = Plugin.BeginCoroutine(ShowTooltip());
        }

        internal void Hide()
        {
            Plugin.HideTooltip();

            if (_cor != null)
                Plugin.EndCoroutine(_cor);

            _cor = null;
        }

        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private IEnumerator ShowTooltip()
        {
            var character = Plugin.Character;

            while (true)
            {
                var item = Plugin.Character.inventory.GetItem(_slotId);
                if (item.id == 0 && !_showEmptySlotMessage)
                    break;

                _updateMessage();
                var sb = new StringBuilder(_getMessage());

                if (Plugin.AltIsDown && _showEstBoostTimes && item.isEquipment())
                    sb.Append(Boosts.BuildEstBoostTimes(item));

                if (_showDaycareLevel)
                    sb.Append(buildDaycareString(item));

                if (_slotId == -6)
                    sb.Append($"\n\n<b>Dual-Wield Effectiveness:</b> {character.inventoryController.weapon2Factor() * 100f:0}%");

                if (item.isMacGuffin())
                {
                    var muff = character.arbitrary.macGuffinBooster1Time.totalseconds > 0.0 || character.arbitrary.macGuffinBooster1InUse;
                    sb.Append($"\n\n<b>Time Factor:</b> x{character.inventoryController.macGuffinBonusTimeFactor()}{(muff ? " (muffin active)" : string.Empty)}");
                }

                if (item.isBoost())
                    sb.Append(buildBoostValuesString(item));

                if (item.id == 92 && item.level > 0 && character.settings.yggdrasilOn)
                    sb.Append($"\n\n<b>Gain <color=blue>{(int)(item.level * (1f + item.level / 100f))}</color> seeds if consumed now</b>");

                if (Plugin.AltIsDown)
                    sb.Append(ItemSources.BuildString(item));

                character.tooltip.showOverrideTooltip(sb.ToString());
                yield return _wait;
            }

            Hide();
        }

        private static string buildDaycareString(Equipment item)
        {
            if (item == null || item.id == 0)
                return null;

            var daycare = Plugin.Character.inventory.daycare;
            var dcId = -1;

            for (var x = 0; x < daycare.Count; x++)
                if (daycare[x].id == item.id)
                    dcId = x;

            if (dcId == -1)
                return null;

            var controller = Plugin.Character.inventoryController.daycares[dcId];
            var dcLevel = daycare[dcId].level + controller.levelsAdded();
            var text = $"\n\n<b>Item level in Daycare:</b> {dcLevel}";

            if (item.type != part.MacGuffin)
            {
                // level + 1 to account for the extra level gained when merging
                var afterMerge = Math.Min(100, dcLevel + item.level + 1);
                var timeToMaxLevel = Math.Max(0, DaycareBarText.TimeToMaxLevel(controller, item.level + 1));

                text += $" ({afterMerge} after merge)"
                    + $"\n  ... time to 100 (merged): {NumberOutput.timeOutput(timeToMaxLevel)}";
            }

            return text;
        }

        private static string buildBoostValuesString(Equipment item)
        {
            var baseValue = Boosts.GetValue(item.id);
            var boostBonus = Plugin.Character.allItemList.boostBonus();
            var boostValue = baseValue * boostBonus;
            var cubeBoost = boostValue / InfinityCubeSoftCap.CubeBoostDivider;
            var text = $"\n     <b>To Cube:</b> {cubeBoost:#,##0.##}";

            var recycleChance = Math.Min(Plugin.Character.totalRecycleBonus(), 1f);
            if (recycleChance > 0)
            {
                var avgBoostWithRecycling = Boosts.GetAverageRecycledBoost(item.id, boostBonus, recycleChance);
                var avgCubeBoostWithRecycling = avgBoostWithRecycling / InfinityCubeSoftCap.CubeBoostDivider;

                var avgTag = (recycleChance > 0 && recycleChance < 1) ? " (avg)" : string.Empty;
                text += $"\n\n<b> ... with Boost Recycling ({recycleChance * 100f:0.#}%):</b> {avgBoostWithRecycling:#,##0.##}{avgTag}"
                    + $"\n     <b>To Cube:</b> {avgCubeBoostWithRecycling:#,##0.##}{avgTag}";
            }

            return text;
        }
    }
}
