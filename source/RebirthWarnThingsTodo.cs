using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RebirthWarnThingsTodo
    {
        private static Button _rbButton;
        private static Button _challButton;

        [HarmonyPrepare]
        private static void prep(MethodInfo method)
        {
            if (method != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                _rbButton = GameObject.Find("Canvas/Rebirth Canvas/Rebirth Menu/Rebirth Button").GetComponent<Button>();
                _challButton = GameObject.Find("Canvas/Rebirth Canvas/Rebirth Menu/Challenge Button").GetComponent<Button>();

                Plugin.BeginCoroutine(SetButtonColors());
            };
        }

        private static IEnumerator SetButtonColors()
        {
            var wait1 = new WaitForSeconds(1);
            var lscButton = Plugin.Character.allChallenges.laserSwordChallenge.challengeButton;

            while (true)
            {
                yield return wait1;

                _rbButton.image.color = HaveThingsTodo() ? Plugin.ButtonColor_Red : Color.white;

                var canDoLSC = CanDoLSC();
                _challButton.image.color = canDoLSC ? Plugin.ButtonColor_Yellow : Color.white;
                lscButton.image.color = canDoLSC ? Plugin.ButtonColor_Yellow : Color.white;
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

        private static bool CanDoLSC()
        {
            var maxSecondsToTarget = Options.LSCreminder.MaxMinutesToTarget.Value * 60;
            if (maxSecondsToTarget == 0)
                return false;

            var character = Plugin.Character;
            var chall = character.allChallenges.laserSwordChallenge;
            if (!chall.unlocked() || chall.currentCompletions() >= 20)
                return false;

            var challTarget = chall.laserSwordTarget();
            var totalEnergy = character.totalCapEnergy();

            var augSeconds = Calculators.AugCalculators[6].TimeToTarget(0, challTarget, totalEnergy, 0f);
            var upgSeconds = Calculators.AugUpgradeCalculators[6].TimeToTarget(0, challTarget, totalEnergy, 0f);

            return (augSeconds + upgSeconds) <= maxSecondsToTarget;
        }
    }
}
