using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class ApiClientResponse
    {
        internal HttpStatusCode Status;
        internal JSONNode Result;
    }

    internal class API
    {
        const string REDIRECT = "twitch/oauth";
        const string SCOPE = "channel:read:subscriptions channel:read:redemptions user:read:email";
        const string URL_OAUTH_AUTHORIZE = "https://id.twitch.tv/oauth2/authorize";
        const string URL_OAUTH_TOKEN = "https://id.twitch.tv/oauth2/token";
        const string URL_USERS = "https://api.twitch.tv/helix/users";
        const string URL_SUBS = "https://api.twitch.tv/helix/eventsub/subscriptions";
        //const string URL_SUBS = "http://127.0.0.1:8080/eventsub/subscriptions";

        private static string _clientId => Options.Twitch.ClientId.Value;
        private static string _clientSecret = Options.Twitch.ClientSecret.Value;
        private static string _redirectUrl = Options.RemoteTriggers.UrlPrefix.Value + REDIRECT;

        private static Action<string> Log = s => Plugin.LogInfo($"[{DateTime.Now:H:mm:ss.fff}] tAPI: {s}");

        internal static Task<ApiClientResponse> GetAppAccessToken()
        {
            Log("GetAppAccessToken()");
            var data = new NameValueCollection
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" }
            };

            return PostData(URL_OAUTH_TOKEN, data);
        }

        internal static Task<ApiClientResponse> GetUserAccessToken(string userAuthCode)
        {
            Log("GetUserAccessToken()");

            var data = new NameValueCollection
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "code", userAuthCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", _redirectUrl }
            };

            return PostData(URL_OAUTH_TOKEN, data);
        }

        internal static Task<ApiClientResponse> RefreshUserAccessToken(string userRefreshToken)
        {
            Log("RefreshUserAccessToken()");

            var data = new NameValueCollection
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", Uri.EscapeDataString(userRefreshToken) }
            };

            return PostData(URL_OAUTH_TOKEN, data);
        }

        internal static Task<ApiClientResponse> GetUserId(string userAccessToken)
        {
            Log("GetUserId()");

            var headers = new NameValueCollection
            {
                { "Authorization", $"Bearer {userAccessToken}" },
                { "Client-Id", _clientId }
            };

            return GetData(URL_USERS, headers);
        }

        internal static Task<ApiClientResponse> SendSubscriptions(string userAccessToken, string userId, string websocketSessionId)
        {
            Log("SendSubscriptions()");

            var headers = new NameValueCollection
            {
                { "Authorization", $"Bearer {userAccessToken}" },
                { "Client-Id", _clientId }
            };

            var root = new JSONObject();
            root.Add("type", "channel.channel_points_custom_reward_redemption.add");
            root.Add("version", "1");

            var condition = new JSONObject();
            condition.Add("broadcaster_user_id", userId);
            root.Add("condition", condition);

            var transport = new JSONObject();
            transport.Add("method", "websocket");
            transport.Add("session_id", websocketSessionId);
            root.Add("transport", transport);

            var json = root.ToString();
            return PostJson(URL_SUBS, json, headers);
        }

        private static string _csrf;
        private static TaskCompletionSource<ApiClientResponse> userAuthTask;

        internal static Task<ApiClientResponse> GetUserAuthCode()
        {
            Log("GetUserAuthCode()");

            _csrf = Guid.NewGuid().ToString();
            userAuthTask = new TaskCompletionSource<ApiClientResponse>();

            var url = URL_OAUTH_AUTHORIZE
                + $"?client_id={_clientId}"
                + $"&redirect_uri={_redirectUrl}"
                + $"&response_type=code"
                + $"&scope={Uri.EscapeDataString(SCOPE)}"
                + $"&state={_csrf}";


            System.Diagnostics.Process.Start(url);
            return userAuthTask.Task;
        }

        internal static Action HandleAuthRedirectRequest(HttpListenerContext context)
        {
            Log("HandleAuthRedirectRequest()");

            var data = context.Request.QueryString;
            var response = new ApiClientResponse();

            var csrf = data["state"];
            if (_csrf != null && (csrf == null || csrf != _csrf))
            {
                Log($"auth handler: CSRF attack detected, expected \"{_csrf}\' but got \"{csrf}\"");
                context.Response.SendResponse(HttpStatusCode.NotAcceptable, "CSRF attack detected, not accepting authorization code!");

                response.Status = HttpStatusCode.NotAcceptable;
                userAuthTask.SetResult(response);

                return () => Plugin.ShowOverrideNotification("TWITCH/OAUTH: CSRF attack detected, not accepting authorization code!");
            }

            var error = data["error"];
            if (error != null)
            {
                Log($"auth handler: error, send log to shep: {data["error_description"]}");
                context.Response.SendResponse(HttpStatusCode.NotAcceptable, data["error_description"]);

                response.Status = HttpStatusCode.NotAcceptable;
                userAuthTask.SetResult(response);

                return () => Plugin.ShowOverrideNotification("TWITCH/OAUTH: error getting user authorization code");
            }

            var code = data["code"];
            if (code == null)
            {
                Log($"auth handler: missing code - send log to shep");
                context.Response.SendResponse(HttpStatusCode.NotAcceptable, "missing \"code\" in querystring, contact shep!");

                response.Status = HttpStatusCode.NoContent;
                userAuthTask.SetResult(response);

                return () => Plugin.ShowOverrideNotification("TWITCH/OAUTH: error getting user authorization code");
            }

            response.Status = HttpStatusCode.OK;
            response.Result = code;
            userAuthTask.SetResult(response);

            context.Response.SendResponse(HttpStatusCode.OK, "auth token received, ok to close this tab");
            return () => Plugin.ShowOverrideNotification("TWITCH/OAUTH: authorized");
        }


        private static async Task<ApiClientResponse> PostData(string url, NameValueCollection data, NameValueCollection headers = null)
        {
            using var wc = new WebClient();
            if (headers != null)
                wc.Headers.Add(headers);

            byte[] buffer;
            var response = new ApiClientResponse();

            try
            {
                buffer = await wc.UploadValuesTaskAsync(url, data);
                response.Status = HttpStatusCode.OK;
                response.Result = JSONNode.Parse(Encoding.UTF8.GetString(buffer));
            }

            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse r)
                {
                    Log($"PostData(): error - HTTP {r.StatusCode}");
                    response.Status = r.StatusCode;
                }
                else
                {
                    Log($"PostData(): error - WebExceptionStatus.{ex.Status}");
                    response.Status = HttpStatusCode.Unused;
                }
            }

            catch (Exception ex)
            {
                Log($"PostData(): exception - {ex.Message}");
                response.Status = HttpStatusCode.Unused;
            }

            return response;
        }

        private static async Task<ApiClientResponse> PostJson(string url, string json, NameValueCollection headers = null)
        {
            using var wc = new WebClient();

            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            if (headers != null)
                wc.Headers.Add(headers);

            string result;
            var response = new ApiClientResponse();

            try
            {
                result = await wc.UploadStringTaskAsync(url, json);
                response.Status = HttpStatusCode.OK;
                response.Result = JSONNode.Parse(result);
            }

            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse r)
                {
                    Log($"PostJson(): error - HTTP {r.StatusCode}");
                    response.Status = r.StatusCode;
                }
                else
                {
                    Log($"PostJson(): error - WebExceptionStatus.{ex.Status}");
                    response.Status = HttpStatusCode.Unused;
                }
            }

            catch (Exception ex)
            {
                Log($"PostJson(): exception - {ex.Message}");
                response.Status = HttpStatusCode.Unused;
            }

            return response;
        }

        private static async Task<ApiClientResponse> GetData(string url, NameValueCollection headers)
        {
            using var wc = new WebClient();
            wc.Headers.Add(headers);

            string result;
            var response = new ApiClientResponse();

            try
            {
                result = await wc.DownloadStringTaskAsync(url);
                response.Status = HttpStatusCode.OK;
                response.Result = JSONNode.Parse(result);
            }

            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse r)
                {
                    Log($"GetData(): error - HTTP {r.StatusCode}");
                    response.Status = r.StatusCode;
                }
                else
                {
                    Log($"GetData(): error - WebExceptionStatus.{ex.Status}");
                    response.Status = HttpStatusCode.Unused;
                }
            }

            catch (Exception ex)
            {
                Log($"GetData(): exception - {ex.Message}");
                response.Status = HttpStatusCode.Unused;
            }

            return response;
        }
    }
}
