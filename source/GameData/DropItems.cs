using System.Linq;
using jshepler.ngu.mods.GameData.DropConditions;

namespace jshepler.ngu.mods.GameData
{
    internal class DropItems
    {
        internal long BaseAmount = 1;
        internal int[] ItemIds;
        internal float BaseDC;
        internal float MaxDC = 1.0f;
        internal float BonuseDC = 0.0f;
        internal BaseDropCondition Condition;

        internal DropItems(float baseDC, params Items[] items)
        {
            ItemIds = items.Cast<int>().ToArray();
            BaseDC = baseDC;
        }

        internal DropItems(float baseDC, float maxDC, params Items[] items)
        {
            ItemIds = items.Cast<int>().ToArray();
            BaseDC = baseDC;
            MaxDC = maxDC;
        }

        internal DropItems(float baseDC, long baseAmount, params Items[] items)
        {
            ItemIds = items.Cast<int>().ToArray();
            BaseDC = baseDC;
            BaseAmount = baseAmount;
        }

        internal DropItems(float baseDC, float maxDC, long baseAmount, params Items[] items)
        {
            ItemIds = items.Cast<int>().ToArray();
            BaseDC = baseDC;
            MaxDC = maxDC;
            BaseAmount = baseAmount;
        }
    }
}
