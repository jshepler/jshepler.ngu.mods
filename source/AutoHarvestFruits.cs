using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoHarvestFruits
    {
        private static bool _enabled
        {
            get => Options.Yggdrasil.AutoHarvest.Value;
            set => Options.Yggdrasil.AutoHarvest.Value = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            var button = __instance.yggdrasil;
            button.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e =>
                {
                    if (Plugin.ShiftIsDown)
                    {
                        _enabled = !_enabled;
                        setColor(button);
                    }
                });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static void ButtonShower_updateButtons(ButtonShower __instance)
        {
            if (__instance.character.arbitrary.hasYggdrasilReminder
                && __instance.character.yggdrasilController.anyFruitMaxxed())
                return;

            setColor(__instance.yggdrasil);
        }

        private static void setColor(Button button)
        {
            if (Plugin.Character == null || !Plugin.Character.settings.yggdrasilOn)
                return;

            button.image.color = _enabled ? Plugin.ButtonColor_LightBlue : Color.white;
        }

        // auto harvest/eat all max tier fruits when any are max tier
        [HarmonyPostfix, HarmonyPatch(typeof(AllYggdrasil), "updateFruitTimers")]
        private static void AllYggdrasil_updateFruitTimers_postfix(AllYggdrasil __instance)
        {
            if (_enabled && __instance.anyFruitMaxxed())
                __instance.consumeAll();
        }

        // auto harvest/east all fruits >= tier 1 on rebirth
        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_bool_prefix(Rebirth __instance)
        {
            __instance.character.yggdrasilController.consumeAll(true);
        }
    }
}
