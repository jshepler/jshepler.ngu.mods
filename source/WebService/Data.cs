using System;
using System.Net;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService
{
    internal class Data
    {
        internal static Action HandleRequest(HttpListenerContext context, string[] segments)
        {
            var isGET = context.Request.HttpMethod == "GET";
            string json = null;

            switch (segments[1])
            {
                case "base_emr3":
                    json = getBaseEMR();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("data: GET base EMR3");

                default:
                    context.Response.SendResponse(HttpStatusCode.BadRequest, $"unknown resource: {segments[1]}");
                    return () => Plugin.ShowOverrideNotification($"data; unknown resource: {segments[1]}");
            }
        }

        private static string getBaseEMR()
        {
            var root = new JSONObject();
            root.Add("epower", Plugin.Character.energyPower);
            root.Add("ecap", Plugin.Character.capEnergy);
            root.Add("ebars", Plugin.Character.energyBars);
            root.Add("mpower", Plugin.Character.magic.magicPower);
            root.Add("mcap", Plugin.Character.magic.capMagic);
            root.Add("mbars", Plugin.Character.magic.magicPerBar);
            root.Add("r3power", Plugin.Character.res3.res3Power);
            root.Add("r3cap", Plugin.Character.res3.capRes3);
            root.Add("r3bars", Plugin.Character.res3.res3PerBar);

            return root.ToString();
        }
    }
}
