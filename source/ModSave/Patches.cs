using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;
using SFB;
using UnityEngine;

namespace jshepler.ngu.mods.ModSave
{
    [HarmonyPatch]
    internal class Patches
    {
        // this adds a way to make a clean save (without the extra data)
        // useful for things like using the gear optimizer as it fails to load modded save
        private static bool _cleanSave = false;

        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "startSaveStandalone")]
        private static void OpenFileDialog_startSaveStandalone_prefix()
        {
            _cleanSave = Input.GetKey(KeyCode.LeftShift);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "SaveFilePanel", typeof(string), typeof(string), typeof(string), typeof(string))]
        private static void StandaloneFileBrowser_SaveFilePanel_prefix(ref string title)
        {
            if (_cleanSave)
                title += " (CLEAN)";
        }

        // serialization patches

        // instead of creating a new instance of PlayerData, create a new instance of ModPlayerData with the additional data set
        // this replaces the CIL to create new PlayerData and instead calls a function that creates ModPlayerData and sets the Data field
        // if doing a clean save, it creates a new instance of PlayerData
        //
        // note that ModPlayerData implements ISerializable and sets the type to be PlayerData - this is so a vanilla game could still load the save
        [HarmonyTranspiler, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static IEnumerable<CodeInstruction> ImportExport_gameStateToData_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .Advance(1)
                .SetInstruction(Transpilers.EmitDelegate(CreatePlayerData));

            return cm.InstructionEnumeration();
        }

        private static PlayerData CreatePlayerData()
        {
            if (_cleanSave)
                return new PlayerData();

            return new ModPlayerData() { Data = Data.Values };
        }

        // deserialization patches

        // this sets BinaryFormatter.Binder to PlayerDataBinder
        // PlayerDataBinder basically redirects deserliazation of PlayerData to ModPlayerData
        [HarmonyTranspiler, HarmonyPatch(typeof(BinaryFormatterExtensions), "DeserializePlayerDataFromString")]
        private static IEnumerable<CodeInstruction> BinaryFormatterExtensions_DeserializePlayerDataFromString_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Ldarg_0)
                    , Transpilers.EmitDelegate((BinaryFormatter bf) => bf.Binder = new PlayerDataBinder()));

            return cm.InstructionEnumeration();
        }

        // after deserialization, loadData is called to set everything - insert a call at the end to set Data.Values (the mod data that got saved)
        [HarmonyTranspiler, HarmonyPatch(typeof(ImportExport), "loadData")]
        private static IEnumerable<CodeInstruction> ImportExport_loadData_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            // instead of relying on the index of the PlayerData local, this is an experiment in getting a reference to it
            // based on Type - there's only 1 local variable of type PlayerData, so this should work (it does)
            var locals = method.GetMethodBody().LocalVariables;
            var playerDataVar = new LocalVar(locals.First(l => l.LocalType == typeof(PlayerData)));

            var cm = new CodeMatcher(instructions)
                .End()
                .Insert(new CodeInstruction(playerDataVar.ToLdloc())
                    , Transpilers.EmitDelegate(LoadModData));

            return cm.InstructionEnumeration();
        }

        private static void LoadModData(PlayerData pd)
        {
            // check first in case loading a vanilla save
            if (pd is ModPlayerData mpd)
                Data.Values = mpd.Data ?? new();
        }
    }
}