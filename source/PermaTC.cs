using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class PermaTC
    {
        private const float TROLL_SECONDS = 120f;
        private static bool Enabled = Options.GameModes.PermaTC.Value;
        internal static bool IsPermaTCGame => Enabled && ModSave.Data.PermaTC;

        private static TrollChallengeController _controller;
        private static bool _challengesUnlocked => _controller.character.challenges.unlocked;
        
        private static bool _running = false;

        private static float _timer
        {
            get => ModSave.Data.PTC_Timer;
            set => ModSave.Data.PTC_Timer = value;
        }

        private static int _counter
        {
            get => ModSave.Data.PTC_TrollCount;
            set => ModSave.Data.PTC_TrollCount = value;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                // if mode disabled, make sure game isn't flagged as PermaTC
                // this prevents starting a new game with it on, turning it off for whatever reason, then turning it back on
                if (!Enabled)
                    ModSave.Data.PermaTC = false;
            };

            Plugin.OnUpdate += (o, e) =>
            {
                if (!_running)
                    return;

                _timer += Time.deltaTime;
                if (_timer >= TROLL_SECONDS)
                {
                    _timer = 0f;
                    _counter++;

                    if (_counter % 5 == 0)
                        _controller.doBigTroll();
                    else
                        _controller.doSmallTroll();
                }
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TrollChallengeController), "Start")]
        private static void TrollChallengeController_Start_postfix(TrollChallengeController __instance)
        {
            _controller = __instance;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static bool Character_addOfflineProgress_prefix(Character __instance)
        {
            // when loading a save, pause running while the offline progress screen is up
            _running = false;

            if (!IsPermaTCGame)
                return true;

            __instance.splashScreen.message = "No Offline Progress for PermaTC";
            __instance.splashScreen.openScreen();

            return false;
        }

        // a save doesn't get the flag set unless it's started with option enabled
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            ModSave.Data.PermaTC = Enabled;
        }

        // don't start running until the new game intro closes or the offline progress screen closes
        [HarmonyPrefix,
            HarmonyPatch(typeof(StartMenu), "hideMenu"),
            HarmonyPatch(typeof(OfflineProgressSplashScreen), "closeScreen")]
        private static void OfflineProgressSplashScreen_closeScreen_prefix()
        {
            _running = IsPermaTCGame;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthButtonHover), "updateText")]
        private static void RebirthButtonHover_updateText_postfix(RebirthButtonHover __instance)
        {
            if (!_running)
                return;

            var nextTrollSeconds = TROLL_SECONDS - _timer;
            var text = $"\n[PTC] troll #{_counter + 1} in {NumberOutput.timeOutput(nextTrollSeconds)}";

            __instance.rebirthTimeText.text += text;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", [typeof(bool)])]
        private static void Rebirth_engate_postfix(bool hardReset, Rebirth __instance)
        {
            if (!IsPermaTCGame)
                return;

            __instance.character.allChallenges.trollChallenge.resetTrolls();

            if (hardReset)
            {
                _timer = 0;
                _counter = 0;
            }
        }
    }
}
