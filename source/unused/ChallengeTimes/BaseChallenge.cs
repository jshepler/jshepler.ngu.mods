using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.ChallengeTimes
{
    abstract class BaseChallenge
    {
        protected enum ChallengeType
        {
            Basic,
            NoAugs,
            Hour24,
            Level100,
            NoEquip,
            Troll,
            NoRebirth,
            LaserSword,
            Blind,
            NoNGU,
            NoTM
        }

        protected static Dictionary<ChallengeType, int> MaxCompletions = new()
        {
            { ChallengeType.Basic, Plugin.Character.allChallenges.basicChallenge.maxCompletions },
            { ChallengeType.NoAugs, Plugin.Character.allChallenges.noAugsChallenge.maxCompletions },
            { ChallengeType.Hour24, Plugin.Character.allChallenges.hour24Challenge.maxCompletions },
            { ChallengeType.Level100, Plugin.Character.allChallenges.level100Challenge.maxCompletions },
            { ChallengeType.NoEquip, Plugin.Character.allChallenges.noEquipmentChallenge.maxCompletions },
            { ChallengeType.Troll, Plugin.Character.allChallenges.trollChallenge.maxCompletions },
            { ChallengeType.NoRebirth, Plugin.Character.allChallenges.noRebirthChallenge.maxCompletions },
            { ChallengeType.LaserSword, Plugin.Character.allChallenges.laserSwordChallenge.maxCompletions },
            { ChallengeType.Blind, Plugin.Character.allChallenges.blindChallenge.maxCompletions },
            { ChallengeType.NoNGU, Plugin.Character.allChallenges.NGUChallenge.maxCompletions },
            { ChallengeType.NoTM, Plugin.Character.allChallenges.timeMachineChallenge.maxCompletions }
        };


        private static Coroutine _cor = null;

        protected static void StartRoutine(Action showChallengeInfo)
        {
            StopRoutine();
            _cor = Plugin.BeginCoroutine(showInfo(showChallengeInfo));
        }

        protected static void StopRoutine()
        {
            if (_cor != null)
            {
                Plugin.EndCoroutine(_cor);
                _cor = null;
            }
        }

        private static WaitForSeconds _delay = new WaitForSeconds(0.1f);
        private static IEnumerator showInfo(Action showChallengeInfo)
        {
            while (Plugin.Character.InMenu(Menu.Challenges))
            {
                showChallengeInfo();
                yield return _delay;
            }

            _cor = null;
        }

        private static Func<int, string> _time = intSeconds => NumberOutput.timeOutput(intSeconds);

        protected static string BuildCompletionTimes(int[][] times, int[] curCompletions, int maxCompletions)
        {
            var numberOfTimeRows = times[0].Length;

            var columns = new string[4][];
            for (var x = 0; x < 4; x++)
                columns[x] = new string[numberOfTimeRows + 2]; // +2: 1 for header, 1 for sub total

            columns[0][0] = "Comp";
            columns[1][0] = $"Norm ({curCompletions[0]}/{maxCompletions})";
            columns[2][0] = $"Evil ({curCompletions[1]}/{maxCompletions})";
            columns[3][0] = $"Sad ({curCompletions[2]}/{maxCompletions})";

            for (var x = 0; x < numberOfTimeRows; x++)
            {
                columns[0][1 + x] = $"{x + 1}:";
                columns[1][1 + x] = times[0][x] == 0 ? " -- " : _time(times[0][x]);
                columns[2][1 + x] = times[1][x] == 0 ? " -- " : _time(times[1][x]);
                columns[3][1 + x] = times[2][x] == 0 ? " -- " : _time(times[2][x]);
            }

            columns[0][1 + numberOfTimeRows] = "sub:";
            columns[1][1 + numberOfTimeRows] = _time(times[0].Sum());
            columns[2][1 + numberOfTimeRows] = _time(times[1].Sum());
            columns[3][1 + numberOfTimeRows] = _time(times[2].Sum());

            var maxWidths = columns.Select(c => c.Max(s => s.Length + 2)).ToArray();
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(string.Empty, columns.Zip(maxWidths, (c, w) => c[0].PadLeft(w))));
            sb.AppendLine(string.Join(string.Empty, maxWidths.Select(w => "  " + new string('-', w - 2))));

            for (var x = 0; x < numberOfTimeRows; x++)
                sb.AppendLine(string.Join(string.Empty, columns.Zip(maxWidths, (c, w) => c[1 + x].PadLeft(w))));

            sb.AppendLine(string.Join(string.Empty, maxWidths.Select(w => "  " + new string('-', w - 2))));
            sb.AppendLine(string.Join(string.Empty, columns.Zip(maxWidths, (c, w) => c[1 + numberOfTimeRows].PadLeft(w))));

            var totalTime = times[0].Sum() + times[1].Sum() + times[2].Sum();
            sb.Append($"\n\n<b>Total Time In Challenge:</b> {_time(totalTime)}");

            return sb.ToString();
        }

        internal static int[][][] InitCompletionTimes()
        {
            var times = new int[MaxCompletions.Count][][];

            foreach (var kvp in MaxCompletions)
            {
                var index = (int)kvp.Key;
                var max = kvp.Value;

                times[index] = new int[3][];
                for (var x = 0; x < 3; x++)
                    times[index][x] = new int[max];
            }

            return times;
        }
    }
}
