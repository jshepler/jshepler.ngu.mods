using System.Collections.Generic;
using System.Linq;

namespace jshepler.ngu.mods
{
    internal static class Wishes
    {
        private static List<WishWrapper> _wishes;
        internal static List<WishWrapper> AllWishes
        {
            get
            {
                if(_wishes == null)
                    _wishes = Enumerable.Range(0, Plugin.Character.wishes.wishes.Count).Select(i => new WishWrapper(i)).ToList();

                return _wishes;
            }
        }

        private static int _maxWishSlots => Plugin.Character.wishesController.curWishSlots();
        internal static IEnumerable<WishWrapper> RunningWishes => AllWishes.Where(w => w.Energy > 0 && w.Magic > 0 && w.Res3 > 0).Take(_maxWishSlots);
        internal static IEnumerable<WishWrapper> PartiallyRunningWishes => AllWishes.Where(w => w.Energy > 0 || w.Magic > 0 || w.Res3 > 0).Take(_maxWishSlots);
        internal static IEnumerable<WishWrapper> CurValidUpgradesList => Plugin.Character.wishesController.curValidUpgradesList.Select(i => AllWishes[i]);
    }

    internal class WishWrapper
    {
        private static List<Wish> _wishes => Plugin.Character.wishes.wishes;
        private static List<WishProperties> _props = Plugin.Character.wishesController.properties;

        internal WishWrapper(int id)
        {
            Id = id;
        }

        internal int Id;
        internal string Name => _props[Id].wishName;
        internal int MaxLevel => (int)_props[Id].maxLevel;
        internal int Level => _wishes[Id].level;
        internal float Progress => _wishes[Id].progress;

        internal long Energy
        {
            get => _wishes[Id].energy;
            set => _wishes[Id].energy = value;
        }

        internal long Magic
        {
            get => _wishes[Id].magic;
            set => _wishes[Id].magic = value;
        }

        internal long Res3
        {
            get => _wishes[Id].res3;
            set => _wishes[Id].res3 = value;
        }

        internal bool IsRunning => Energy > 0 && Magic > 0 && Res3 > 0;
    }
}
