using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace jshepler.ngu.mods.WebService
{
    internal static class ContentTypes
    {
        internal const string PlainText = "text/plain";
        internal const string JSON = "application/json";
    }

    [HarmonyPatch]
    internal class Listener
    {
        // because the listener is not running in the UI thread, or even part of Unity's lifecycle (e.g. Update),
        // anything that needs to do something in the UI (e.g. showing notifications) is added to queue and executed in Update
        private static Queue<Action> _actions = new();

        [HarmonyPrepare, HarmonyPatch]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                if (HttpListener.IsSupported)
                    Task.Run(() => RunListener());
                else
                    Plugin.LogInfo("HttpListener NOT SUPPORTED!!!");
            };

            Plugin.OnUpdate += (o, e) =>
            {
                while (_actions.Count > 0)
                    _actions.Dequeue()();
            };
        }

        private static async Task RunListener()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(Options.RemoteTriggers.UrlPrefix.Value);
            listener.Start();

            while (true)
            {
                var context = await listener.GetContextAsync();

                // handle preflight requests
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.SendResponse(HttpStatusCode.OK);
                    continue;
                }

                // [0] /
                // [1] ngu/
                var segments = context.Request.Url.Segments.Skip(2).Select(s => s.TrimEnd('/').ToLowerInvariant()).ToArray();
                Dispatch(context, segments);
            }
        }

        private static void Dispatch(HttpListenerContext context, string[] segments)
        {
            switch (segments[0])
            {
                case "totaltimeplayed":
                    TotalTimePlayed.HandleRequest(context);
                    break;

                case "trigger":
                    _actions.Enqueue(Triggers.Dispatcher.HandleRequest(context, segments[1]));
                    break;

                case "ngu2go":
                    _actions.Enqueue(GO.NGU2GO.HandleRequest(context, segments[1]));
                    break;

                case "go2ngu":
                    _actions.Enqueue(GO.GO2NGU.HandleRequest(context, segments[1]));
                    break;

                case "data":
                    _actions.Enqueue(Data.HandleRequest(context, segments));
                    break;

                case "autoboost":
                case "automerge":
                case "tossgold":
                case "fightboss":
                case "kitty":
                    _actions.Enqueue(Triggers.Dispatcher.HandleRequest(context, segments[0]));
                    context.Response.SendResponse(HttpStatusCode.OK);
                    break;

                default:
                    context.Response.SendResponse(HttpStatusCode.BadRequest, $"unknown handler: {segments[0]}");
                    break;
            }
        }
    }

    internal static class HttpListenerResponseExtension
    {
        internal static void SendResponse(this HttpListenerResponse response, HttpStatusCode status, string responseString = "OK", string contentType = ContentTypes.PlainText)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
            response.StatusCode = (int)status;
            response.ContentType = contentType;

            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            response.Close();
        }
    }
}
