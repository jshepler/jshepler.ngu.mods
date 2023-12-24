using System;

namespace jshepler.ngu.mods.GameData.DropConditions
{
    internal class CustomDropCondition : BaseDropCondition
    {
        private Func<bool> _predicate;

        internal CustomDropCondition(Func<bool> predicate)
        {
            _predicate = predicate;
        }

        internal override bool IsConditionMet()
        {
            return _predicate();
        }
    }
}
