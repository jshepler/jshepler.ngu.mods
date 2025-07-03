using System;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ImprovedNumberBreakdown
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_prefix(Rebirth __instance)
        {
            var character = __instance.character;

            Data.LastBossMultiCombined = character.bossMulti * character.oldBossMulti;
            Data.LastTimeMultiCombined = character.timeMulti * character.oldTimeMulti;
            Data.LastTrainingFactor = character.training.totalAttackLevels / 10000 + 1;
            Data.LastNGUNumberBonus = character.NGUController.numberBonus();
            Data.LastBeardNumberBonus = character.allBeards.numberBonus();
            Data.LastYggNumberBonus = character.yggdrasilController.permNumberBonus();
            Data.LastGuffNumberBonus = character.inventory.macguffinBonuses[17];
            Data.LastHackNumberBonus = character.hacksController.totalNumberBonus();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerDisplay), "Start")]
        private static void RebirthPowerDisplay_Start_postfix(RebirthPowerDisplay __instance)
        {
            adjust(__instance.RebirthInfoText);
            adjust(__instance.rebirthInfoValues);
        }

        private static void adjust(Text t)
        {
            var rt = t.rectTransform;
            rt.position = new Vector3(rt.position.x, rt.position.y + 6);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 10);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerDisplay), "Update")]
        private static void RebirthPowerDisplay_Update_postfix(RebirthPowerDisplay __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Rebirth)
                || (character.challenges.blindChallenge.inChallenge && character.allChallenges.blindChallenge.completions() >= 4))
                return;

            var disp = character.display;
            var diffFactor = __instance.difficultyFactor();
            var lastBossId = (int)Math.Log(character.oldBossMulti, diffFactor);
            var curBossMultiCombined = character.bossMulti * character.oldBossMulti;
            var curTimeMultiCombined = character.timeMulti * character.oldTimeMulti;
            var trainingFactor = character.training.totalAttackLevels / 10000 + 1;

            var labels = "Boss Power Bonus: "
                + "\n   this rebirth: "
                + "\n   last rebirth: "
                + "\nRebirth Time Factor: "
                + "\n   this rebirth: "
                + "\n   last rebirth: "
                + "\nTraining level Factor: ";

            var values = $"x {buildString(curBossMultiCombined, Data.LastBossMultiCombined)}"
                + $"\n   {diffFactor:r} ^ {character.bossID} = {disp(character.bossMulti)}"
                + $"\n   {diffFactor:r} ^ {lastBossId} = {disp(character.oldBossMulti)}"
                + $"\nx {buildString(curTimeMultiCombined, Data.LastTimeMultiCombined, "0.000000")}"
                + $"\n   {character.timeMulti:0.000000}"
                + $"\n   {character.oldTimeMulti:0.000000}"
                + $"\nx {buildString(trainingFactor, Data.LastTrainingFactor)}";

            if (character.bossID > 36)
            {
                labels += "\nBlood Magic Bonus: ";
                values += $"\nx {buildString(character.bloodMagic.rebirthPower, character.stats.lastBloodMagic)}";
            }

            if (character.NGUController.numberBonus(noTimeMulti: false) > 1.0)
            {
                labels += "\nNGU NUMBER Bonus: ";
                values += $"\nx {buildString(character.NGUController.numberBonus(), Data.LastNGUNumberBonus)}";
            }

            var beardNumberBonus = character.allBeards.numberBonus();
            if (beardNumberBonus > 1f)
            {
                labels += "\nBeard NUMBER Bonus: ";
                values += $"\nx {buildString(beardNumberBonus, Data.LastBeardNumberBonus)}";
            }

            var yggNumberBonus = character.yggdrasilController.permNumberBonus();
            if (yggNumberBonus > 1.0)
            {
                labels += "\nYggdrasil NUMBER Bonus: ";
                values += $"\nx {buildString(yggNumberBonus, Data.LastYggNumberBonus)}";
            }

            var guffNumberBonus = character.inventory.macguffinBonuses[17];
            if (guffNumberBonus > 1f)
            {
                labels += "\nMacGuffin NUMBER Bonus: ";
                values += $"\nx {buildString(guffNumberBonus, Data.LastGuffNumberBonus)}";
            }

            var hackNumberBonus = character.hacksController.totalNumberBonus();
            if (hackNumberBonus > 1f)
            {
                labels += "\nHack NUMBER Bonus: ";
                values += $"\nx {buildString(hackNumberBonus, Data.LastHackNumberBonus)}";
            }

            __instance.RebirthInfoText.text = labels;
            __instance.rebirthInfoValues.text = values;

            var time = (long)__instance.character.rebirthTime.totalseconds - 3600;
            if (time < 0)
                time = 0L;

            var ap = character.checkAPAdded(time / 500);
            __instance.rebirthChange.text += $"\nYou will gain {ap} AP if you rebirth now.";
        }

        const string RED = "#B22735"; // D82E3F

        private static string buildString(long cur, long last)
        {
            var diff = cur - last;
            var color = diff == 0L ? "black" : diff < 0 ? RED : "green";
            var sign = diff > 0L ? "+" : string.Empty;

            return $"{Plugin.Character.display(cur)}  <color={color}>({sign}{Plugin.Character.display(diff)})</color>";
        }

        private static string buildString(float cur, float last)
        {
            var diff = cur - last;
            var color = diff == 0f ? "black" : diff < 0 ? RED : "green";
            var sign = diff > 0f ? "+" : string.Empty;

            return $"{Plugin.Character.display(cur)}  <color={color}>({sign}{Plugin.Character.display(diff)})</color>";
        }

        private static string buildString(double cur, double last, string format = null)
        {
            var diff = cur - last;
            var color = diff == 0.0 ? "black" : diff < 0 ? RED : "green";
            var sign = diff > 0.0 ? "+" : string.Empty;

            if (format == null)
                return $"{Plugin.Character.display(cur)}  <color={color}>({sign}{Plugin.Character.display(diff)})</color>";

            return $"{cur.ToString(format)}  <color={color}>({sign}{diff.ToString(format)})</color>";
        }
    }
}
