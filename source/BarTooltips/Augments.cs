using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class Augments
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AugmentController), "showAugTooltip")]
        private static void AugmentController_showAugTooltip_postfix(AugmentController __instance)
        {
            var character = __instance.character;

            if (character.bossID <= __instance.augBossRequired)
                return;

            var id = __instance.id;
            var aug = character.augments.augs[id];

            var currentLevel = aug.augLevel;
            var energy = aug.augEnergy;

            var message = string.Empty;
            var targetLevel = aug.augmentTarget;
            if (targetLevel > currentLevel)
            {
                if (energy == 0)
                    energy = character.totalCapEnergy();

                var currentProgress = aug.augProgress;
                var secondsToTarget = Calculators.AugCalculators[id].TimeToTarget(currentLevel, targetLevel, energy, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.getAugProgressPerTick();
            if (ppt > 1)
            {
                var capLevel = Calculators.AugCalculators[id].LevelFromResource(energy);

                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.AugCalculators[id].ResourceFromLevel(currentLevel + 1);
            var capMessage = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            message += $"\n\n{capMessage}";

            __instance.tooltip.showTooltip($"{LastTooltip.Message}{message}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AugmentController), "showUpgradeTooltip")]
        private static void AugmentController_showUpgradeTooltip_postfix(AugmentController __instance)
        {
            var character = __instance.character;

            if (character.bossID <= __instance.upgradeBossRequired)
                return;

            var id = __instance.id;
            var aug = character.augments.augs[id];

            var currentLevel = aug.upgradeLevel;
            var energy = aug.upgradeEnergy;

            var message = string.Empty;
            var targetLevel = aug.upgradeTarget;
            if (targetLevel > currentLevel)
            {
                if (energy == 0)
                    energy = character.totalCapEnergy();

                var currentProgress = aug.upgradeProgress;
                var secondsToTarget = Calculators.AugUpgradeCalculators[id].TimeToTarget(currentLevel, targetLevel, energy, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.getUpgradeProgressPerTick();
            if (ppt > 1)
            {
                var capLevel = Calculators.AugUpgradeCalculators[id].LevelFromResource(energy);

                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.AugUpgradeCalculators[id].ResourceFromLevel(currentLevel + 1);
            var capMessage = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            message += $"\n\n{capMessage}";

            __instance.tooltip.showTooltip($"{LastTooltip.Message}{message}");
        }
    }
}
