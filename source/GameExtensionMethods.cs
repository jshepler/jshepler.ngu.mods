using System;
using System.Collections.Generic;
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
            return l.head == -1000 && l.chest == -1000 && l.legs == -1000 && l.boots == -1000
                && l.weapon == -1000 && l.weapon2 == -1000
                && l.accessories.TrueForAll(i => i == -1000);
        }

        internal static Equipment GetItem(this Inventory inv, int slotId)
        {
            return slotId switch
            {
                -1000 => null,
                -6 => inv.weapon2,
                -5 => inv.weapon,
                -4 => inv.boots,
                -3 => inv.legs,
                -2 => inv.chest,
                -1 => inv.head,
                _ => slotId < 10000 ? inv.inventory[slotId]
                    : slotId < 100000 ? inv.accs[slotId - 10000]
                    : inv.macguffins[slotId - 100000]
            };
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

        internal static IEnumerable<CodeInstruction> DumpToLog(this IEnumerable<CodeInstruction> instructions)
        {
            Plugin.LogInfo($"\n{instructions.Join(i => $"{i}", "\n")}");
            return instructions;
        }

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

        // based on https://stackoverflow.com/a/43639947
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
            var l = (long)num;
            if (num > l)
                l++;

            return l;
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

            var hasDecimals = Math.Truncate(rounded) == rounded;
            var format = "#,##0";

            if (minDecimalPlaces > 0 || hasDecimals)
                format += ".";

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
    }
}
