using System;
using System.Net;

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
    }
}
