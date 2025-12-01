using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    internal static class GameExtensionMethods
    {
        #region loadouts

        internal static void SetInventorySlotId(this LoadoutDisplayController ldc, int invSlotId)
        {
            ldc.character.inventory.loadouts[ldc.loadoutID].SetInventorySlotId(ldc.id, invSlotId);
        }

        internal static int GetInventorySlotId(this LoadoutDisplayController ldc)
        {
            return ldc.character.inventory.loadouts[ldc.loadoutID].GetInventorySlotId(ldc.id);
        }

        internal static Equipment GetItem(this LoadoutDisplayController ldc)
        {
            return Plugin.Character.inventory.GetItem(ldc.GetInventorySlotId());
        }

        internal static part GetEqupmentType(this LoadoutDisplayController ldc)
        {
            return ldc.id switch
            {
                -1 => part.Head,
                -2 => part.Chest,
                -3 => part.Legs,
                -4 => part.Boots,
                -5 => part.Weapon,
                -6 => part.Weapon,
                _ => part.Accessory
            };
        }

        internal static void SetInventorySlotId(this Loadout loadout, int loadoutSlotId, int invSlotId)
        {
            switch (loadoutSlotId)
            {
                case -1:
                    loadout.head = invSlotId;
                    break;

                case -2:
                    loadout.chest = invSlotId;
                    break;

                case -3:
                    loadout.legs = invSlotId;
                    break;

                case -4:
                    loadout.boots = invSlotId;
                    break;

                case -5:
                    loadout.weapon = invSlotId;
                    break;

                case -6:
                    loadout.weapon2 = invSlotId;
                    break;

                default:
                    if (loadoutSlotId >= 10000 && loadoutSlotId < 100000)
                    {
                        loadout.accessories[loadoutSlotId - 10000] = invSlotId;
                    }
                    break;
            }
        }

        internal static int GetInventorySlotId(this Loadout loadout, int loadoutSlotId)
        {
            return loadoutSlotId switch
            {
                -1 => loadout.head,
                -2 => loadout.chest,
                -3 => loadout.legs,
                -4 => loadout.boots,
                -5 => loadout.weapon,
                -6 => loadout.weapon2,
                _ => loadout.accessories[loadoutSlotId - 10000]
            };
        }

        internal static bool IsEmpty(this Loadout l)
        {
            var weap2Unlocked = Plugin.Character.inventoryController.weapon2Unlocked();
            return l.head == -1000 && l.chest == -1000 && l.legs == -1000 && l.boots == -1000
                && l.weapon == -1000
                && (weap2Unlocked || l.weapon2 == -1000)
                && l.accessories.TrueForAll(i => i == -1000);
        }

        internal static Equipment GetItem(this Inventory inv, int slotId)
        {
            try
            {
                if (slotId < 0)
                    return slotId switch
                    {
                        -1000 => null, // nothing
                        -100 => null, // inf cube
                        -6 => inv.weapon2,
                        -5 => inv.weapon,
                        -4 => inv.boots,
                        -3 => inv.legs,
                        -2 => inv.chest,
                        -1 => inv.head,
                        _ => null
                        //< 10000 => inv.inventory[slotId],
                        //< 100000 => inv.accs[slotId - 10000],
                        //< 1000000 => inv.daycare[slotId - 100000],
                        //_ => inv.macguffins[slotId - 1000000]
                    };

                if (slotId < 10000)
                    return (slotId < inv.inventory.Count) ? inv.inventory[slotId] : null;

                if (slotId < 100000)
                    return (slotId - 10000) < inv.accs.Count ? inv.accs[slotId - 10000] : null;

                if (slotId < 1000000)
                    return (slotId - 100000) < inv.daycare.Count ? inv.daycare[slotId - 100000] : null;

                if (slotId < 10000000)
                    return (slotId - 1000000) < inv.macguffins.Count ? inv.macguffins[slotId - 1000000] : null;

                Plugin.LogInfo($"unknown slotId: {slotId}");
                return null;
            }

            catch (Exception ex)
            {
                Plugin.LogInfo($"exception getting item for slotId: {slotId}\n{ex}");
                return null;
            }
        }

        #endregion

        #region Advanced Training

        internal static AdvancedTrainingController AdvancedTrainingController(this AllAdvancedTraining atc, int id)
        {
            return id switch
            {
                0 => atc.defense,
                1 => atc.attack,
                2 => atc.block,
                3 => atc.wandoosEnergy,
                4 => atc.wandoosMagic,
                _ => null
            };
        }

        internal static long CurrentLevel(this AdvancedTrainingController controller)
        {
            return controller.character.advancedTraining.level[controller.id];
        }

        internal static bool HitTarget(this AdvancedTrainingController controller)
        {
            var target = controller.character.advancedTraining.levelTarget[controller.id];

            if (target == -1) return true;
            if (target == 0) return false;

            return controller.CurrentLevel() >= target;
        }

        #endregion

        #region NGU Energy

        internal static long CurrentLevel(this NGUController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.skills[controller.id].level,
                difficulty.evil => controller.character.NGU.skills[controller.id].evilLevel,
                difficulty.sadistic => controller.character.NGU.skills[controller.id].sadisticLevel,
                _ => -1
            };
        }

        internal static long GetTarget(this NGUController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.skills[controller.id].target,
                difficulty.evil => controller.character.NGU.skills[controller.id].evilTarget,
                difficulty.sadistic => controller.character.NGU.skills[controller.id].sadisticTarget,
                _ => -1
            };
        }

        internal static bool HitTarget(this NGUController controller)
        {
            var target = controller.GetTarget();

            if (target == -1) return true;
            if (target == 0) return false;

            return controller.getLevel() >= target;
        }

        internal static float GetProgress(this NGUController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.skills[controller.id].progress,
                difficulty.evil => controller.character.NGU.skills[controller.id].evilProgress,
                difficulty.sadistic => controller.character.NGU.skills[controller.id].sadisticProgress,
                _ => -1
            };
        }

        #endregion

        #region NGU Magic

        internal static long CurrentLevel(this NGUMagicController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.magicSkills[controller.id].level,
                difficulty.evil => controller.character.NGU.magicSkills[controller.id].evilLevel,
                difficulty.sadistic => controller.character.NGU.magicSkills[controller.id].sadisticLevel,
                _ => -1
            };
        }

        internal static long GetTarget(this NGUMagicController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.magicSkills[controller.id].target,
                difficulty.evil => controller.character.NGU.magicSkills[controller.id].evilTarget,
                difficulty.sadistic => controller.character.NGU.magicSkills[controller.id].sadisticTarget,
                _ => -1
            };
        }

        internal static bool HitTarget(this NGUMagicController controller)
        {
            var target = controller.GetTarget();

            if (target == -1) return true;
            if (target == 0) return false;

            return controller.getLevel() >= target;
        }

        internal static float GetProgress(this NGUMagicController controller)
        {
            return controller.character.settings.nguLevelTrack switch
            {
                difficulty.normal => controller.character.NGU.magicSkills[controller.id].progress,
                difficulty.evil => controller.character.NGU.magicSkills[controller.id].evilProgress,
                difficulty.sadistic => controller.character.NGU.magicSkills[controller.id].sadisticProgress,
                _ => -1
            };
        }

        #endregion

        #region IEnumerables

        // https://stackoverflow.com/a/23164737
        internal static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, int, TResult> selector)
        {
            var index = -1;
            foreach (var s in source)
            {
                checked { ++index; }

                if (predicate(s))
                    yield return selector(s, index);
            }
        }

        internal static void DoMany<TSource, TCollection>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Action<TSource, TCollection> action)
        {
            foreach (TSource sourceItem in source)
                foreach (TCollection innerItem in collectionSelector(sourceItem))
                    action(sourceItem, innerItem);
        }

        internal static void Do<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
        {
            var index = 0;
            foreach (TSource item in source)
                action(item, index++);
        }

        #endregion

        #region Maths

        // https://stackoverflow.com/a/43639947
        internal static decimal Truncate(this decimal d, byte decimals)
        {
            decimal r = Math.Round(d, decimals);

            if (d > 0 && r > d)
            {
                return r - new decimal(1, 0, 0, false, decimals);
            }
            else if (d < 0 && r < d)
            {
                return r + new decimal(1, 0, 0, false, decimals);
            }

            return r;
        }

        internal static long CeilToLong(this float num)
        {
            if (num >= long.MaxValue)
                return long.MaxValue;

            var l = (long)num;
            if (num > l && l < long.MaxValue)
                l++;

            return l;
        }

        internal static long CeilToLong(this double num)
        {
            if (num >= long.MaxValue)
                return long.MaxValue;

            var l = (long)num;
            if (num > l && l < long.MaxValue)
                l++;

            return l;
        }

        internal static long RoundToLong(this float num)
        {
            if (num >= long.MaxValue)
                return long.MaxValue;

            //return long.Parse($"{num:0}");
            return (long)Math.Round(num, MidpointRounding.AwayFromZero);
        }

        internal static long RoundToLong(this double num)
        {
            if (num >= long.MaxValue)
                return long.MaxValue;

            return long.Parse($"{num:0}");
        }

        // 32bit versions of BitConverter.DoubleToInt64Bits and .Int64BitsToDouble
        // because .net framework doesn't have a 32bit versions
        private static unsafe int FloatToInt32Bits(float value)
        {
            return *(int*)(&value);
        }

        private static unsafe float Int32BitsToFloat(int value)
        {
            return *(float*)(&value);
        }

        // from ChatGPT
        // ULP is derived from a float's exponent and instead of doing floor(log2(x)), doing this because
        // 1) it's more accurate since the result of log2() is a float/double, and so could be rounded
        //      whereas this extracts the actual (biased) exponent from the bits
        // 2) this is much faster
        internal static int ExtractExponent(this float value)
        {
            var bits = FloatToInt32Bits(value);
            var exponent = ((bits >> 23) & 0xFF) - 127; // -127 to remove the bias and get the actual exponent

            return exponent;
        }

        // from ChatGPT
        // could be useful to know at which number ULP changes - might add this to wish tooltip to know when min increases
        internal static float MinFloatOfNextExponent(this float value)
        {
            var bits = FloatToInt32Bits(value);
            var currentExp = ((bits >> 23) & 0xFF) - 127;
            var nextExp = currentExp + 1;
            var nextBits = (nextExp + 127) << 23;
            var smallestNext = Int32BitsToFloat(nextBits);

            return smallestNext;
        }

        // provided by discord user Erunion
        // (not using this since doing +1 will only work for numbers where ULP > 2)
        //internal static float MinimumAdditional(this float current)
        //{
        //    float next = current.NextFloat();

        //    // This difference should always be a power of two
        //    float difference = next - current;
        //    //Plugin.LogInfo($"diff: {difference:r}");
        //    if (difference <= 1.0f)
        //        return 1.0f;

        //    // while difference is the actual difference between the values, anything
        //    // more than half of that difference should round up to the next value
        //    var min = difference / 2.0f + 1.0f;
        //    //Plugin.LogInfo($"min: {min:r}");

        //    return min;
        //}

        // provided by discord user Erunion
        internal static float NextFloat(this float value)
        {
            var bits = FloatToInt32Bits(value);
            var next = Int32BitsToFloat(bits + 1);
            //Plugin.LogInfo($"v: {value:r}, b: {bits}, n: {next:r}");

            return next;
        }

        // spacing (or ULP) is the delta to the next representable value - the value from incrementing the LSB
        internal static float ULP(this float value)
        {
            // could do math to calculate it
            //var ulp = (float)Math.Pow(2, value.GetFloatExponent() - 23);

            // but this is faster than doing bit manipulation to get the exponent and doing Math.Pow()
            return value.NextFloat() - value;
        }

        internal static float MinNeededToRoundToNextFloat(this float value)
        {
            var halfULP = value.ULP() / 2f;

            // rounding in floats uses nearest-to-even, so sometimes half will round up and sometimes down
            // check if half is enough and if not, return next float of half
            // (half will be a much smaller scale so its ULP < value's ULP)
            if (value + halfULP > value)
                return halfULP;

            return halfULP.NextFloat();
        }

        #endregion

        #region BepinEx

        internal static IEnumerable<CodeInstruction> DumpToLog(this IEnumerable<CodeInstruction> instructions)
        {
            //Plugin.LogInfo($"\n{instructions.Join(i => $"{i}", "\n")}");
            //return instructions;

            var codeList = new List<CodeInstruction>(instructions);
            var sb = new StringBuilder();

            for (int i = 0; i < codeList.Count; i++)
            {
                var instr = codeList[i];
                string operandStr = string.Empty;

                switch (instr.operand)
                {
                    case FieldInfo field:
                        operandStr = $"{field.FieldType.Name} {field.DeclaringType.FullName}::{field.Name}";
                        break;

                    case MethodInfo method:
                        var paramTypes = string.Join(", ", Array.ConvertAll(method.GetParameters(), p => p.ParameterType.Name));
                        operandStr = $"{(method.IsStatic ? "static" : "instance")} {method.ReturnType.Name} {method.DeclaringType.FullName}::{method.Name}({paramTypes})";
                        break;

                    case Label label:
                        int targetIndex = codeList.FindIndex(ci => ci.labels.Contains(label));
                        operandStr = targetIndex >= 0 ? $"Label->{targetIndex:D3}" : "Label->?";
                        break;

                    case Label[] labels:
                        var labelIndices = new List<string>();
                        foreach (var l in labels)
                        {
                            int targetIndex2 = codeList.FindIndex(ci => ci.labels.Contains(l));
                            labelIndices.Add(targetIndex2 >= 0 ? $"Label->{targetIndex2:D3}" : "Label->?");
                        }
                        operandStr = string.Join(", ", labelIndices);
                        break;

                    case string s:
                        operandStr = $"\"{s}\"";
                        break;

                    default:
                        operandStr = instr.operand?.ToString() ?? string.Empty;
                        break;
                }

                string indexStr = instr.labels.Count > 0 ? $"*{i:D3}" : $" {i:D3}";
                sb.AppendLine($"{indexStr}: {instr.opcode,-10} {operandStr}");
            }

            Plugin.LogInfo($"\n{sb}");
            return instructions;
        }

        // bepinex's SetInstruction (and SetInstructionAndAdvance) don't keep labels, which I would assume you'd want to by default
        internal static CodeMatcher ReplaceInstruction(this CodeMatcher m, CodeInstruction replacement, bool moveLabels = true)
        {
            if(moveLabels)
                replacement.MoveLabelsFrom(m.Instruction);

            m.SetInstruction(replacement);

            return m;
        }

        internal static CodeMatcher ReplaceInstructionAndAdvance(this CodeMatcher m, CodeInstruction replacement, bool moveLabels = true)
        {
            if (moveLabels)
                replacement.MoveLabelsFrom(m.Instruction);

            m.SetInstructionAndAdvance(replacement);

            return m;
        }

        #endregion

        internal static Menu CurrentMenu(this Character character)
        {
            return (Menu)character.menuID;
        }

        internal static bool InMenu(this Character character, Menu menu)
        {
            return character.CurrentMenu() == menu;
        }

        internal static void SwapMenu(this MenuSwapper swapper, Menu menu)
        {
            swapper.swapMenu((int)menu);
        }

        internal static Texture2D CreateSolidColorTexture(this Color32 color, Rect rect)
        {
            return color.CreateSolidColorTexture(rect.size);
        }

        internal static Texture2D CreateSolidColorTexture(this Color32 color, Vector2 size)
        {
            return color.CreateSolidColorTexture((int)size.x, (int)size.y);
        }

        internal static Texture2D CreateSolidColorTexture(this Color32 color, int width, int height)
        {
            var image = new Texture2D((int)width + 1, (int)height + 1, TextureFormat.ARGB32, false);

            for (var x = 0; x < image.width; x++)
                for (var y = 0; y < image.height; y++)
                    image.SetPixel(x, y, color);

            image.Apply();

            return image;
        }

        internal static string display(this Character character, double number, int minDecimalPlaces, int maxDecimalPlaces = 0)
        {
            var rounded = Math.Round(number, Math.Max(minDecimalPlaces, maxDecimalPlaces));

            if (number >= 1000000.0 || (minDecimalPlaces < 1 && maxDecimalPlaces < 1))
                return character.display(rounded);

            var format = "#,##0.";

            if(minDecimalPlaces > 0)
                format += new string('0', minDecimalPlaces);

            if (maxDecimalPlaces > minDecimalPlaces)
                format += new string('#', maxDecimalPlaces - minDecimalPlaces);

            return number.ToString(format);
        }

        internal static string Path(this GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;

            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

        internal static string FormatRoundTripWithSeparators(this float value)
        {
            var s = value.ToString("R");

            var dotIndex = s.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (dotIndex < 0)
                return int.Parse(s).ToString("N0");

            var intPart = s.Substring(0, dotIndex);
            intPart = long.Parse(intPart).ToString("N0");

            var fracPart = s.Substring(dotIndex); // includes the dot

            return intPart + fracPart;
        }
    }
}
