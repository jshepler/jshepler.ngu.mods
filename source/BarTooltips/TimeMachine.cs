using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class TimeMachine
    {
        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "displaySpeedTooltip")]
        private static void TimeMachineController_displaySpeedTooltip_postfix(TimeMachineController __instance, ref string ___message)
        {
            var character = __instance.character;
            if (character.challenges.blindChallenge.inChallenge)
                return;

            var currentLevel = character.machine.levelSpeed;
            var energy = character.machine.speedEnergy;

            var targetLevel = character.machine.speedTarget;
            if (targetLevel > currentLevel)
            {
                if (energy == 0)
                    energy = character.totalCapEnergy();

                var currentProgress = character.machine.speedProgress;
                var secondsToTarget = Calculators.TM_EnergyCalculator.TimeToTarget(currentLevel, targetLevel, energy, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                ___message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.speedProgressPerTick();
            if (ppt > 1)
            {
                var capLevel = Calculators.TM_EnergyCalculator.LevelFromResource(energy);
                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.TM_EnergyCalculator.ResourceFromLevel(currentLevel + 1);
            var capText = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            ___message += $"\n\n{capText}";

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeMachineController), "displayGoldMultiTooltip")]
        private static void TimeMachineController_displayGoldMultiTooltip_postfix(TimeMachineController __instance, ref string ___message)
        {
            var character = __instance.character;
            if (character.challenges.blindChallenge.inChallenge)
                return;

            var currentLevel = character.machine.levelGoldMulti;
            var magic = character.machine.goldMultiMagic;

            var targetLevel = character.machine.multiTarget;
            if (targetLevel > currentLevel)
            {
                if (magic == 0)
                    magic = character.totalCapMagic();

                var currentProgress = character.machine.goldMultiProgress;
                var secondsToTarget = Calculators.TM_MagicCalculator.TimeToTarget(currentLevel, targetLevel, magic, currentProgress);
                var ttt = Tooltips.BuildTimeToTargetText(secondsToTarget);
                ___message += $"\n{ttt}";
            }

            var overCappedDuration = 0f;
            var ppt = __instance.goldMultiProgressPerTick();

            if (ppt > 1)
            {
                var capLevel = Calculators.TM_MagicCalculator.LevelFromResource(magic);
                overCappedDuration = (capLevel - currentLevel) / 50f;
            }

            var currentCap = Calculators.TM_MagicCalculator.ResourceFromLevel(currentLevel + 1);
            var capText = Tooltips.BuildCurrentCapText(currentCap, overCappedDuration, ppt);
            ___message += $"\n\n{capText}";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
