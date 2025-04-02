using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class AugmentUpgradeAllocator : BaseAllocator
    {
        private static AllAugsController _allAugsController;
        private static Character _character;
        private static AugmentUpgradeAllocator Instance = new();

        internal AugmentUpgradeAllocator() : base(7)
        {
            Allocators.Energy.Add(Allocators.Feature.AugmentUpgrade, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.augments.augs[id].upgradeEnergy += amount;
            _allAugsController.augments[id].updateUpgradeTexts();
        }

        internal override long CalcCapDelta(int id)
        {
            var aug = _character.augments.augs[id];
            var cap = CalcCapForLevel(id, aug.upgradeLevel + 1);

            var delta = cap - aug.upgradeEnergy;
            if (aug.upgradeEnergy + delta < 0)
                delta = -aug.upgradeEnergy;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _allAugsController.augments[id].hitUpgradeTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllAugsController), "Start")]
        private static void AllAugsController_Start_postfix(AllAugsController __instance)
        {
            _allAugsController = __instance;
            _character = __instance.character;

            __instance.augments.Do((c, i) =>
                Instance.TextComponents[i] = c.transform.Find("Upgrade +/Text").GetComponent<Text>());
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AugmentController), "addEnergyUpgrade")]
        private static bool AugmentController_addEnergyUpgrade_prefix(AugmentController __instance)
        {
            var id = __instance.id;

            if (Plugin.ShiftIsDown)
            {
                Instance[id] = !Instance[id];

                if (Plugin.AltIsDown)
                    Allocators.Energy[Allocators.Feature.Augment][id] = Instance[id];

                return false;
            }

            if (Plugin.ControlIsDown)
            {
                Instance[id] = false;
                OverCap(id);

                if (Plugin.AltIsDown)
                {
                    Allocators.Energy[Allocators.Feature.Augment][id] = false;
                    AugmentAllocator.OverCap(id);
                }

                return false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AugmentController), "removeEnergyUpgrade")]
        private static bool AugmentController_removeEnergyUpgrade_prefix(AugmentController __instance)
        {
            if (Plugin.ControlIsDown)
            {
                setTimeTarget(__instance.id);

                if (Plugin.AltIsDown)
                    AugmentAllocator.setTimeTarget(__instance.id);

                return false;
            }

            Instance[__instance.id] = false;

            if(Plugin.ShiftIsDown)
                __instance.character.idleEnergy += __instance.character.augments.augs[__instance.id].removeEnergyUpgrade(long.MaxValue);

            return true;
        }

        private static long CalcCapForLevel(int id, long level)
        {
            var cap = Calculators.AugUpgradeCalculators[id].ResourceFromLevel(level) * 1.000002;

            //var extraSadDivider = _character.settings.rebirthDifficulty >= difficulty.sadistic ? _allAugsController.augments[id].sadisticDivider() : 1.0;

            //var allAugsController = _character.augmentsController;
            //double speedDivider = _character.settings.rebirthDifficulty switch
            //{
            //    difficulty.normal => allAugsController.normalUpgradeSpeedDividers[id],
            //    difficulty.evil => allAugsController.evilUpgradeSpeedDividers[id],
            //    difficulty.sadistic => allAugsController.sadisticUpgradeSpeedDividers[id],
            //    _ => 1
            //};

            //var cap = 50000f * speedDivider * level * extraSadDivider /
            //    (
            //        _character.totalEnergyPower()
            //        * (double)(1.0 + _character.inventoryController.bonuses[specType.Augs])
            //        * (double)_character.inventory.macguffinBonuses[12]
            //        * (double)_character.hacksController.totalAugSpeedBonus()
            //        * (double)_character.adventureController.itopod.totalAugSpeedBonus()
            //        * (double)_character.cardsController.getBonus(cardBonus.augSpeed)
            //        * (double)(1.0 + _character.allChallenges.noAugsChallenge.evilCompletions() * 0.05)
            //        * (_character.allChallenges.noAugsChallenge.completions() >= 1 ? 1.1000000238418579 : 1.0)
            //        * (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions ? 1.25 : 1.0)
            //    );

            if (cap >= long.MaxValue)
                return long.MaxValue;

            return ((long)cap) + 1;
        }

        internal static void OverCap(int id)
        {
            var aug = _character.augments.augs[id];
            if (aug.upgradeTarget == -1)
                return;

            var currentLevel = (ulong)aug.upgradeLevel;
            var targetLevel = (ulong)aug.upgradeTarget;
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

            _character.idleEnergy += aug.upgradeEnergy;
            aug.upgradeEnergy = 0;

            var cap = CalcCapForLevel(id, (long)targetLevel);
            if (cap > _character.idleEnergy)
                cap = _character.idleEnergy;

            Instance.Allocate(id, cap);
            _character.idleEnergy -= cap;
        }

        internal static void setTimeTarget(int id)
        {
            var controller = _allAugsController.augments[id];
            var aug = _character.augments.augs[id];
            var calc = Calculators.AugUpgradeCalculators[id];

            var resource = aug.upgradeEnergy;
            if (resource == 0)
                resource = _character.totalCapEnergy();

            var runTimeSeconds = _character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = aug.upgradeLevel;

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            controller.upgradeTarget.text = targetLevel.ToString();
            controller.checkUpgradeTargetInput();
        }
    }
}
