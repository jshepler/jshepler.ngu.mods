using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoSaves
    {
        internal static string GameName = null;

        private static string ModifiedPersistentDataPath()
        {
            var path = Application.persistentDataPath;

            if (GameName != null)
            {
                path += $"/{GameName}";
                Directory.CreateDirectory(path);
            }

            return path;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    if (Plugin.Character.settings.dailySaveRewardTime.totalseconds >= 82800.0)
                    {
                        Plugin.Character.settings.dailySaveRewardTime.reset();
                        Plugin.ShowOverrideNotification($"You (tried) to manually save your file today! Here's {Plugin.Character.addAP(200)} AP as a bribe!");
                    }

                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        DoSave("QuickSave (CLEAN)", true);
                    else
                        DoSave("QuickSave");
                }

                else if (Input.GetKeyDown(KeyCode.F6))
                    LoadLastQuicksave();

                else if (Input.GetKeyDown(KeyCode.F7))
                    System.Diagnostics.Process.Start(ModifiedPersistentDataPath());
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

        private static void DoSave(string saveName, bool doCleanSave = false)
        {
            ModSave.Patches.DoCleanSave = doCleanSave;

            var character = Plugin.Character;
            character.lastTime = Epoch.Current();
            var data = character.importExport.getBase64Data();
            var saveFolder = ModifiedPersistentDataPath();

            try
            {
                File.WriteAllText($"{saveFolder}/{saveName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt", data);
                Plugin.ShowOverrideNotification($"game saved: {saveName}", 1);
            }
            catch (Exception ex)
            {
                Plugin.LogInfo($"Failed to write {saveName}.txt: " + ex.Message);
            }

            var daysToKeep = Options.PruneSaves.DaysToKeep.Value;
            if (daysToKeep <= 0)
                return;

            var folder = new DirectoryInfo(saveFolder);
            var files = folder.GetFiles().Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-daysToKeep));
            foreach (var f in files)
                f.Delete();
        }

        private static void LoadLastQuicksave()
        {
            var folder = new DirectoryInfo(ModifiedPersistentDataPath());
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

        private static MethodInfo persistentDataPath = typeof(Application).GetProperty("persistentDataPath", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
        [HarmonyTranspiler,
            HarmonyPatch(typeof(OpenFileDialog), "quickSave", typeof(string)),
            HarmonyPatch(typeof(OpenFileDialog), "quickSaveStandalone"),
            HarmonyPatch(typeof(OpenFileDialog), "quickSaveSteam"),
            HarmonyPatch(typeof(OpenFileDialog), "backupSave"),
            HarmonyPatch(typeof(OpenFileDialog), "backupSaveSteam"),
            HarmonyPatch(typeof(OpenFileDialog), "deleteLocalSave"),
            HarmonyPatch(typeof(OpenFileDialog), "quickLoad", []),
            HarmonyPatch(typeof(OpenFileDialog), "setLocalSave"),
            HarmonyPatch(typeof(OpenFileDialog), "setLocalSaveSteam"),
            HarmonyPatch(typeof(OpenFileDialog), "setKartBackupSave"),
            HarmonyPatch(typeof(OpenFileDialog), "initialLoad"),
            HarmonyPatch(typeof(OpenFileDialog), "quicklyLoad"),
            HarmonyPatch(typeof(OpenFileDialog), "startSaveStandalone"),
            HarmonyPatch(typeof(OpenFileDialog), "startLoadStandalone"),
            HarmonyPatch(typeof(OpenFileDialog), "loadFileMainMenuStandalone")]
        private static IEnumerable<CodeInstruction> persistentDataPath_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, persistentDataPath))
                .Repeat(m => m.SetInstruction(Transpilers.EmitDelegate(ModifiedPersistentDataPath)));

            return cm.InstructionEnumeration();//.DumpToLog() ;
        }
    }
}
