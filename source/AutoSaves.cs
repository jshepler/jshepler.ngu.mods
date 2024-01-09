using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoSaves
    {
        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if (Input.GetKeyDown(KeyCode.F5))
                    DoSave($"QuickSave");
                else if (Input.GetKeyDown(KeyCode.F6))
                    LoadLastQuicksave();
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_bool_prefix(bool hardReset, Rebirth __instance)
        {
            DoSave(hardReset ? "Challenge" : "Rebirth");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PitController), "engage")]
        private static void PitController_engage_prefix(PitController __instance)
        {
            DoSave("PitThrow");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Wandoos98Controller), "setOSType")]
        private static void Wandoos98_changeOS_prefix(Wandoos98Controller __instance, int ___nextOS)
        {
            var newOS = ___nextOS switch
            {
                0 => OSType.wandoos98,
                1 => OSType.wandoosMEH,
                _ => OSType.wandoosXL
            };

            if (newOS != __instance.character.wandoos98.os)
            {
                DoSave("Change_Wandoos_OS");
            }
        }

        private static void DoSave(string saveName)
        {
            var character = Plugin.Character;
            character.lastTime = Epoch.Current();
            var data = character.importExport.getBase64Data();

            try
            {
                File.WriteAllText($"{Application.persistentDataPath}/{saveName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt", data);
                character.tooltip.showOverrideTooltip($"game saved: {saveName}", 1);
            }
            catch (Exception ex)
            {
                Plugin.LogInfo($"Failed to write {saveName}.txt: " + ex.Message);
            }

            var daysToKeep = Options.PruneSaves.DaysToKeep.Value;
            if (daysToKeep <= 0)
                return;

            var folder = new DirectoryInfo(Application.persistentDataPath);
            var files = folder.GetFiles().Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-daysToKeep));
            foreach (var f in files)
                f.Delete();
        }

        private static void LoadLastQuicksave()
        {
            var folder = new DirectoryInfo(Application.persistentDataPath);
            if (!folder.Exists)
                return;

            var quickSaves = folder.GetFiles("QuickSave*").OrderByDescending(f => f.LastWriteTimeUtc).ToArray();
            if (quickSaves.Length == 0 || quickSaves[0] == null)
                return;

            // modelled on OpenFileDialog.setLocalSaveSteam()
            var character = Plugin.Character;
            var importExport = character.importExport;

            var text = File.ReadAllText(quickSaves[0].FullName);
            var saveDataFromString = importExport.getSaveDataFromString(text);
            var dataFromString = importExport.getDataFromString(text);

            character.mainMenu.setLocalSave(saveDataFromString);
            character.mainMenu.setLocalPlayerData(dataFromString);
            character.mainMenu.setLocalSaveValidity(validity: true);

            character.mainMenu.loadAutosaveSteam();
        }
    }
}
