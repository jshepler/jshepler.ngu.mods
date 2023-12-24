using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace jshepler.ngu.mods.ModSave
{
    [Serializable]
    internal class ModPlayerData : PlayerData, ISerializable
    {
        public Dictionary<string, object> Data = new();

        public ModPlayerData() { }

        // ISerializable expects a specific ctor to populate fields
        public ModPlayerData(SerializationInfo info, StreamingContext context)
        {
            var fields = typeof(ModPlayerData).GetFields();

            foreach (var entry in info)
                fields.FirstOrDefault(f => f.Name == entry.Name)?.SetValue(this, entry.Value);
        }

        // we use this to change the serialization type from ModPlayerData to PlayerData,
        // and then manually add all the fields, including the mod data we want to include
        // this is so a vanilla game can deserialize the save without choking on the ModPlayerData type since it wouldn't exist
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(PlayerData));

            foreach (var f in typeof(ModPlayerData).GetFields())
                info.AddValue(f.Name, f.GetValue(this));
        }
    }
}