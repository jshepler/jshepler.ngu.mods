using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class NGU
    {
        [HarmonyPostfix, HarmonyPatch(typeof(NGUController), "displayTooltip")]
        private static void NGUController_displayTooltip_postfix(NGUController __instance, ref string ___message)
        {
            var character = __instance.character;
            var id = __instance.id;

            var currentLevel = __instance.CurrentLevel();
            var energy = character.NGU.skills[id].energy;

            var targetLevel = __instance.GetTarget();
            if (targetLevel > currentLevel)
            {
                if (energy == 0)
                    energy = character.totalCapEnergy();

                var currentProgress = __instance.GetProgress();
                var secondsToTarget = Calculators.NGU_EnergyCalculators[id].TimeToTarget(currentLevel, targetLevel, energy, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                ___message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.progressPerTick();
            if (ppt > 1)
            {
                var capLevel = Calculators.NGU_EnergyCalculators[id].LevelFromResource(energy);
                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.NGU_EnergyCalculators[id].ResourceFromLevel(currentLevel);
            var capMessage = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            ___message += $"\n\n{capMessage}";

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NGUMagicController), "displayTooltip")]
        private static void NGUMagicController_displayTooltip_postfix(NGUMagicController __instance, ref string ___message)
        {
            var character = __instance.character;
            var id = __instance.id;

            var currentLevel = __instance.CurrentLevel();
            var magic = character.NGU.magicSkills[id].magic;

            var targetLevel = __instance.GetTarget();
            if (targetLevel > currentLevel)
            {
                if (magic == 0)
                    magic = character.totalCapMagic();

                var currentProgress = __instance.GetProgress();
                var secondsToTarget = Calculators.NGU_MagicCalculators[id].TimeToTarget(currentLevel, targetLevel, magic, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                ___message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.progressPerTick();
            if (ppt > 1)
            {
                var capLevel = Calculators.NGU_MagicCalculators[id].LevelFromResource(magic);
                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.NGU_MagicCalculators[id].ResourceFromLevel(currentLevel);
            var capMessage = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            ___message += $"\n\n{capMessage}";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
