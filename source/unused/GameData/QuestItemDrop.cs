using jshepler.ngu.mods.GameData.DropConditions;

namespace jshepler.ngu.mods.GameData
{
    internal class QuestItemDrop
    {
        internal Items QuestItem;
        internal SetCompleteDropCondition Condition;

        internal QuestItemDrop(Items item, ItemSets itemSet)
        {
            QuestItem = item;
            Condition = new SetCompleteDropCondition(itemSet);
        }
    }
}
