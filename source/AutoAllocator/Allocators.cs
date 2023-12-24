using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods.AutoAllocator
{
    internal class Allocators
    {
        internal enum Feature
        {
            AdvancedTraining,
            Augment,
            AugmentUpgrade,
            BloodRitual,
            BT_Attack,
            BT_Defense,
            Hacks,
            NGU_Energy,
            NGU_Magic,
            TM_Energy,
            TM_Magic,
            Wandoos_Energy,
            Wandoos_Magic
        }

        static Allocators()
        {
            Plugin.OnPreSave += (o, e) =>
            {
                ModSave.Data.EnabledEnergyIDs = Energy.Where(kv => kv.Value.EnabledIDs.Any()).ToDictionary(kv => (int)kv.Key, kv => kv.Value.EnabledIDs.ToArray());
                ModSave.Data.EnabledMagicIDs = Magic.Where(kv => kv.Value.EnabledIDs.Any()).ToDictionary(kv => (int)kv.Key, kv => kv.Value.EnabledIDs.ToArray());
                ModSave.Data.EnabledRes3IDs = Res3.Where(kv => kv.Value.EnabledIDs.Any()).ToDictionary(kv => (int)kv.Key, kv => kv.Value.EnabledIDs.ToArray());
            };

            Plugin.OnSaveLoaded += (o, e) =>
            {
                ModSave.Data.EnabledEnergyIDs.Do(kv => Energy[(Feature)kv.Key].SetEnabled(kv.Value));
                ModSave.Data.EnabledMagicIDs.Do(kv => Magic[(Feature)kv.Key].SetEnabled(kv.Value));
                ModSave.Data.EnabledRes3IDs.Do(kv => Res3[(Feature)kv.Key].SetEnabled(kv.Value));
            };
        }

        public static Dictionary<Feature, BaseAllocator> Energy = new();
        public static Dictionary<Feature, BaseAllocator> Magic = new();
        public static Dictionary<Feature, BaseAllocator> Res3 = new();
    }
}
