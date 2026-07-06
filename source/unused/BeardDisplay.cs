using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BeardDisplay
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeardController), "Start")]
        private static void BeardController_Start_postfix(BeardController __instance)
        {
            GameObject.Find("Canvas/Beard Canvas/Beard Menu + AllBeardController/Text").SetActive(false);

            adjust(__instance.levelText);
            adjust(__instance.tempValue);
            adjust(__instance.permValue);

            __instance.tempBonus.alignment = TextAnchor.MiddleRight;
            __instance.permBonus.alignment = TextAnchor.MiddleRight;
        }

        private static void adjust(Text textbox)
        {
            textbox.alignment = TextAnchor.UpperRight;

            var rt = textbox.rectTransform;
            rt.position = new Vector3(rt.position.x, rt.position.y + 10);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 30);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeardController), "updateText")]
        private static bool BeardController_updateText_prefix(BeardController __instance)
        {
            var character = Plugin.Character;
            if (character == null || !character.InMenu(Menu.Beards))
                return false;

            var id = __instance.id;
            var beard = character.beards.beards[id];

            __instance.levelText.text = "Temporary:\nPermanent:\nTotal:";
            __instance.tempBonus.text = "<b>Level</b>";
            __instance.permBonus.text = $"<b>{bonusText(id)}</b>";
            __instance.tempValue.text = $"{beard.beardLevel:#,##0}\n{beard.permLevel:#,##0}";

            if (beard.active)
                __instance.permValue.text = $"{tempBonus(id):#,##0.00}%";
            else
                __instance.permValue.text = "<color=red>100%</color>";

            __instance.permValue.text += $"\nx{permBonus(id):#,##0.00}%\n{totalBonus(id):#,##0.00}%";

            __instance.activeBeardsText.text = $"Active:\n{character.beards.activeBeards.Count}/{character.allBeards.capBeards()}";
            return false;
        }

        private static string bonusText(int id)
        {
            return id switch
            {
                0 => "Atack/Defense Bonus",
                1 => "Drop Chance Bonus",
                2 => "Number Bonus",
                3 => "NGU Speed Bonus",
                4 => "Wandoos Speed Bonus",
                5 => "Adventure Bonus",
                6 => "GPS Bonus",
                _ => string.Empty
            };
        }

        private static double tempBonus(int id)
        {
            var controller = Plugin.Character.allBeards;

            var bonus = id switch
            {
                0 => controller.tempStatBonus(true),
                1 => controller.tempLootBonus(true),
                2 => controller.tempNumberBonus(true),
                3 => controller.tempNGUBonus(true),
                4 => controller.tempWandoosBonus(true),
                5 => controller.tempAdventureBonus(true),
                6 => controller.tempGoldBonus(true),
                _ => 0f
            };

            return bonus * 100.0;
        }

        private static double permBonus(int id)
        {
            var controller = Plugin.Character.allBeards;

            var bonus = id switch
            {
                0 => controller.permStatBonus(),
                1 => controller.permLootBonus(),
                2 => controller.permNumberBonus(),
                3 => controller.permNGUBonus(),
                4 => controller.permWandoosBonus(),
                5 => controller.permAdventureBonus(),
                6 => controller.permGoldBonus(),
                _ => 0f
            };

            return bonus * 100.0;
        }

        private static double totalBonus(int id)
        {
            var controller = Plugin.Character.allBeards;

            var bonus = id switch
            {
                0 => controller.statBonus(),
                1 => controller.lootBonus(),
                2 => controller.numberBonus(),
                3 => controller.nguBonus(),
                4 => controller.wandoosBonus(),
                5 => controller.adventureBonus(),
                6 => controller.goldBonus(),
                _ => 0f
            };

            return bonus * 100.0;
        }
    }
}
