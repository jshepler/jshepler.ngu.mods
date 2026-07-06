using System;

namespace jshepler.ngu.mods.GameData.DropConditions
{
    internal static class PuzzleDropConditions
    {
        internal static T10PuzzleStartedDropCondition T10PuzzleStarted = new();
    }

    internal class T10PuzzleStartedDropCondition : BaseDropCondition
    {
        internal override bool IsConditionMet()
        {
            return Plugin.Character.adventure.titan10questStarted;
        }
    }
}
