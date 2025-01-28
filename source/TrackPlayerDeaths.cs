using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackPlayerDeaths
    {
        private static int _nDeaths
        {
            get => Data.Deaths_Normal;
            set => Data.Deaths_Normal = value;
        }

        private static int _eDeaths
        {
            get => Data.Deaths_Evil;
            set => Data.Deaths_Evil = value;
        }

        private static int _sDeaths
        {
            get => Data.Deaths_Sadistic;
            set => Data.Deaths_Sadistic = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "playerDeath")]
        private static void AdventureController_playerDeath_postfix()
        {
            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    _nDeaths++;
                    break;

                case difficulty.evil:
                    _eDeaths++;
                    break;

                case difficulty.sadistic:
                    _sDeaths++;
                    break;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "fight")]
        private static void BossController_fight_postfix()
        {
            if (Plugin.Character.curHP > 0)
                return;

            switch (Plugin.Character.settings.rebirthDifficulty)
            {
                case difficulty.normal:
                    _nDeaths++;
                    break;

                case difficulty.evil:
                    _eDeaths++;
                    break;

                case difficulty.sadistic:
                    _sDeaths++;
                    break;
            }
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(MiscStatsDisplay), "updateMiscStats")]
        private static IEnumerable<CodeInstruction> MiscStatsDisplay_updateMiscStats_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var statsName = typeof(MiscStatsDisplay).GetField("statsName", BindingFlags.Instance | BindingFlags.NonPublic);
            var statsValue = typeof(MiscStatsDisplay).GetField("statsValue", BindingFlags.Instance | BindingFlags.NonPublic);
            var titansDefeated = typeof(Stats).GetField("titansDefeated");
            var concat = typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, [typeof(string), typeof(string)], []);

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\nTotal Earned EXP:\n"))
                .Advance(-3)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), 
                    new CodeInstruction(OpCodes.Ldarg_0), 
                    new CodeInstruction(OpCodes.Ldfld, statsName), 
                    Transpilers.EmitDelegate(() => "\nPlayer Deaths (norm / evil / sad):\n"),
                    new CodeInstruction(OpCodes.Call, concat),
                    new CodeInstruction(OpCodes.Stfld, statsName))

                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, titansDefeated))
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, statsValue),
                    Transpilers.EmitDelegate(() => $"\n\n{_nDeaths} / {_eDeaths} / {_sDeaths}"),
                    new CodeInstruction(OpCodes.Call, concat),
                    new CodeInstruction(OpCodes.Stfld, statsValue));

            return cm.InstructionEnumeration();//.DumpToLog();
        }
    }
}
