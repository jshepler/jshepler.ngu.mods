using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoButter
    {
        private static bool _enabled
        {
            get => Options.Questing.AutoButter.Value;
            set => Options.Questing.AutoButter.Value = value;
        }

        private static Button _button;

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "Start")]
        private static void BeastQuestController_Start_postfix(BeastQuestController __instance)
        {
            var gob = GameObject.Find("Canvas/Beast Quest Canvas/Beast Quest Menu/Butter Button");
            if (gob == null)
                return;

            _button = gob.GetComponent<Button>();
            gob.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (!Plugin.ShiftIsDown)
                        return;

                    _enabled = !_enabled;
                    setColor();

                    if(_enabled)
                        useButter(__instance);
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static void BeastQuestController_updateText_postfix()
        {
            setColor();
        }

        private static void setColor()
        {
            _button.image.color = _enabled ? Plugin.ButtonColor_LightBlue : Color.white;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "startQuest")]
        private static void BeastQuestController_startQuest_postfix(BeastQuestController __instance)
        {
            useButter(__instance);
        }

        private static void useButter(BeastQuestController controller)
        {
            var character = controller.character;
            if (_enabled
                && character.arbitrary.beastButterCount > 0
                && character.settings.beastOn
                && character.settings.useMajorQuests
                && character.beastQuest.inQuest
                && !character.beastQuest.idleMode
                && !character.beastQuest.usedButter)

                controller.tryUseButter();
        }
    }
}
