using System;

namespace jshepler.ngu.mods.GameData.DropConditions
{
    internal class WishDropCondition : BaseDropCondition
    {
        private int _wishId;

        internal static WishDropCondition T6QP = new WishDropCondition(73);
        internal static WishDropCondition T7QP = new WishDropCondition(74);
        internal static WishDropCondition T8QP = new WishDropCondition(40);
        internal static WishDropCondition T9QP = new WishDropCondition(41);
        internal static WishDropCondition T10QP = new WishDropCondition(100);
        internal static WishDropCondition T11QP = new WishDropCondition(187);
        internal static WishDropCondition T12QP = new WishDropCondition(204);

        internal WishDropCondition(int wishId)
        {
            _wishId = wishId;
        }

        internal override bool IsConditionMet()
        {
            return Plugin.Character.wishes.wishes[_wishId].level > 0;
        }
    }
}
