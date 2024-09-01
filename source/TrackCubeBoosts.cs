using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackCubeBoosts
    {
        internal static float PowerGainedLastRebirth
        {
            get => ModSave.Data.CubePowerBoostLastRB;
            set => ModSave.Data.CubePowerBoostLastRB = value;
        }

        internal static float PowerGainedThisRebirth
        {
            get => ModSave.Data.CubePowerBoostThisRB;
            set => ModSave.Data.CubePowerBoostThisRB = value;
        }

        internal static float ToughnessGainedLastRebirth
        {
            get => ModSave.Data.CubeToughnessBoostLastRB;
            set => ModSave.Data.CubeToughnessBoostLastRB = value;
        }

        internal static float ToughnessGainedThisRebirth
        {
            get => ModSave.Data.CubeToughnessBoostThisRB;
            set => ModSave.Data.CubeToughnessBoostThisRB = value;
        }


        private static float _curP => Plugin.Character.inventory.cubePower;
        private static float _curT => Plugin.Character.inventory.cubeToughness;

        private static float _lastP;
        private static float _lastT;


        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastP = _curP;
                _lastT = _curT;
            };

            Plugin.OnLateUpdate += (o, e) =>
            {
                var p = _curP;
                if (p > _lastP)
                    PowerGainedThisRebirth += (p - _lastP);

                var t = _curT;
                if (t > _lastT)
                    ToughnessGainedThisRebirth += (t - _lastT);

                _lastP = p;
                _lastT = t;
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            PowerGainedLastRebirth = PowerGainedThisRebirth;
            PowerGainedThisRebirth = 0L;

            ToughnessGainedLastRebirth = ToughnessGainedThisRebirth;
            ToughnessGainedThisRebirth = 0L;
        }
    }
}
