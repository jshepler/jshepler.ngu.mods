using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class AugmentAllocator : BaseAllocator
    {
        private static AllAugsController _allAugsController;
        private static Character _character;
        private static AugmentAllocator Instance = new();

        private static float _augDivider(int id) => _character.settings.rebirthDifficulty switch
        {
            difficulty.normal => _allAugsController.normalAugSpeedDividers[id],
            difficulty.evil => _allAugsController.evilAugSpeedDividers[id],
            difficulty.sadistic => _allAugsController.sadisticAugSpeedDividers[id],
            _ => 0
        };

        private static float _upgDivider(int id) => _character.settings.rebirthDifficulty switch
        {
            difficulty.normal => _allAugsController.normalUpgradeSpeedDividers[id],
            difficulty.evil => _allAugsController.evilUpgradeSpeedDividers[id],
            difficulty.sadistic => _allAugsController.sadisticUpgradeSpeedDividers[id],
            _ => 0
        };

        internal AugmentAllocator() : base(7)
        {
            Allocators.Energy.Add(Allocators.Feature.Augment, this);
        }

        internal override void Allocate(int id, long amount)
        {
            _character.augments.augs[id].augEnergy += amount;
            _allAugsController.augments[id].updateAugTexts();
        }

        internal override long CalcCapDelta(int id)
        {
            var aug = _character.augments.augs[id];
            var cap = CalcCapForLevel(id, aug.augLevel + 1);

            var delta = cap - aug.augEnergy;
            if (aug.augEnergy + delta < 0)
                delta = -aug.augEnergy;

            return delta;
        }

        internal override bool IsTargetReached(int id)
        {
            return _allAugsController.augments[id].hitAugmentTarget();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AllAugsController), "Start")]
        private static void AllAugsController_Start_postfix(AllAugsController __instance)
        {
            _allAugsController = __instance;
            _character = __instance.character;

            __instance.augments.Do((c, i) =>
                Instance.TextComponents[i] = c.transform.Find("Aug +/Text").GetComponent<Text>());
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AugmentController), "addEnergyAug")]
        private static bool AugmentController_addEnergyAug_prefix(AugmentController __instance)
        {
            var id = __instance.id;

            if (Input.GetKey(KeyCode.LeftShift) && Options.Allocators.AutoAllocatorEnabled.Value == true)
            {
                Instance[id] = !Instance[id];

                if(Input.GetKey(KeyCode.LeftAlt))
                    Allocators.Energy[Allocators.Feature.AugmentUpgrade][id] = Instance[id];

                return false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Options.Allocators.OverCapAllocatorEnabled.Value == true)
            {
                Instance[id] = false;
                OverCap(id);

                return false;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Options.Allocators.RatioSplitAllocatorEnabled.Value == true)
            {
                Instance[id] = false;
                Allocators.Energy[Allocators.Feature.AugmentUpgrade][id] = false;
                SplitEnergy(id);

                return false;
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AugmentController), "removeEnergyAug")]
        private static void AugmentController_removeEnergyAug_postfix(AugmentController __instance)
        {
            Instance[__instance.id] = false;
        }

        private static long CalcCapForLevel(int id, long level)
        {
            var extraSadDivider = _character.settings.rebirthDifficulty >= difficulty.sadistic ? _allAugsController.augments[id].sadisticDivider() : 1.0;

            var allAugsController = _character.augmentsController;
            double speedDivider = _character.settings.rebirthDifficulty switch
            {
                difficulty.normal => allAugsController.normalAugSpeedDividers[id],
                difficulty.evil => allAugsController.evilAugSpeedDividers[id],
                difficulty.sadistic => allAugsController.sadisticAugSpeedDividers[id],
                _ => 1
            };

            var cap = 50000d * speedDivider * level * extraSadDivider /
                (
                    _character.totalEnergyPower()
                    * (double)(1f + _character.inventoryController.bonuses[specType.Augs])
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

        private static void OverCap(int id)
        {
            var aug = _character.augments.augs[id];
            if (aug.augmentTarget == -1)
                return;

            var currentLevel = (ulong)aug.augLevel;
            var targetLevel = (ulong)aug.augmentTarget;
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

            _character.idleEnergy += aug.augEnergy;
            aug.augEnergy = 0;

            var cap = CalcCapForLevel(id, (long)targetLevel);
            if (cap > _character.idleEnergy)
                cap = _character.idleEnergy;

            Instance.Allocate(id, cap);
            _character.idleEnergy -= cap;
        }

        private static void SplitEnergy(int id)
        {
            var aug = _character.augments.augs[id];

            _character.idleEnergy += aug.augEnergy;
            aug.augEnergy = 0;

            _character.idleEnergy += aug.upgradeEnergy;
            aug.upgradeEnergy = 0;

            var augDivider = _augDivider(id);
            var upgDivider = _upgDivider(id);
            var sum = augDivider + upgDivider;

            var amount = (long)(augDivider / sum * _character.idleEnergy);
            aug.augEnergy = amount;
            aug.upgradeEnergy = _character.idleEnergy - amount;

            _character.idleEnergy = 0;

            _allAugsController.augments[id].updateAugTexts();
            _allAugsController.augments[id].updateUpgradeTexts();
        }
    }
}
