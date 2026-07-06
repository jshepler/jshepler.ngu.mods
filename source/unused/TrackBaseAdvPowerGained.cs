using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.ModSave;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackBaseAdvPowerGained
    {
        private static float _totalGained
        {
            get => Data.BaseAdvPowerGained;
            set => Data.BaseAdvPowerGained = value;
        }

        private static float[] _sources
        {
            get => Data.BaseAdvPowerGainSources;
            set => Data.BaseAdvPowerGainSources = value;
        }

        private static float _before;
        private static int _nextSource;

        private static float _curAdvPower => Plugin.Character.adventure.attack;
        private static float _lastAdvPower;
        internal static Action Reset => () => _lastAdvPower = 0f;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventurePurchases), "Start")]
        private static void AdventurePurchases_Start_postfix(AdventurePurchases __instance)
        {
            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastAdvPower = _curAdvPower;

                if (_sources[Sources.SewerSet] == 0 && Plugin.Character.inventory.itemList.maxxedSewers())
                {
                    _sources[Sources.SewerSet] = 5f;
                    _totalGained += 5f;
                }

                if (_sources[Sources.Perks] == 0 && Plugin.Character.adventure.itopod.perkLevel[2] > 0)
                {
                    _sources[Sources.Perks] = 100f;
                    _totalGained += 100f;
                }
            };

            Plugin.OnLateUpdate += (o, e) =>
            {
                var power = _curAdvPower;
                if (power > _lastAdvPower)
                {
                    _totalGained += (power - _lastAdvPower);
                    _lastAdvPower = power;
                }
            };
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(AdventurePurchases), "buy1Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy10Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy100Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy1000Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy10KAttack"),
            HarmonyPatch(typeof(AdventurePurchases), "buyCustomPower")]
        private static void exp_before()
        {
            _before = _curAdvPower;
            _nextSource = Sources.Exp;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
        private static void sewerSet_before()
        {
            var il = Plugin.Character.inventory.itemList;
            if (!il.sewersComplete && il.maxxedSewers())
                _sources[Sources.SewerSet] += 5;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FruitController), "consumeAdventureFruit")]
        private static void fruit_before()
        {
            _before = _curAdvPower;
            _nextSource = Sources.Fruit;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItopodPerkController), "doEffect")]
        private static void perks_before()
        {
            _before = _curAdvPower;
            _nextSource = Sources.Perks;
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(PitController), "tier1Reward"),
            HarmonyPatch(typeof(PitController), "tier2Reward"),
            HarmonyPatch(typeof(PitController), "tier3Reward"),
            HarmonyPatch(typeof(PitController), "tier4Reward"),
            HarmonyPatch(typeof(PitController), "tier5Reward"),
            HarmonyPatch(typeof(PitController), "tier6Reward"),
            HarmonyPatch(typeof(PitController), "tier7Reward"),
            HarmonyPatch(typeof(PitController), "tier8Reward"),
            HarmonyPatch(typeof(PitController), "tier9Reward"),
            HarmonyPatch(typeof(PitController), "tier10Reward"),
            HarmonyPatch(typeof(PitController), "tier11Reward"),
            HarmonyPatch(typeof(PitController), "tier12Reward"),
            HarmonyPatch(typeof(PitController), "tier13Reward"),
            HarmonyPatch(typeof(PitController), "tier14Reward"),
            HarmonyPatch(typeof(PitController), "tier15Reward"),
            HarmonyPatch(typeof(PitController), "tier16Reward"),
            HarmonyPatch(typeof(PitController), "tier1TotalReward")]
        private static void pit_before()
        {
            _before = _curAdvPower;
            _nextSource = Sources.Pit;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthPowerSpell), "castAdventurePowerupSpell")]
        private static void pill_before()
        {
            _before = _curAdvPower;
            _nextSource = Sources.IronPill;
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(AdventurePurchases), "buy1Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy10Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy100Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy1000Attack"),
            HarmonyPatch(typeof(AdventurePurchases), "buy10KAttack"),
            HarmonyPatch(typeof(AdventurePurchases), "buyCustomPower"),
            //HarmonyPatch(typeof(AllItemListController), "checkforBonuses"),
            HarmonyPatch(typeof(FruitController), "consumeAdventureFruit"),
            HarmonyPatch(typeof(ItopodPerkController), "doEffect"),
            HarmonyPatch(typeof(PitController), "tier1Reward"),
            HarmonyPatch(typeof(PitController), "tier2Reward"),
            HarmonyPatch(typeof(PitController), "tier3Reward"),
            HarmonyPatch(typeof(PitController), "tier4Reward"),
            HarmonyPatch(typeof(PitController), "tier5Reward"),
            HarmonyPatch(typeof(PitController), "tier6Reward"),
            HarmonyPatch(typeof(PitController), "tier7Reward"),
            HarmonyPatch(typeof(PitController), "tier8Reward"),
            HarmonyPatch(typeof(PitController), "tier9Reward"),
            HarmonyPatch(typeof(PitController), "tier10Reward"),
            HarmonyPatch(typeof(PitController), "tier11Reward"),
            HarmonyPatch(typeof(PitController), "tier12Reward"),
            HarmonyPatch(typeof(PitController), "tier13Reward"),
            HarmonyPatch(typeof(PitController), "tier14Reward"),
            HarmonyPatch(typeof(PitController), "tier15Reward"),
            HarmonyPatch(typeof(PitController), "tier16Reward"),
            HarmonyPatch(typeof(PitController), "tier1TotalReward"),
            HarmonyPatch(typeof(RebirthPowerSpell), "castAdventurePowerupSpell")]
        private static void after()
        {
            var gained = _curAdvPower - _before;
            _sources[_nextSource] += gained;
        }




        private static string _baseMessage;

        [HarmonyPostfix, HarmonyPatch(typeof(TooltipDisplay), "Start")]
        private static void TooltipDisplay_Start_postfix(TooltipDisplay __instance)
        {
            //__instance.SetMessage($"[id:{__instance.id}] {__instance.GetMessage()}");

            if (__instance.id != 6)
                return;

            _baseMessage = __instance.GetMessage();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TooltipDisplay), "OnPointerEnter")]
        private static void TooltipDisplay_OnPointerEnter_prefix(TooltipDisplay __instance)
        {
            if (__instance.id != 6)
                return;

            var sources = new List<(string source, float value, float pct)>();
            for (var s = 0; s < Sources.COUNT; s++)
                if(_sources[s] > 0)
                    sources.Add((Sources.Name(s), _sources[s], _totalGained == 0 ? 0f : _sources[s] / _totalGained));

            var sum = _sources.Sum();
            var other = _totalGained - sum;
            if (other > 0)
                sources.Add(("Other", other, _totalGained == 0 ? 0f : other / _totalGained));

            sources.Sort(sorter);
            var lines = sources.Select(s => $"  <b>{s.source}:</b> {Plugin.Character.display(s.value)} <color=blue>({s.pct * 100f:0.#}%)</color>");
            var sourcesText = string.Join("\n", lines);

            __instance.SetMessage($"{_baseMessage}\n\n<b>Total Base Gained:</b> {Plugin.Character.display(_totalGained)}\n{sourcesText}");
        }

        private static int sorter((string s, float f1, float f2) a, (string s, float f1, float f2) b) => b.f2.CompareTo(a.f2);

        internal static class Sources
        {
            internal const int COUNT = 6;

            internal const int Exp = 0;
            internal const int SewerSet = 1;
            internal const int Fruit = 2;
            internal const int Perks = 3;
            internal const int Pit = 4;
            internal const int IronPill = 5;

            internal static string Name(int source)
            {
                return source switch
                {
                    0 => "Exp",
                    1 => "Sewer Set",
                    2 => "Fruit",
                    3 => "Perk 2",
                    4 => "Money Pit",
                    5 => "Iron Pill",
                    _ => $"??({source})"
                };
            }
        }
    }

    internal static class TrackBaseAdvPowerGained_Extensions
    {
        private static FieldInfo messageField = typeof(TooltipDisplay).GetField("message", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static string GetMessage(this TooltipDisplay td) => messageField.GetValue(td) as string;
        internal static void SetMessage(this TooltipDisplay td, string message) => messageField.SetValue(td, message);
    }
}
