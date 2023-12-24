using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TransformAllBoosts
    {
        private static InventoryController _controller;

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "Start")]
        private static void InventoryController_Start_postfix(InventoryController __instance)
        {
            _controller = __instance;

            _controller.powerToggle.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    _controller.selectAutoPowerTransform();
                    TransformAll(1);
                });

            _controller.toughToggle.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    _controller.selectAutoToughTransform();
                    TransformAll(2);
                });

            _controller.specialToggle.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    _controller.selectAutoSpecialTransform();
                    TransformAll(3);
                });
        }

        private static void TransformAll(int selection)
        {
            // don't really need this check as the buttons wouldn't be visible
            //if (_controller.character.allChallenges.level100Challenge.completions() >= _controller.character.allChallenges.level100Challenge.maxCompletions)
            //    return;

            _controller.character.settings.autoTransform = selection;
            _controller.updateTransformToggles();

            var numberOfInventorySlots = _controller.curSpaces();
            var inventory = _controller.character.inventory;
            var equipmentList = inventory.inventory;

            for (var slotIndex = 0; slotIndex < numberOfInventorySlots; slotIndex++)
            {
                var item = equipmentList[slotIndex];
                if (item == null || !item.isBoost() || !item.removable)
                    continue;
                
                var id = (item.id - 1) % 13 + 1 + (selection - 1) * 13;
                item.id = id;

                _controller.updateItem(slotIndex);
            }
        }
    }
}
