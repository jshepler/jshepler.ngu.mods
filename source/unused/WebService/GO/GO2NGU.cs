using System;
using System.IO;
using System.Net;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class GO2NGU
    {
        internal static Action HandleRequest(HttpListenerContext context, string resource)
        {
            string body = null;

            if (context.Request.HasEntityBody)
            {
                using (var sr = new StreamReader(context.Request.InputStream))
                {
                    body = sr.ReadToEnd();
                    context.Request.InputStream.Close();
                    sr.Close();
                }
            }
            else
            {
                context.Response.SendResponse(HttpStatusCode.BadRequest, "missing payload");
                return () => Plugin.ShowOverrideNotification($"GO2NGU: bad request \"{resource}\" - missing payload");
            }

            switch (resource)
            {
                case "loadouts":
                    Loadouts.ImportFromJSON(body);
                    context.Response.SendResponse(HttpStatusCode.OK);
                    return () => Plugin.ShowOverrideNotification("GO2NGU: loadouts");

                case "hacks":
                    Hacks.ApplyHackTargets(body);
                    context.Response.SendResponse(HttpStatusCode.OK);
                    return () => Plugin.ShowOverrideNotification("GO2NGU: hacks");

                default:
                    context.Response.SendResponse(HttpStatusCode.BadRequest, $"unknown resource: {resource}");
                    return () => Plugin.ShowOverrideNotification("GO2NGU: loadouts");
            }
        }
    }
}
