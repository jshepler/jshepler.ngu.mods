using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoMayoGen
    {
        internal enum Mode { OFF, LOW, ALL }
        
        private static Mode _mode
        {
            get => (Mode)ModSave.Data.AutoMayGenMode;
            set => ModSave.Data.AutoMayGenMode = (int)value;
        }

        private static Button _castButton;
        private static Button _protButton;
        private static Button _yeetButton;

        private static Text _castText;
        private static Text _protText;
        private static Text _yeetText;

        private static bool _altIsDown = false;
        private static bool _allocateWhenManaPodUpdates = false;


        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "Start")]
        private static void CardsController_Start_postfix(CardsController __instance)
        {
            _castButton = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Cast Card Button").GetComponent<Button>();
            _protButton = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Protect Card button").GetComponent<Button>();
            _yeetButton = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Trash Card Button").GetComponent<Button>();

            _castText = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Cast Card Button/Text").GetComponent<Text>();
            _protText = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Protect Card button/Text").GetComponent<Text>();
            _yeetText = GameObject.Find("Canvas/Cards Canvas/Cards Menu/Trash Card Button/Text").GetComponent<Text>();

            Plugin.OnSaveLoaded += (o, e) => AllocateGenerators();

            Plugin.OnUpdate += (o, e) =>
            {
                if (Plugin.Character.InMenu(Menu.Cards) == false)
                    return;

                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    _castText.text = "OFF";
                    _protText.text = "LOW";
                    _yeetText.text = "ALL";
                    _altIsDown = true;

                    SetButtonColors();
                }

                else if (Input.GetKeyUp(KeyCode.LeftAlt))
                {
                    _castText.text = "Cast";
                    _protText.text = "Protect";
                    _yeetText.text = "Yeet";
                    _altIsDown = false;

                    SetButtonColors();
                }
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "tryConsumeCurrentCard")]
        private static bool CardsController_tryConsumeCurrentCard_prefix()
        {
            if (!_altIsDown)
                return true;

            ChangeMode(Mode.OFF);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "protectCurrentCard")]
        private static bool CardsController_protectCurrentCard_prefix()
        {
            if (!_altIsDown)
                return true;

            ChangeMode(Mode.LOW);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CardsController), "trashCurrentCard")]
        private static bool CardsController_trashCurrentCard_prefix()
        {
            if (!_altIsDown)
                return true;

            ChangeMode(Mode.ALL);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardsController), "updateManaGens")]
        [HarmonyPatch(typeof(CardsController), "postYeetEffect")]
        private static void CardsController_updateManaGens_prefix()
        {
            _allocateWhenManaPodUpdates = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardsController), "updateManaGens")]
        [HarmonyPatch(typeof(CardsController), "postYeetEffect")]
        private static void CardsController_updateManaGens_postfix()
        {
            _allocateWhenManaPodUpdates = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "updateManaPod")]
        private static void CardsController_updateManaPod_postfix(CardsController __instance)
        {
            if (!Plugin.Character.cards.cardsOn || !_allocateWhenManaPodUpdates)
                return;

            AllocateGenerators();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "tryConsumeCard")]
        private static void CardsController_tryConsumeCard_postfix()
        {
            AllocateGenerators();
        }

        private static void AllocateGenerators()
        {
            if (_mode == Mode.OFF)
                return;

            var gens = Plugin.Character.cards.manas.OrderBy(m => m.amount).ToList();
            var maxActive = Plugin.Character.cardsController.maxManaGenSize();
            var lowest = gens[0].amount;

            gens.Do(m => m.running = false);

            if (_mode == Mode.LOW)
                gens = gens.Where(m => m.amount == lowest).ToList();

            gens.Take(maxActive).Do(m => m.running = true);
            Plugin.Character.cardsController.updateManaPods();
        }

        private static void ChangeMode(Mode mode)
        {
            _mode = mode;
            AllocateGenerators();
            SetButtonColors();
        }

        private static void SetButtonColors()
        {
            _castButton.image.color = (_altIsDown && _mode == Mode.OFF) ? Plugin.ButtonColor_Green : Color.white;
            _protButton.image.color = (_altIsDown && _mode == Mode.LOW) ? Plugin.ButtonColor_Green : Color.white;
            _yeetButton.image.color = (_altIsDown && _mode == Mode.ALL) ? Plugin.ButtonColor_Green : Color.white;
        }
    }
}
