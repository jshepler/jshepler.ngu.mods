using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RebirthWarnThingsTodo
    {
        private static Button _button;

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "Awake")]
        private static void Character_Start_postfix(Character __instance)
        {
            Plugin.OnGameStart += (o, e) =>
            {
                _button = GameObject.Find("Canvas/Rebirth Canvas/Rebirth Menu/Rebirth Button").GetComponent<Button>();
                __instance.StartCoroutine(SetRebirthColor());
            };
        }

        private static IEnumerator SetRebirthColor()
        {
            var wait1 = new WaitForSeconds(1);
            while (true)
            {
                yield return wait1;

                _button.image.color = HaveThingsTodo() ? Plugin.ButtonColor_Red : Color.white;
            }
        }

        private static bool HaveThingsTodo()
        {
            var character = Plugin.Character;

            if (character.settings.pitUnlocked
                && !character.pit.tossedGold
                && character.realGold > 1000.0
                && character.pit.pitTime.totalseconds >= (double)character.pitController.currentPitTime())
                return true;

            if (character.bloodMagic.bloodPoints > 0.0 && !character.bloodMagicController.spells.castingAutoSpells())
                return true;

            if (character.bossID >= 58 && character.adventure.boss1Spawn.seconds >= (double)character.adventureController.boss1SpawnTime())
                return true;

            if (character.bossID >= 66 && character.adventure.boss2Spawn.seconds >= (double)character.adventureController.boss2SpawnTime())
                return true;

            if (character.bossID >= 82 && character.adventure.boss3Spawn.seconds >= (double)character.adventureController.boss3SpawnTime())
                return true;

            if (character.yggdrasil.fruits.Any(f => f.harvestTier() >= 1))
                return true;

            return false;
        }
    }
}
