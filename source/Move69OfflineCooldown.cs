using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Move69OfflineCooldown
    {
        private static Move69 _move69;
        private static FieldInfo _move69TimerField = typeof(Move69).GetField("move69Timer", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float _move69Timer
        {
            get => (float)_move69TimerField.GetValue(_move69);
            set => _move69TimerField.SetValue(_move69, value);
        }

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnPreSave += (o, e) => Data.Move69Timer = _move69Timer;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Move69), "Update")]
        private static void Move69_Update_postfix(Move69 __instance)
        {
            _move69 ??= __instance;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_postfix(int timeElapsed)
        {
            _move69Timer = Data.Move69Timer;

            var challenges = Plugin.Character.challenges;
            if (!OfflineTime.SkipOfflineProgress && !challenges.levelChallenge10k.inChallenge && !challenges.trollChallenge.inChallenge && !challenges.hour24Challenge.inChallenge)
                _move69Timer += timeElapsed;
        }
    }
}
