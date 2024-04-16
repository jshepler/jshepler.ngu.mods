using System;
using System.Net;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class NGU2GO
    {
        internal static Action HandleRequest(HttpListenerContext context, string resource)
        {
            string json;

            switch (resource)
            {
                case "augstats":
                    json = Augments.BuildHackStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: aug stats");

                case "ngustats":
                    json = NGUs.BuildNGUStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: ngu stats");

                case "nakedemr":
                    json = getNakedEMR();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: naked EMR3");

                case "hacks":
                    json = Hacks.BuildHackStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: hacks");

                case "equipped":
                    json = Loadouts.BuildCurrentEquipJson();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: equipped");

                default:
                    context.Response.SendResponse(HttpStatusCode.BadRequest, $"unknown resource: {resource}");
                    return () => Plugin.ShowOverrideNotification($"NGU2GO: unknown resource: {resource}");
            }
        }

        private static string getNakedEMR()
        {
            var c = Plugin.Character;
            var bonuses = c.inventoryController.bonuses;

            var root = new JSONObject();
            root.Add("Nude Energy Cap", (long)(c.totalCapEnergy() / (1.0 + bonuses[specType.EnergyCap] + bonuses[specType.EnergyCap3] + bonuses[specType.AllCap])));
            root.Add("Nude Magic Cap", (long)(c.totalCapMagic() / (1f + bonuses[specType.MagicCap] + bonuses[specType.MagicCap3] + bonuses[specType.AllCap])));
            root.Add("Nude Energy Power", c.totalEnergyPower() / (1f + bonuses[specType.EnergyPower] + bonuses[specType.EnergyPower2] + bonuses[specType.EnergyPower3] + bonuses[specType.AllPower]));
            root.Add("Nude Magic Power", c.totalMagicPower() / (1f + bonuses[specType.MagicPower] + bonuses[specType.MagicPower2] + bonuses[specType.MagicPower3] + bonuses[specType.AllPower]));
            root.Add("Nude Energy Bars", (long)(c.totalEnergyBar() / (1f + bonuses[specType.EnergyPerBar] + bonuses[specType.EnergyPerBar2] + bonuses[specType.EnergyPerBar3] + bonuses[specType.AllPerBar])));
            root.Add("Nude Magic Bars", (long)(c.totalMagicBar() / (1f + bonuses[specType.MagicPerBar] + bonuses[specType.MagicPerBar2] + bonuses[specType.MagicPerBar3] + bonuses[specType.AllPerBar])));
            root.Add("Nude Resource 3 Power", c.totalRes3Power() / (1f + bonuses[specType.Res3Power]));
            root.Add("Nude Resource 3 Cap", (long)(c.totalCapRes3() / (1.0 + bonuses[specType.Res3Cap])));
            root.Add("Nude Resource 3 Bars", (long)(c.totalRes3Bar() / (1f + bonuses[specType.Res3Bar])));
            root.Add("modifiers", true);

            return root.ToString();
        }
    }
}
