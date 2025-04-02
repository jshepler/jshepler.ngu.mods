using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
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
            var tm = _character.machine;
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
            if (Plugin.ShiftIsDown)
            {
                Instance[0] = !Instance[0];

                if (Plugin.AltIsDown)
                {
                    var magic = Allocators.Magic[Allocators.Feature.TM_Magic];
                    magic[0] = !magic[0];
                }

                return false;
            }

            if (Plugin.ControlIsDown)
            {
                Instance[0] = false;
                OverCap();

                if (Plugin.AltIsDown)
                {
                    Allocators.Magic[Allocators.Feature.TM_Magic][0] = false;
                    TimeMachineMagicAllocator.OverCap();
                }

                return false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TimeMachineController), "removeEnergy")]
        private static bool TimeMachineController_removeEnergy_prefix(TimeMachineController __instance)
        {
            if (Plugin.ControlIsDown)
            {
                setTimeTarget();

                if (Plugin.AltIsDown)
                    TimeMachineMagicAllocator.setTimeTarget();

                return false;
            }

            Instance[0] = false;

            if (Plugin.ShiftIsDown)
                __instance.removeAllEnergy();

            return true;
        }

        private static long CalcCapForLevel(long level)
        {
            var cap = 50000.0 * level * (double)_controller.baseSpeedDivider() /
                (
                      (double)_character.totalEnergyPower()
                    * (double)_character.allChallenges.timeMachineChallenge.TMSpeedBonus()
                    * (double)_character.hacksController.totalTMSpeedBonus()
                    * (double)_character.cardsController.getBonus(cardBonus.TMSpeed)
                )
                * 1.000002;

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return ((long)cap) + 1;
        }

        internal static void OverCap()
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

        internal static void setTimeTarget()
        {
            var calc = Calculators.TM_EnergyCalculator;
            var resource = _character.machine.speedEnergy;
            if (resource == 0)
                resource = _character.totalCapEnergy();

            var runTimeSeconds = _character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = _character.machine.levelSpeed;

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            _controller.speedTarget.text = targetLevel.ToString();
            _controller.checkSpeedTargetInput();
        }
    }
}
