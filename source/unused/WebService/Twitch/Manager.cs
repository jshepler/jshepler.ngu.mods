using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;

namespace jshepler.ngu.mods.WebService.Twitch
{
    [HarmonyPatch]
    internal class Manager
    {
        const string URL_WSS = "wss://eventsub.wss.twitch.tv/ws";
        //const string URL_WSS = "ws://127.0.0.1:8080/ws";

        internal static bool IsConnected;
        internal static bool IsConnecting;

        private static string _appAccessToken
        {
            get => Options.Twitch.AppAccessToken.Value;
            set => Options.Twitch.AppAccessToken.Value = value;
        }

        private static string _userAccessToken
        {
            get => Options.Twitch.UserAccessToken.Value;
            set => Options.Twitch.UserAccessToken.Value = value;
        }

        private static string _userRefreshToken
        {
            get => Options.Twitch.UserRefreshToken.Value;
            set => Options.Twitch.UserRefreshToken.Value = value;
        }

        private static string _userId;
        private static string _sessionId;
        private static WebSocketClient _wsc;

        private static Action<string> Log = s => Plugin.LogInfo($"[{DateTime.Now:H:mm:ss.fff}] tMAN: {s}");
        private static Queue<Action> _actions = new();
        private static void ShowNotification(string msg)
        {
            _actions.Enqueue(() => Plugin.ShowOverrideNotification($"TWITCH: {msg}"));
        }

        [HarmonyPrepare, HarmonyPatch]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                if (Options.RemoteTriggers.Enabled.Value == true
                    && Options.Twitch.Enabled.Value == true
                    && Options.Twitch.AutoConnect.Value == true)
                    Connect();
            };

