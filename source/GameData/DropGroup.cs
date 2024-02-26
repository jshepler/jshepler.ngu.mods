using System;

namespace jshepler.ngu.mods.GameData
{
    internal class DropGroup
    {
        internal DropGroup(params DropItems[] items) : this(0f, items) { }
        internal DropGroup(double baseGold, params DropItems[] items)
        {
            BaseGold = baseGold;
            Items = items;
        }

        internal double BaseGold;
        internal DropItems[] Items;
    }

    internal class EnemyDropGroup : DropGroup
    {
        internal int EnemyId;

        internal EnemyDropGroup(Enemies enemy, params DropItems[] items) : this(enemy, 0f, items) { }
        internal EnemyDropGroup(Enemies enemy, double baseGold, params DropItems[] items) : base(baseGold, items)
        {
            EnemyId = (int)enemy;
        }

        internal bool HasVisibleDrops()
        {
            foreach (var di in Items)
            {
                if (di.Condition == null)
                    continue;

                if (di.Condition.IsConditionMet() == false)
                    return false;
            }

            return true;
        }
    }
}
