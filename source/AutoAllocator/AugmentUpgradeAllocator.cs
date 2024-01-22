using HarmonyLib;
using UnityEngine;
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

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[id] = !Instance[id];

                if (Input.GetKey(KeyCode.LeftAlt))
                    Allocators.Energy[Allocators.Feature.Augment][id] = Instance[id];

                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Options.Allocators.OverCapAllocatorEnabled.Value == true)
            {
                Instance[id] = false;
                OverCap(id);

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    Allocators.Energy[Allocators.Feature.Augment][id] = false;
                    AugmentAllocator.OverCap(id);
                }

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AugmentController), "removeEnergyUpgrade")]
        private static void AugmentController_removeEnergyUpgrade_postfix(AugmentController __instance)
        {
            Instance[__instance.id] = false;

            if(Input.GetKey(KeyCode.LeftShift))
                __instance.character.idleEnergy += __instance.character.augments.augs[__instance.id].removeEnergyUpgrade(long.MaxValue);
        }

        private static long CalcCapForLevel(int id, long level)
        {
            var extraSadDivider = _character.settings.rebirthDifficulty >= difficulty.sadistic ? _allAugsController.augments[id].sadisticDivider() : 1.0;

            var allAugsController = _character.augmentsController;
            double speedDivider = _character.settings.rebirthDifficulty switch
            {
                difficulty.normal => allAugsController.normalUpgradeSpeedDividers[id],
                difficulty.evil => allAugsController.evilUpgradeSpeedDividers[id],
                difficulty.sadistic => allAugsController.sadisticUpgradeSpeedDividers[id],
                _ => 1
            };

            var cap = 50000f * speedDivider * level * extraSadDivider /
                (
                    _character.totalEnergyPower()
                    * (double)(1.0 + _character.inventoryController.bonuses[specType.Augs])
                    * (double)_character.inventory.macguffinBonuses[12]
                    * (double)_character.hacksController.totalAugSpeedBonus()
                    * (double)_character.adventureController.itopod.totalAugSpeedBonus()
                    * (double)_character.cardsController.getBonus(cardBonus.augSpeed)
                    * (double)(1.0 + _character.allChallenges.noAugsChallenge.evilCompletions() * 0.05)
                    * (_character.allChallenges.noAugsChallenge.completions() >= 1 ? 1.1000000238418579 : 1.0)
                    * (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions ? 1.25 : 1.0)
                );

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
    }
}
