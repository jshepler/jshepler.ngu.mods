using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AdvancedTrainingTitanAK
    {
        // this removes an extra line-feed that's only on the "BEAST SPAWN READY" line, it bothered me
        [HarmonyTranspiler, HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        private static IEnumerable<CodeInstruction> ButtonShower_showTitanTimer_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldString = "\n<b>THE BEAST SPAWN READY</b>\n";
            var newString = "\n<b>THE BEAST SPAWN READY</b>";

            foreach (var i in instructions)
            {
                if (i.opcode == OpCodes.Ldstr && (string)i.operand == oldString)
                    i.operand = newString;

                yield return i;
            }
        }


        private static Character character;
        private static AllAdvancedTraining advancedTraining;

        private static float totalPowerWithoutAdvPower;
        private static float totalDefWithoutAdvDef;
        private static float totalRegenWithoutAdvDef;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_showTitanTimer_Start(ButtonShower __instance)
        {
            character = __instance.character;
            advancedTraining = character.advancedTrainingController;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showTitanTimer")]
        private static void ButtonShower_showTitanTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            if (!Plugin.GameHasStarted || __instance.adventure.interactable == false || string.IsNullOrWhiteSpace(___message))
                return;

            totalPowerWithoutAdvPower = character.totalAdvAttack() / (advancedTraining.adventurePowerBonus(0) + 1f);
            totalDefWithoutAdvDef = character.totalAdvDefense() / (advancedTraining.adventureToughnessBonus(0) + 1f);
            totalRegenWithoutAdvDef = character.totalAdvHPRegen() / (advancedTraining.adventureToughnessBonus(0) + 1f);

            var effectiveBossId = character.effectiveBossID();
            var sb = new StringBuilder();
            PTR pt;
            var currentPT = new PTR(character.advancedTraining.level[1], character.advancedTraining.level[0]);

            // T1
            if (effectiveBossId >= 58 || character.achievements.achievementComplete[128])
            {
                pt = ComputeATPT(3000f, 2500f);
                if (pt != null) sb.Append(BuildTitanString("GRB", pt, pt < currentPT));
            }

            // T2
            if (effectiveBossId >= 66 || character.achievements.achievementComplete[129])
            {
                pt = ComputeATPT(9000f, 7000f);
                if (pt != null) sb.Append(BuildTitanString("GCT", pt, pt < currentPT));
            }

            // T3
            if (effectiveBossId >= 82 || character.bestiary.enemies[304].kills > 0)
            {
                pt = ComputeATPT(25000f, 15000f);
                if (pt != null) sb.Append(BuildTitanString("JAKE", pt, pt < currentPT));
            }

            // T4
            if (effectiveBossId >= 100 || character.achievements.achievementComplete[130])
            {
                pt = ComputeATPT(800000f, 400000f, 14000f);
                if (pt != null) sb.Append(BuildTitanString("UUG", pt, pt < currentPT));
            }

            // T5
            if (effectiveBossId >= 116 || character.achievements.achievementComplete[145])
            {
                pt = ComputeATPT(1.3e+7f, 7.0e+6f, 150000f);
                if (pt != null) sb.Append(BuildTitanString("WALDERP", pt, pt < currentPT));
            }

            // T6
            if (effectiveBossId >= 132 || character.adventure.boss6Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    pt = x switch
                    {
                        1 => ComputeATPT(2.5E+09f, 1.6E+09f, 2.5E+07f),
                        2 => ComputeATPT(2.5E+10f, 1.6E+10f, 2.5E+08f),
                        3 => ComputeATPT(2.5E+11f, 1.6E+11f, 2.5E+09f),
                        4 => ComputeATPT(2.5E+12f, 1.6E+12f, 2.5E+10f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"BEAST V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T7
            if (effectiveBossId >= 426 || character.adventure.boss7Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    pt = x switch
                    {
                        1 => ComputeATPT(5E+14f, 2.5E+14f, 5E+12f),
                        2 => ComputeATPT(1E+16f, 5E+15f, 1E+14f),
                        3 => ComputeATPT(2E+17f, 1E+17f, 2E+15f),
                        4 => ComputeATPT(5E+18f, 2.5E+18f, 5E+16f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"NERD V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T8
            if (effectiveBossId >= 467 || character.adventure.boss8Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    pt = x switch
                    {
                        1 => ComputeATPT(5E+18f, 2.5E+18f, 5E+16f),
                        2 => ComputeATPT(1E+20f, 5E+19f, 1E+18f),
                        3 => ComputeATPT(2E+21f, 1E+21f, 2E+19f),
                        4 => ComputeATPT(5E+22f, 2.5E+22f, 5E+20f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"GM V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T9
            if (effectiveBossId >= 491 || character.adventure.boss9Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    if (character.bestiary.enemies[343 + x].kills >= 24) continue;

                    pt = x switch
                    {
                        1 => ComputeATPT(1E+23f, 5E+22f, 1E+21f),
                        2 => ComputeATPT(2E+24f, 1E+24f, 2E+22f),
                        3 => ComputeATPT(4E+25f, 2E+25f, 4E+23f),
                        4 => ComputeATPT(7.5E+26f, 3.7E+26f, 7.5E+24f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"EXILE V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T10
            if (effectiveBossId >= 727 || character.adventure.boss10Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    if (character.bestiary.enemies[364 + x].kills >= 4) continue;

                    pt = x switch
                    {
                        1 => ComputeATPT(4E+28f, 2E+28f, 4E+26f),
                        2 => ComputeATPT(3.2E+29f, 1.6E+29f, 1.6E+27f),
                        3 => ComputeATPT(2E+30f, 1E+30f, 9.999999E+27f),
                        4 => ComputeATPT(1E+31f, 5E+30f, 5E+28f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"IT-H V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T11
            if (effectiveBossId >= 826 || character.adventure.boss11Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    if (character.bestiary.enemies[368 + x].kills >= 4) continue;

                    pt = x switch
                    {
                        1 => ComputeATPT(1.8E+31f, 6E+30f, 1.2E+29f),
                        2 => ComputeATPT(9E+31f, 3E+31f, 6E+29f),
                        3 => ComputeATPT(3.6E+32f, 1.2E+32f, 2.5E+30f),
                        4 => ComputeATPT(1.1E+33f, 3.6E+32f, 7.5E+30f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"LOB V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // T12
            if (effectiveBossId >= 848 || character.adventure.boss12Kills >= 1)
            {
                for (var x = 1; x < 5; x++)
                {
                    if (character.bestiary.enemies[372 + x].kills >= 4) continue;

                    pt = x switch
                    {
                        1 => ComputeATPT(3E+33f, 1E+33f, 2E+31f),
                        2 => ComputeATPT(1.2E+34f, 4E+33f, 8E+31f),
                        3 => ComputeATPT(3.6E+34f, 1.2E+34f, 2.4E+32f),
                        4 => ComputeATPT(7.2E+34f, 2.4E+34f, 4.8E+32f),
                        _ => null
                    };

                    if (pt == null) continue;
                    sb.Append(BuildTitanString($"AMAL V{x}", pt, pt < currentPT));
                    if (pt > currentPT) break;
                }
            }

            // don't think these can be autokilled, leaving these jic might need in the future

            // TIPPI
            if (effectiveBossId >= 897 || character.adventure.ratTitanDefeated)
            {
            }

            // TRAITOR
            if (effectiveBossId >= 902 && character.adventure.ratTitanDefeated)
            {
            }

            if (sb.Length > 0)
            {
                ___message += $"\n\n<b>Adv. Training needed to autokill Titans:</b>{sb}";
                __instance.tooltip.showTooltip(___message);
            }
        }

#pragma warning disable Harmony003
        private static string BuildTitanString(string titan, PTR pt, bool? haveIt = null)
        {
            var color = haveIt.HasValue ? (haveIt.Value ? "green" : "red") : "black";
            return $"\n<color={color}>{titan}:  T={character.display(pt.Toughness)}, P={character.display(pt.Power)}</color>";
        }
#pragma warning restore Harmony003

        private static PTR ComputeATPT(float akPower, float akToughness, float akRegen = 0f)
        {
            if (totalPowerWithoutAdvPower >= akPower && totalDefWithoutAdvDef >= akToughness) return null;

            var atPowerPct = ((akPower / totalPowerWithoutAdvPower) - 1) * 100f; // -1 to convert form "multiplier" to "bonus"
            var atPower = (long)Math.Ceiling(Math.Pow(atPowerPct / 10, 2.5)); // https://ngu-idle.fandom.com/wiki/Advanced_Training#Formulas
            //if (float.IsNaN(atPower)) atPower = 0;
            if (atPower < 0) atPower = 0;

            var atDefPct = ((akToughness / totalDefWithoutAdvDef) - 1) * 100f; // -1 to convert from "multiplier" to "bonus"
            var atTough = (long)Math.Ceiling(Math.Pow(atDefPct / 10, 2.5)); // https://ngu-idle.fandom.com/wiki/Advanced_Training#Formulas
            //if(float.IsNaN(atTough)) atTough = 0f;
            if (atTough < 0) atTough = 0;

            // regen also gets AT toughness multiplier, calc AT toughness needed for regen
            // and if higher than what's needed for akToughness, use it instead
            if (akRegen > 0f)
            {
                var atRegenPct = ((akRegen / totalRegenWithoutAdvDef) - 1) * 100f;
                var atToughRegen = (long)Math.Ceiling(Math.Pow(atRegenPct / 10, 2.5));
                //if (float.IsNaN(atToughRegen)) atToughRegen = 0;

                if (atToughRegen > atTough) atTough = atToughRegen;
            }

            if (atPower == 0 && atTough == 0) return null;

            return new PTR(atPower, atTough);
        }

        class PTR
        {
            public long Power;
            public long Toughness;
            public long Regen;

            public PTR(long power, long toughness, long regen = 0)
            {
                this.Power = power;
                this.Toughness = toughness;
                this.Regen = regen;
            }

            public PTR(float power, float toughness, float regen = 0f)
            {
                this.Power = (long)Math.Ceiling(power);
                this.Toughness = (long)Math.Ceiling(toughness);
                this.Regen = (long)Math.Ceiling(regen);
            }

            public static bool operator >(PTR a, PTR b)
            {
                var aDefense = a.Toughness > a.Regen ? a.Toughness : a.Regen;
                var bDefense = b.Toughness > b.Regen ? b.Toughness : b.Regen;

                return (a.Power > b.Power) || (aDefense > bDefense);
            }

            public static bool operator <(PTR a, PTR b)
            {
                return !(a > b);
            }
        }




        // not using this class or the following Dictionary, but keeping it in case I ever change my mind,
        // don't want to type all this crap again
        class TitanAutokillRequirements
        {
            public string Name;
            public PTR[] Requirements;
            public Func<int> GetVersion;

            public TitanAutokillRequirements(string name, Func<int> fnGetVersion, PTR[] ptReqs)
            {
                this.Name = name;
                this.GetVersion = fnGetVersion ?? new Func<int>(() => 0);
                this.Requirements = ptReqs;
            }
        }

        // data from AdventureController manageFight() and autokillTitanXAchieved()
        // key is effective boss id for titan
        private static Dictionary<int, TitanAutokillRequirements> _tar = new Dictionary<int, TitanAutokillRequirements>
        {
            {58, new TitanAutokillRequirements("GRB", null, new[] { new PTR(3000f, 2500f) }) },
            {66, new TitanAutokillRequirements("GCT", null, new[] { new PTR(9000f, 7000f) }) },
            {82, new TitanAutokillRequirements("JAKE", null, new[] { new PTR(25000f, 15000f) }) },
            {100, new TitanAutokillRequirements("UUG", null, new[] { new PTR(800000f, 400000f, 14000f) }) },
            {116, new TitanAutokillRequirements("WALDERP", null, new[] { new PTR(1.3e+7f, 7.0e+6f, 150000f) }) },

            {132, new TitanAutokillRequirements("THE BEAST",
                () => character.adventure.titan6Version,
                new[]
                {
                    new PTR(2.5E+09f, 1.6E+09f, 2.5e+07f),
                    new PTR(2.5E+10f, 1.6E+10f, 2.5e+08f),
                    new PTR(2.5E+11f, 1.6E+11f, 2.5e+09f),
                    new PTR(2.5E+12f, 1.6E+12f, 2.5e+10f)
                })},

            {426, new TitanAutokillRequirements("GREASY NERD",
                () => character.adventure.titan7Version,
                new []
                {
                    new PTR(5E+14f, 2.5E+14f, 5E+12f),
                    new PTR(1E+16f, 5E+15f, 1E+14f),
                    new PTR(2E+17f, 1E+17f, 2E+15f),
                    new PTR(5E+18f, 2.5E+18f, 5E+16f)
                })
            },

            // GODMOTHER
            {467, null },
            
            // EXILE
            {491, null },
            
            // IT HUNGERS
            {727, null },
            
            // ROCK LOBSTER
            {826, null },
            
            // AMALGAMATE
            {848, null },
            
            // TIPPI
            {897, null },
            
            // TRAITOR
            {902, null }
        };
    }
}
