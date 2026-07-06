using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PauseGame
    {
        internal static bool IsPaused = false;

        private static float _timer = 0f;
        private static bool _blinkOff = false;

        private static Text _mainScreen;
        private static string _currentMainScreenText;

        private static Text _rebirthScreen;
        private static string _currentRebirtScreenText;

        // rebirth button tooltip is handled in TrackLastRebirth mod

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthButtonHover), "Start")]
        private static void RebirthButtonHover_Start_postfix(RebirthButtonHover __instance)
        {
            Plugin.OnUpdate += OnUpdate;

            _mainScreen = __instance.rebirthTimeText;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LittleRebirthTimer), "Start")]
        private static void LittleRebirthTimer_Start_postfix(LittleRebirthTimer __instance)
        {
            _rebirthScreen = __instance.timerText;
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= 0.5)
            {
                _blinkOff = !_blinkOff;
                _timer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.Pause))
            {
                IsPaused = !IsPaused;
                Time.timeScale = IsPaused ? 0f : 1f;

                if (IsPaused)
                {
                    _currentMainScreenText = _mainScreen.text;
                    _currentRebirtScreenText = _rebirthScreen.text;
                }
            }

            if (!IsPaused)
                return;

            if (_blinkOff)
            {
                _mainScreen.text = string.Empty;
                _rebirthScreen.text = string.Empty;
            }

            else
            {
                _mainScreen.text = _currentMainScreenText + " (PAUSED)";
                _rebirthScreen.text = _currentRebirtScreenText + " (PAUSED)";
            }
        }
    }
}
