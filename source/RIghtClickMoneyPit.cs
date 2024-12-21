using System.Reflection;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RIghtClickMoneyPit
    {
        private static MethodInfo _tossGoldMethod = typeof(PitController).GetMethod("engage", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            __instance.pit.gameObject
                .AddComponent<ClickHandlerComponent>()
                .OnRightClick(OnRightClick);
        }

        private static void OnRightClick(PointerEventData e)
        {
            var character = Plugin.Character;
            var pitController = character.pitController;

            if (character.pit.pitTime.totalseconds < (double)pitController.currentPitTime())
                return;

            _tossGoldMethod.Invoke(pitController, []);
            Plugin.ShowNotification(pitController.pitText.text);
        }
    }
}
