using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class TimeMachineEnergyAllocator : BaseAllocator
    {
        private static Character _character;
        private static TimeMachineController _controller;
        private static TimeMachineEnergyAllocator Instance = new();

        internal TimeMachineEnergyAllocator() : base(1)
        {
            Allocators.Energy.Add(Allocators.Feature.TM_Energy, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.machine.speedEnergy += amount;
            _controller.updateSpeedText();
        }

        internal override long CalcCapDelta(int id)
        {
            var tm = _controller.character.machine;
            var cap = CalcCapForLevel(tm.levelSpeed + 1);

            var delta = cap - tm.speedEnergy;
            if (tm.speedEnergy + delta < 0)
                delta = -tm.speedEnergy;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _controller.hitSpeedLevelTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            _controller = __instance;
            _character = __instance.character;
            Instance.TextComponents[0] = __instance.transform.parent.Find("Machine Speed Panel/Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TimeMachineController), "addEnergy")]
        private static bool TimeMachineController_addEnergy_prefix(TimeMachineController __instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[0] = !Instance[0];
                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Options.Allocators.OverCapAllocatorEnabled.Value == true)
            {
                Instance[0] = false;
                OverCap();
                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "removeEnergy")]
        private static void TimeMachineController_removeEnergy_postfix(TimeMachineController __instance)
        {
            Instance[0] = false;
        }

        private static long CalcCapForLevel(long level)
        {
            var cap = 50000f * level * _controller.baseSpeedDivider() /
                (
                    (double)_character.totalEnergyPower()
                    * (double)_character.allChallenges.timeMachineChallenge.TMSpeedBonus()
                    * (double)_character.hacksController.totalTMSpeedBonus()
                    * (double)_character.cardsController.getBonus(cardBonus.TMSpeed)
                );

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return ((long)cap) + 1;
        }

        private static void OverCap()
        {
            if (_character.machine.speedTarget == -1)
                return;

            var currentLevel = (ulong)_character.machine.levelSpeed;
            var targetLevel = (ulong)_character.machine.speedTarget;
            if (targetLevel > 0 && currentLevel >= targetLevel)
                return;

            if (targetLevel == 0)
            {
                var targetRbSeconds = (ulong)_character.input.energyMagicInput * 60;
                var curRebirthSeconds = (ulong)_character.rebirthTime.totalseconds;
                if (targetRbSeconds <= curRebirthSeconds)
                    return;

                var gainLevels = (targetRbSeconds - curRebirthSeconds) * 50;
                targetLevel = currentLevel + gainLevels;
                if (targetLevel > long.MaxValue)
                    targetLevel = long.MaxValue;
            }

            _character.timeMachineController.removeAllEnergy();

            var cap = CalcCapForLevel((long)targetLevel);
            if (cap > _character.idleEnergy)
                cap = _character.idleEnergy;

            Instance.Allocate(0, cap);
            _character.idleEnergy -= cap;
        }
    }
}
