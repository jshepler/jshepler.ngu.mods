using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace jshepler.ngu.mods.ModSave
{
    internal class PlayerDataBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "PlayerData")
                return typeof(ModPlayerData);

            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}
