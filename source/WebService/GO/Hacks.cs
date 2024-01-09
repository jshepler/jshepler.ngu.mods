using System;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class Hacks
    {
        private static Action _uiUpdateAction;

        static Hacks()
        {
            Plugin.OnUpdate += (o, e) =>
            {
                if (_uiUpdateAction != null)
                {
                    _uiUpdateAction();
                    _uiUpdateAction = null;
                }
            };
        }

        internal static void ApplyHackTargets(string json)
        {
            var hacks = Plugin.Character.hacks.hacks;
            var targets = JSON.Parse(json).AsArray;

            for (var x = 0; x < 15; x++)
                hacks[x].target = targets[x].AsLong;

            _uiUpdateAction = Plugin.Character.hacksController.refreshMenu;
        }

        internal static string BuildHackStats()
        {
            var root = new JSONObject();
            root.Add("rpow", new JSONNumber(Plugin.Character.totalRes3Power()));
            root.Add("rcap", new JSONNumber(Plugin.Character.totalCapRes3()));
            root.Add("hackspeed", new JSONNumber(Plugin.Character.hacksController.totalHackSpeedBonus()));

            var arr = new JSONArray();
            root.Add("hacks", arr);

            var hacks = GetHacks();

            foreach (var h in hacks)
            {
                var node = new JSONObject();
                node.Add("level", new JSONNumber(h.level));
                node.Add("reducer", new JSONNumber(h.redcuer));

                arr.Add(node);
            }

            return root.ToString();
        }

        private static Hack[] GetHacks()
        {
            var hacks = new Hack[15];

            for (var x = 0; x < 15; x++)
                hacks[x] = new Hack
                {
                    level = Plugin.Character.hacks.hacks[x].level,
                    redcuer = ShowHackMilestoneReducersTooltip.GetReducerCount(Plugin.Character, x)
                };

            return hacks;
        }

        private class Hack
        {
            public long level;
            public long redcuer;
        }
    }
}