            Plugin.OnUpdate += (o, e) =>
            {
                while (_actions.Count > 0)
                    _actions.Dequeue()();
            };
        }

        internal static void Connect()
        {
            _ = Task.Run(() => ConnectAsync().ConfigureAwait(false));
        }

        private static async Task ConnectAsync()
        {
            Log("ConnectAsync()");
            if (_wsc != null && _wsc.State != WebSocketState.Closed)
                await _wsc.StopAsync();

            IsConnected = false;
            IsConnecting = true;

            if (string.IsNullOrWhiteSpace(_appAccessToken) && !(await GetAppAccessToken()))
            {
                ShowNotification($"failed getting app access token, aborting");
                return;
            }

            if (!(await GetUserId()))
            {
                ShowNotification("failed getting userId, aborting");
                return;
            }

            _wsc = new WebSocketClient();
            _wsc.OnMessage += OnSocketMessage;
            _wsc.OnDisconnect += OnSocketDisconnect;

            await _wsc.StartAsync(URL_WSS);

            IsConnected = true;
            IsConnecting = false;
        }

        internal static void Disconnect()
        {
            _ = DisconnectAsync();
        }

        private static async Task DisconnectAsync()
        {
            Log("DisconnectAsync()");
            if (_wsc != null && _wsc.State == WebSocketState.Open)
            {
                await _wsc.StopAsync();
                _wsc = null;
            }

            _sessionId = null;
            IsConnected = false;
        }

        internal static void Reset()
        {
            _ = ResetAsync();
        }

        private static async Task ResetAsync()
        {
            Log("ResetAync()");

            await DisconnectAsync();

            _userAccessToken = string.Empty;
            _userRefreshToken = string.Empty;

            await ConnectAsync();
        }

        private static async Task<bool> GetAppAccessToken()
        {
            Log("GetAppAccessToken()");

            var res = await API.GetAppAccessToken();
            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} getting app access token, aborting");
                return false;
            }

            _appAccessToken = res.Result["access_token"].Value;

            return true;
        }

        private static async Task<bool> RefreshUserAccessToken()
        {
            Log("RefreshUserAccessToken()");

            var res = await API.RefreshUserAccessToken(_userRefreshToken);
            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} refreshing user access token, aborting");
                return false;
            }

            _userAccessToken = res.Result["access_token"].Value;
            _userRefreshToken = res.Result["refresh_token"].Value;

            return true;
        }

        private static async Task<bool> GetUserAccessToken()
        {
            Log("GetUserAccessToken()");

            if (!string.IsNullOrWhiteSpace(_userRefreshToken) && (await RefreshUserAccessToken()))
                return true;

            var res = await API.GetUserAuthCode();
            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} getting user auth code, aborting");
                return false;
            }

            var code = res.Result.Value;
            res = await API.GetUserAccessToken(code);
            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} getting user access token, aborting");
                return false;
            }

            _userAccessToken = res.Result["access_token"].Value;
            _userRefreshToken = res.Result["refresh_token"].Value;

            return true;
        }

        private static async Task<bool> GetUserId()
        {
            Log("GetUserId()");

            if (string.IsNullOrWhiteSpace(_userAccessToken) && !(await GetUserAccessToken()))
                return false;

            var res = await API.GetUserId(_userAccessToken);

            if (res.Status == HttpStatusCode.Unauthorized)
            {
                if (!(await GetUserAccessToken()))
                {
                    Log("unable to get userId - 401 unauthorized and failed to get new user access token, aborting");
                    return false;
                }

                res = await API.GetUserId(_userAccessToken);

                if (res.Status == HttpStatusCode.Unauthorized)
                {
                    Log("401 unauthorized getting userId even after getting new user access token, aborting");
                    return false;
                }
            }

            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} getting userId, aborting");
                return false;
            }

            _userId = res.Result["data"].AsArray[0]["id"].Value;
            return true;
        }

        private static async Task<bool> SendSubscriptions()
        {
            Log("SendSubscriptions()");

            if (string.IsNullOrWhiteSpace(_sessionId)
                || (string.IsNullOrWhiteSpace(_userId) && !(await GetUserId()))
                || (string.IsNullOrWhiteSpace(_userAccessToken) && !(await GetUserAccessToken())))
            {
                Log("SendSubscriptions() aborted - no sessionId or no sessionId or no access token");
                return false;
            }

            var res = await API.SendSubscriptions(_userAccessToken, _userId, _sessionId);

            if (res.Status == HttpStatusCode.Unauthorized)
            {
                if (!(await GetUserAccessToken()))
                {
                    Log("unable to send subscriptions - 401 unauthorized and failed to get new user access token, aborting");
                    return false;
                }

                res = await API.SendSubscriptions(_userAccessToken, _userId, _sessionId);

                if (res.Status == HttpStatusCode.Unauthorized)
                {
                    Log("401 unauthorized sending subscriptions even after getting new user access token, aborting");
                    return false;
                }
            }

            if (res.Status != HttpStatusCode.OK)
            {
                Log($"HTTP {res.Status} sending subscriptions, aborting");
                return false;
            }

            return true;
        }

        private static TaskCompletionSource<bool> reconnectTaskSource = null;
        private static async void OnSocketMessage(object sender, TwitchEventArgs e)
        {
            switch (e.Message.metaData.messageType)
            {
                case TwitchMessage.MessageTypes.Welcome:
                    if (reconnectTaskSource == null)
                    {
                        _sessionId = e.Message.payload.session.id;
                        _ = SendSubscriptions();
                    }
                    else
                    {
                        reconnectTaskSource.SetResult(true);
                    }

                    break;

                case TwitchMessage.MessageTypes.Reconnect:
                    _wsc.OnMessage -= OnSocketMessage;
                    _wsc.OnDisconnect -= OnSocketDisconnect;

                    var newWSC = new WebSocketClient();
                    newWSC.OnMessage += OnSocketMessage;
                    newWSC.OnDisconnect += OnSocketDisconnect;

                    reconnectTaskSource = new TaskCompletionSource<bool>();

                    var reconnectUrl = e.Message.payload.session.reconnectUrl;
                    await newWSC.StartAsync(reconnectUrl);
                    
                    await reconnectTaskSource.Task;
                    reconnectTaskSource = null;

                    await _wsc.StopAsync();
                    _wsc = newWSC;

                    break;

                case TwitchMessage.MessageTypes.Notification:
                    _actions.Enqueue(Triggers.Dispatcher.HandleTwitchReward(e.Message.payload.@event.reward));
                    break;

                case TwitchMessage.MessageTypes.KeepAlive:
                    // do nothing, but don't want to log it
                    break;

                default: // ignore anything else
                    Log($"unhandled message \"{e.Message.metaData.messageType}\", raw: {e.RawMessage}");
                    break;
            }
        }

        private static void OnSocketDisconnect(object sender, EventArgs e)
        {
            var wsc = sender as WebSocketClient;

            Log($"OnSocketDisconnect(socketId: {wsc?.SocketId ?? 0})");

            if (wsc != null && wsc == _wsc)
            {
                IsConnected = false;

                if (wsc.SocketException)
                    _ = Task.Run(async () =>
                    {
                        Log("attempting re-connect in 3 seconds");
                        await Task.Delay(3000);
                        return ConnectAsync();
                    });

                _wsc.OnMessage -= OnSocketMessage;
                _wsc.OnDisconnect -= OnSocketDisconnect;
                _wsc = null;
            }
        }
    }
}
