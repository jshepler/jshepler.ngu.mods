using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class AdvancedTraining
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AdvancedTrainingController), "showTooltip")]
        private static void AdvancedTrainingController_showTooltip_postfix(AdvancedTrainingController __instance)
        {
            var id = __instance.id;
            var character = __instance.character;

            var currentLevel = character.advancedTraining.level[id];
            var energy = character.advancedTraining.energy[id];

            var message = string.Empty;
            var targetLevel = character.advancedTraining.levelTarget[id];
            if (targetLevel > currentLevel)
            {
                if (energy == 0)
                    energy = character.totalCapEnergy();

                var currentProgress = character.advancedTraining.barProgress[id];
                var secondsToTarget = Calculators.AdvancedTrainingCalculators[id].TimeToTarget(currentLevel, targetLevel, energy, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.progressPerTick();

            if (ppt > 1)
            {
                var capLevel = Calculators.AdvancedTrainingCalculators[id].LevelFromResource(energy);

                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.AdvancedTrainingCalculators[id].ResourceFromLevel(currentLevel + 1);
            var capMessage = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            message += $"\n\n{capMessage}";

            var bank = character.adventureController.itopod.totalBankedAdvTraining();
            if (bank > 0f)
                message += $"\n\n<b>Banked ({bank * 100f:0}%):</b> {character.display(currentLevel * bank)}";

            __instance.tooltip.showTooltip($"{LastTooltip.Message}{message}");
        }
    }
}
