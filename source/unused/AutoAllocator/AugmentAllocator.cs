using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
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

        private static bool _altIsDown = false;

        [HarmonyPostfix, HarmonyPatch(typeof(AllAugsController), "Start")]
        private static void AllAugsController_Start_postfix(AllAugsController __instance)
        {
            _allAugsController = __instance;
            _character = __instance.character;

            __instance.augments.Do((c, i) =>
                Instance.TextComponents[i] = c.transform.Find("Aug +/Text").GetComponent<Text>());

            var wasAltDown = false;
            Plugin.OnUpdate += (o, e) =>
            {
                _altIsDown = Plugin.AltIsDown;
                if (_altIsDown != wasAltDown)
                {
                    wasAltDown = _altIsDown;
                    __instance.updateMenu();
                }
            };
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(AugmentController), "updateAugTexts"),
            HarmonyPatch(typeof(AugmentController), "updateUpgradeTexts")]
        private static void AugmentController_updateAugTexts_postfix(AugmentController __instance)
        {
            if (!_altIsDown || !Plugin.Character.InMenu(Menu.Augments))
                return;

            var id = __instance.id;
            var augD = _augDivider(id);
            var upgD = _upgDivider(id);

            __instance.augLevelText.text = augD < upgD ? "1" : (augD / upgD).ToString("#.#####");
            __instance.upgradeLevelText.text = upgD < augD ? "1" : (upgD / augD).ToString("#.#####");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AugmentController), "addEnergyAug")]
        private static bool AugmentController_addEnergyAug_prefix(AugmentController __instance)
        {
            var id = __instance.id;

            if (Plugin.ShiftIsDown)
            {
                Instance[id] = !Instance[id];

                if(Plugin.AltIsDown)
                    Allocators.Energy[Allocators.Feature.AugmentUpgrade][id] = Instance[id];

                return false;
            }

            if (Plugin.ControlIsDown)
            {
                Instance[id] = false;
                OverCap(id);

                if (Plugin.AltIsDown)
                {
                    Allocators.Energy[Allocators.Feature.AugmentUpgrade][id] = false;
                    AugmentUpgradeAllocator.OverCap(id);
                }

                return false;
            }

            if (Plugin.AltIsDown)
            {
                Instance[id] = false;
                Allocators.Energy[Allocators.Feature.AugmentUpgrade][id] = false;
                SplitEnergy(id);

                return false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AugmentController), "removeEnergyAug")]
        private static bool AugmentController_removeEnergyAug_prefix(AugmentController __instance)
        {
            if (Plugin.ControlIsDown)
            {
                setTimeTarget(__instance.id);

                if (Plugin.AltIsDown)
                    AugmentUpgradeAllocator.setTimeTarget(__instance.id);

                return false;
            }

            Instance[__instance.id] = false;

            if (Plugin.ShiftIsDown)
                __instance.character.idleEnergy += __instance.character.augments.augs[__instance.id].removeEnergyAug(long.MaxValue);

            return true;
        }

        private static long CalcCapForLevel(int id, long level)
        {
            var cap = Calculators.AugCalculators[id].ResourceFromLevel(level) * 1.000002;

            //var extraSadDivider = _character.settings.rebirthDifficulty >= difficulty.sadistic ? _allAugsController.augments[id].sadisticDivider() : 1.0;

            //var allAugsController = _character.augmentsController;
            //double speedDivider = _character.settings.rebirthDifficulty switch
            //{
            //    difficulty.normal => allAugsController.normalAugSpeedDividers[id],
            //    difficulty.evil => allAugsController.evilAugSpeedDividers[id],
            //    difficulty.sadistic => allAugsController.sadisticAugSpeedDividers[id],
            //    _ => 1
            //};

            //var cap = 50000d * speedDivider * level * extraSadDivider /
            //    (
            //        _character.totalEnergyPower()
            //        * (double)(1f + _character.inventoryController.bonuses[specType.Augs])
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

        internal static void setTimeTarget(int id)
        {
            var controller = _allAugsController.augments[id];
            var aug = _character.augments.augs[id];
            var calc = Calculators.AugCalculators[id];

            var resource = aug.augEnergy;
            if (resource == 0)
                resource = _character.totalCapEnergy();

            var runTimeSeconds = _character.input.energyMagicInput * 60;
            if (runTimeSeconds > 172800)
            {
                Plugin.ShowNotification("Max time allowed is 2 days");
                return;
            }

            var ticksRemaining = runTimeSeconds * 50;
            var targetLevel = aug.augLevel;

            while (ticksRemaining > 0)
            {
                var ttl = calc.TicksToLevel(resource, targetLevel + 1);
                if (ttl > ticksRemaining)
                    break;

                ticksRemaining -= ttl;
                targetLevel++;
            }

            controller.augmentTarget.text = targetLevel.ToString();
            controller.checkAugTargetInput();
        }
    }
}
