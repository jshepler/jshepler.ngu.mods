using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class TimeMachineMagicAllocator : BaseAllocator
    {
        private static Character _character;
        private static TimeMachineController _controller;
        private static TimeMachineMagicAllocator Instance = new();

        internal TimeMachineMagicAllocator() : base(1)
        {
            Allocators.Magic.Add(Allocators.Feature.TM_Magic, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.machine.goldMultiMagic += amount;
            _controller.updateGoldMultiText();
        }

        internal override long CalcCapDelta(int id)
        {
            var tm = _character.machine;
            var cap = CalcCapForLevel(tm.levelGoldMulti + 1);

            var delta = cap - tm.goldMultiMagic;
            if (tm.goldMultiMagic + delta < 0)
                delta = -tm.goldMultiMagic;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _controller.hitMultiLevelTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "Start")]
        private static void TimeMachineController_Start_postfix(TimeMachineController __instance)
        {
            _controller = __instance;
            _character = __instance.character;
            Instance.TextComponents[0] = __instance.transform.parent.Find("Gold Multi Panel/Add/Text").GetComponent<Text>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TimeMachineController), "addMagic")]
        private static bool TimeMachineController_addMagic_prefix(TimeMachineController __instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[0] = !Instance[0];

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    var energy = Allocators.Energy[Allocators.Feature.TM_Energy];
                    energy[0] = !energy[0];
                }

                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Options.Allocators.OverCapAllocatorEnabled.Value == true)
            {
                Instance[0] = false;
                OverCap();

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    Allocators.Magic[Allocators.Feature.TM_Energy][0] = false;
                    TimeMachineEnergyAllocator.OverCap();
                }

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "removeMagic")]
        private static void TimeMachineController_removeMagic_postfix(TimeMachineController __instance)
        {
            Instance[0] = false;

            if (Input.GetKey(KeyCode.LeftShift))
                __instance.removeAllMagic();
        }

        private static long CalcCapForLevel(long level)
        {
            var cap = 50000f * level * _controller.baseGoldMultiDivider() /
                (
                    _character.totalMagicPower()
                    * (double)_character.allChallenges.timeMachineChallenge.TMSpeedBonus()
                    * (double)_character.hacksController.totalTMSpeedBonus()
                    * (double)_character.cardsController.getBonus(cardBonus.TMSpeed)
                );

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return ((long)cap) + 1;
        }

        internal static void OverCap()
        {
            if (_character.machine.multiTarget == -1)
                return;

            var currentLevel = (ulong)_character.machine.levelGoldMulti;
            var targetLevel = (ulong)_character.machine.multiTarget;
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

            _character.timeMachineController.removeAllMagic();

            var cap = CalcCapForLevel((long)targetLevel);
            if (cap > _character.magic.idleMagic)
                cap = _character.magic.idleMagic;

            Instance.Allocate(0, cap);
            _character.magic.idleMagic -= cap;
        }
    }
}