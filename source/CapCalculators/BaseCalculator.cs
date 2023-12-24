using System;

namespace jshepler.ngu.mods.CapCalculators
{
    internal abstract class BaseCalculator
    {
        private bool _dirty = true;

        internal BaseCalculator()
        {
            Plugin.OnSaveLoaded += (o, e) => _dirty = true;
            Plugin.OnLateUpdate += (o, e) => _dirty = true;
        }

        protected abstract void UpdateModifier();
        protected abstract long GetResource(long L);
        protected abstract long GetLevel(long r);
        protected abstract double GetProgressPerTick(long r, long L);

        internal virtual long ResourceFromLevel(long level)
        {
            if (_dirty)
            {
                UpdateModifier();
                _dirty = false;
            }

            return GetResource(level);
        }

        internal long LevelFromResource(long resource)
        {
            if (_dirty)
            {
                UpdateModifier();
                _dirty = false;
            }

            return GetLevel(resource);
        }

        internal double TimeToTarget(long currentLevel, long targetLevel, long resource, double progressToNextLevel = 0.0)
        {
            if (targetLevel <= currentLevel)
                return 0.0;

            // account for partial progress from current level to next level
            var totalTicks = Math.Ceiling((1.0 - progressToNextLevel) / GetProgressPerTick(resource, currentLevel + 1));
            currentLevel++;

            var threshold = ResourceFromLevel(101);
            //Plugin.LogInfo($"currentLevel: {currentLevel}, targetLevel: {targetLevel}, resource: {resource}, threshold: {threshold}");

            if (resource <= threshold)
                totalTicks += ticksMethod1(currentLevel, targetLevel, resource);
            else
                totalTicks += ticksMethod2(currentLevel, targetLevel, resource);

            return totalTicks / 50.0;
        }

        // brute-force looping over each level and calculating t/bar
        private double ticksMethod1(long currentLevel, long targetLevel, long resource)
        {
            var ticks = 0.0;

            for (var x = currentLevel + 1; x <= targetLevel; x++)
                ticks += Math.Ceiling(1.0 / GetProgressPerTick(resource, x));

            return ticks;
        }

        // breakpoints are the multiples of BB level, starting at BB level + 1
        // (e.g. if BB level is 100, 100-101 is 1t but 101-102 is 2t, so 101 is where 2t starts)
        //
        // if startLevel < first breakpoint, then first breakpoint - startLevel are at 1t/bar (ticksPerBar or tpb)
        // if startLevel >= first breakpoint, then keep increasing breakpoint by bbLevel and tpb by 1 until startLevel < breakpoint
        private double ticksMethod2(long currentLevel, long targetLevel, long resource)
        {
            var levelsLeft = targetLevel - currentLevel;
            var bbLevel = LevelFromResource(resource);
            var ticksPerBar = 1;
            var breakpoint = bbLevel + 1;
            var lastBreakpoint = 0L;

            while (currentLevel >= breakpoint)
            {
                lastBreakpoint = breakpoint;
                breakpoint += bbLevel;
                ticksPerBar++;
            }

            // lastBreakpoint was used to find the lowest breakpoint < current level,
            // but it starts at currentLevel so that the first block is currentLevel - breakpoint
            lastBreakpoint = currentLevel;

            var ticks = 0.0;
            while (levelsLeft > 0)
            {
                var levels = breakpoint - lastBreakpoint;
                if (levels > levelsLeft)
                    levels = levelsLeft;


                ticks += (levels * ticksPerBar);
                levelsLeft -= levels;
                //Plugin.LogInfo($"levels: {levels}, totalTicks: {totalTicks}, levelsLeft: {levelsLeft}");

                lastBreakpoint = breakpoint;
                breakpoint += bbLevel;
                ticksPerBar++;
            }

            //Plugin.LogInfo($"returning: {NumberOutput.timeOutput(totalTicks / 50.0)}");
            return ticks;
        }
    }
}
