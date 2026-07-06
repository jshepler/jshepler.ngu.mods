using System.Linq;

namespace jshepler.ngu.mods.GameData.DropConditions
{
    internal class EnemiesKilledDropCondition : BaseDropCondition
    {
        private int[] _enemyIds;

        internal static EnemiesKilledDropCondition Walerp5Killed = new EnemiesKilledDropCondition(Enemies.WALDERP5);

        internal EnemiesKilledDropCondition(params Enemies[] enemies) : this(enemies.Cast<int>().ToArray()) { }

        internal EnemiesKilledDropCondition(params int[] enemyIds)
        {
            _enemyIds = enemyIds;
        }

        internal override bool IsConditionMet()
        {
            return _enemyIds.All(i => Plugin.Character.bestiary.enemies[i].kills > 0);
        }
    }
}
