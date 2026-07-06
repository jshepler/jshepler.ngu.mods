using jshepler.ngu.mods.GameData.DropConditions;

namespace jshepler.ngu.mods.GameData
{
    internal class MacGuffinDrop
    {
        internal Items MacGuffinItem;
        internal BaseDropCondition Condition;

        internal MacGuffinDrop(Items item, BaseDropCondition condition = null)
        {
            MacGuffinItem = item;
            Condition = condition;
        }
    }
}
