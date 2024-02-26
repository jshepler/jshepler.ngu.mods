using System;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class Augments
    {
        private static Action _uiUpdateAction;

        static Augments()
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

        internal static string BuildHackStats()
        {
            var root = new JSONObject();
            root.Add("augspeed", Plugin.Character.augmentsController.getTotalSpeedFactor());
            root.Add("ecap", Plugin.Character.totalCapEnergy());
            root.Add("gps", Plugin.Character.goldPerSecond());
            root.Add("lsc", Plugin.Character.challenges.laserSwordChallenge.curCompletions);
            root.Add("nac", Plugin.Character.challenges.noAugsChallenge.curCompletions);

            return root.ToString();
        }
    }
}
