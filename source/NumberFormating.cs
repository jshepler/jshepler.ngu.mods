using System;
using System.Collections.Generic;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class NumberFormating
    {
        private static double _threshold = Math.Max(1e+6, Options.MixedNumberFormat.Threshold.Value);

        // these allow the number formatting to do appropriate formatting with negative numbers
        // as well as implementing mixed number format (sci/eng uses suffix if number < threshold)

        [HarmonyPrefix,
            HarmonyPatch(typeof(NumberFormat), "realSuffixFormat"),
            HarmonyPatch(typeof(NumberOutput), "realSuffixFormat")]
        private static bool realSuffixFormat_prefix(double number, ref string __result, Dictionary<int, string> ___suffixString)
        {
            var abs = Math.Abs(number);

            if (double.IsPositiveInfinity(number))
                __result = "Infinity";

            else if (double.IsNegativeInfinity(number))
                __result = "-Infinity";

            else if (double.IsNaN(number))
                __result = "NaN";

            else if (abs < 1.0)
                __result = number.ToString();

            else if (abs < 1000000.0)
                __result = number.ToString("###,##0");

            else
            {
                var log = (int)Math.Floor(Math.Log(abs, 1000.0));
                number /= Math.Pow(1000.0, log);

                __result = $"{number:###.000}{___suffixString[log]}";
            }

            return false;
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(NumberFormat), "engineerFormat"),
            HarmonyPatch(typeof(NumberOutput), "engineerFormat")]
        private static bool engineerFormat_prefix(double number, ref string __result, Dictionary<int, string> ___suffixString)
        {
            var abs = Math.Abs(number);

            if (double.IsPositiveInfinity(number))
                __result = "Infinity";

            else if (double.IsNegativeInfinity(number))
                __result = "-Infinity";

            else if (double.IsNaN(number))
                __result = "NaN";

            else if (abs < _threshold)
                realSuffixFormat_prefix(number, ref __result, ___suffixString);

            else
            {
                var num = Math.Floor(Math.Log10(abs) / 3.0) * 3.0;
                number /= Math.Pow(10.0, num);

                __result = number.ToString("###.000") + "E+" + num;
            }

            return false;
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(NumberFormat), "sciFormat"),
            HarmonyPatch(typeof(NumberOutput), "sciFormat")]
        private static bool sciFormat_prefix(double number, ref string __result, Dictionary<int, string> ___suffixString)
        {
            var abs = Math.Abs(number);

            if (double.IsPositiveInfinity(number))
                __result = "Infinity";

            else if (double.IsNegativeInfinity(number))
                __result = "-Infinity";

            else if (double.IsNaN(number))
                __result = "NaN";

            else if (abs < _threshold)
                realSuffixFormat_prefix(number, ref __result, ___suffixString);

            else
                __result = number.ToString("e3");

            return false;
        }
    }
}
