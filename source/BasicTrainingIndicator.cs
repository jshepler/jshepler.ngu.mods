using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BasicTrainingIndicator
    {
        private static Button _button;
        private static Toggle _autoAdvanceToggle;
        private static Coroutine _coroutine;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _button = __instance.basicTraining;

            Plugin.OnOfflineProgressionComplete += (o, e) =>
            {
                _autoAdvanceToggle = Plugin.Character.allOffenseController.autoAdvance;

                if (_coroutine != null)
                    __instance.StopCoroutine(_coroutine);

                _coroutine = __instance.StartCoroutine(UpdateColor(Plugin.Character.training));
            };
        }

        private static IEnumerator UpdateColor(Training training)
        {
            var purchases = Plugin.Character.purchases;
            var wait = new WaitForSeconds(0.1f);

            while (true)
            {
                var buttonOn = false;

                if (!purchases.hasAutoAdvance || !_autoAdvanceToggle.isOn)
                {
                    for (var x = 0; x < 5; x++)
                    {
                        if ((training.attackTraining[x] > (x + 1) * 5000 && training.attackTraining[x + 1] == 0)
                            || (training.defenseTraining[x] > (x + 1) * 5000 && training.defenseTraining[x + 1] == 0))
                        {
                            buttonOn = true;
                            break;
                        }
                    }
                }

                _button.image.color = buttonOn ? Plugin.ButtonColor_Yellow : Color.white;
                yield return wait;
            }
        }
    }
}
