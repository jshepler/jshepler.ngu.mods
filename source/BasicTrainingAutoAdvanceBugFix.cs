using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class BasicTrainingAutoAdvanceBugFix
    {
        [HarmonyPrepare]
        internal static void prep(MethodBase original)
        {
            if (original != null) return;
            
            Plugin.OnSaveLoaded += (o, e) =>
            {
                e.Character.allOffenseController.autoAdvanceController.autoAdvanceToggle.isOn = e.Character.training.autoAdvanceToggle;
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix(ImportExport __instance)
        {
            __instance.character.training.autoAdvanceToggle = __instance.character.allOffenseController.autoAdvanceController.autoAdvanceToggle.isOn;
        }
    }
}
