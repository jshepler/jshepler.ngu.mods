using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class RemoteTriggers
    {
        private static object[] _noArgs = new object[0];
        private static Button _settingsButton;
        private static Queue<string> _messages = new();

        private static bool InManualFight => Plugin.Character.adventureController.fightInProgress && !Plugin.Character.adventure.autoattacking;

        private static bool _remoteTriggersEnabled
        {
            get => Options.RemoteTriggers.Enabled.Value;
            set => Options.RemoteTriggers.Enabled.Value = value;
        }

        private static bool _autoBoostEnabled
        {
            get => Options.RemoteTriggers.AutoBoost.Enabled.Value;
            set => Options.RemoteTriggers.AutoBoost.Enabled.Value = value;
        }

        private static bool _autoMergeEnabled
        {
            get => Options.RemoteTriggers.AutoMerge.Enabled.Value;
            set => Options.RemoteTriggers.AutoMerge.Enabled.Value = value;
        }

        private static bool _tossGoldEnabled
        {
            get => Options.RemoteTriggers.TossGold.Enabled.Value;
            set => Options.RemoteTriggers.TossGold.Enabled.Value = value;
        }

        private static bool _fightBossEnabled
        {
            get => Options.RemoteTriggers.FightBoss.Enabled.Value;
            set => Options.RemoteTriggers.FightBoss.Enabled.Value = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_prefix()
        {
            _settingsButton = GameObject.Find("Canvas/Player Panel Canvas/Player Panel/Settings").GetComponent<Button>();
            _settingsButton.image.color = _remoteTriggersEnabled ? Plugin.ButtonColor_Green : Color.white;

            _settingsButton.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e => OpenConfig());

            if (!HttpListener.IsSupported)
                Plugin.LogInfo("HttpListener NOT SUPPORTED!!!");
            else
                Task.Run(() => RunListener());

            InitPopup();
            Plugin.OnUpdate += (o, e) =>
            {
                if (_messages.Count > 0)
                {
                    var msg = _messages.Dequeue();
                    Plugin.Character.tooltip.showOverrideTooltip(msg, 4f);
                }

                RunCommands();
            };
        }

        private static WaitForSeconds _notificationDuration = new WaitForSeconds(4.0f);
        private static IEnumerator ShowNotification(string message)
        {
            Plugin.Character.tooltip.showTooltip(message, Screen.width * 0.5f, Screen.height * 0.4f);
            yield return _notificationDuration;
            Plugin.Character.tooltip.hideTooltip();
        }

        // I believe this method is running in a background/worker thread and so it (or anything it calls) shouldn't directly call any game/unity code.
        // I'm using some static bools that Update() checks every frame and will call the appropriate game/unity code
        private static async Task RunListener()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(Options.RemoteTriggers.UrlPrefix.Value);
            listener.Start();

            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var parts = request.Url.Segments;

                var command = parts[2].TrimEnd('/');
                if (command.ToLowerInvariant() == "totaltimeplayed")
                {
                    SendResponse(context.Response, HttpStatusCode.OK, GetTotalTimePlayed(), false);
                }
                else
                {
                    var responseString = SetCommandFlags(command);
                    SendResponse(context.Response, responseString == null ? HttpStatusCode.NotImplemented : HttpStatusCode.OK, responseString ?? "not implemented");
                }
            }
        }

        private static void SendResponse(HttpListenerResponse response, HttpStatusCode status, string responseString, bool displayNotification = true)
        {
            var buffer = Encoding.UTF8.GetBytes($"<HTML><BODY> {responseString}</BODY></HTML>");
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)status;

            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

            if (displayNotification)
                _messages.Enqueue(responseString);
        }

        private static bool _autoBoost = false;
        private static bool _autoMerge = false;
        private static bool _tossGold = false;
        private static bool _tossingGold = false;
        private static bool _fightBoss = false;
        private static bool _fightingBoss = false;

        private static string SetCommandFlags(string command)
        {
            if (!_remoteTriggersEnabled)
                return "remote triggers disabled";

            switch (command.ToLowerInvariant())
            {
                case "boostall":
                case "autoboost":
                    if (_autoBoostEnabled)
                    {
                        _autoBoost = true;
                        return "auto-boost: triggered";
                    }

                    return "auto-boost: disabled";

                case "automerge":
                    if (_autoMergeEnabled)
                    {
                        _autoMerge = true;
                        return "auto-merge: triggered";
                    }

                    return "auto-merge: disabled";

                case "tossgold":
                    if (!_tossGoldEnabled)
                        return "toss gold: disabled";

                    if (InManualFight)
                        return "toss gold: ignored - manual adventure fight in progress";

                    if (_tossingGold)
                        return "toss gold: ignored - already triggered";

                    if (!Plugin.Character.pitController.canToss())
                        return "toss gold: ignored - pit not ready";

                    _tossGold = true;
                    return "toss gold: triggered";

                case "fightboss":
                    if (!_fightBossEnabled)
                        return "fight boss: disabled";

                    if (InManualFight)
                        return "fight boss: ignored - manual adventure fight in progress";

                    if (_fightingBoss)
                        return "fight boss: ignoring - already triggered";

                    if (Plugin.Character.bossController.isFighting)
                        return "fight boss: ignoring - fight in progress";

                    if (!CanFightBoss.CanFight)
                        return "fight boss: ignored - boss not beatable";

                    _fightBoss = true;
                    return "fight boss: triggered";

                default:
                    return null;
            };
        }

        private static void ToggleRemoteTriggersEnabled()
        {
            _remoteTriggersEnabled = !_remoteTriggersEnabled;
            _settingsButton.image.color = _remoteTriggersEnabled ? Plugin.ButtonColor_Green : Color.white;
        }

        private static void RunCommands()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    OpenConfig();
                }
                else
                {
                    ToggleRemoteTriggersEnabled();
                }
            }

            if (_autoBoost)
            {
                Plugin.Character.inventoryController.autoBoost();
                _autoBoost = false;
            }

            if (_autoMerge)
            {
                Plugin.Character.inventoryController.autoMerge();
                _autoMerge = false;
            }

            if (_tossGold)
            {
                _tossingGold = true;
                _tossGold = false;
                Plugin.Character.StartCoroutine(TossGold());
            }

            if (_fightBoss)
            {
                _fightingBoss = true;
                _fightBoss = false;
                Plugin.Character.StartCoroutine(DoFight());
            }
        }


        private static WaitForSeconds _waitQuarterSecond = new WaitForSeconds(0.25f);
        private static WaitForSeconds _waitHalfSecond = new WaitForSeconds(.5f);

        private static IEnumerator DoFight()
        {
            var currentMenu = Plugin.Character.CurrentMenu();

            Plugin.Character.menuSwapper.SwapMenu(Menu.FightBoss);
            yield return _waitQuarterSecond;

            if (CanFightBoss.CanNuke)
            {
                Plugin.Character.bossController.startNuke();

                while (Plugin.Character.bossController.isFighting)
                    yield return _waitQuarterSecond;
            }

            if (CanFightBoss.CanFight)
            {
                Plugin.Character.bossController.beginFight();

                while (Plugin.Character.bossController.isFighting)
                    yield return _waitQuarterSecond;
            }

            yield return _waitHalfSecond;
            Plugin.Character.menuSwapper.SwapMenu(currentMenu);
            _fightingBoss = false;
        }

        private static MethodInfo _tossGoldMethod = typeof(PitController).GetMethod("engage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static WaitForSeconds _waitForPitReward = new WaitForSeconds(5f);
        private static IEnumerator TossGold()
        {
            var currentMenu = Plugin.Character.CurrentMenu();

            Plugin.Character.menuSwapper.SwapMenu(Menu.MoneyPit);
            yield return _waitQuarterSecond;

            _tossGoldMethod.Invoke(Plugin.Character.pitController, _noArgs);
            yield return _waitForPitReward;

            Plugin.Character.menuSwapper.SwapMenu(currentMenu);
            _tossingGold = false;
        }

        private static string GetTotalTimePlayed()
        {
            var totalseconds = Plugin.Character.totalPlaytime.totalseconds;
            var path = $"{Paths.ConfigPath}\\TotalTimePlayed.htm";

            if (!File.Exists(path))
                File.WriteAllText(path, _defaultTimeHtml);

            var html = File.ReadAllText(path).Replace("%totalSeconds%", totalseconds.ToString());

            return html;
        }

        private static string _defaultTimeHtml = @"
<html>
    <head>
        <style>
            body {
                background-color: #000;
                color: #fff;
                font-size: 32px;
                font-weight: bold;
            }
        </style>
    </head>
    <body>
        <div id='time'></div>
        <script type='text/javascript'>
            let totalTimePlayed = %totalSeconds%;
            let el = document.getElementById(""time"");

            let start = document.timeline.currentTime;
            requestAnimationFrame(onFrame);

            function onFrame(ts)
            {
                displayTime(totalTimePlayed + (ts - start) / 1000);
                requestAnimationFrame(onFrame);
            }

            function displayTime(totalSeconds)
            {
                let days = Math.floor(totalSeconds / 86400);
                totalSeconds -= days * 86400;
    
                let hours = Math.floor(totalSeconds / 3600);
                totalSeconds -= hours * 3600;
    
                let minutes = Math.floor(totalSeconds / 60);
                totalSeconds -= minutes * 60;
    
                let seconds = Math.floor(totalSeconds);
                totalSeconds -= seconds;
                
                let ms = Math.floor(totalSeconds * 10);

                let text = days + "" days "" + (""0"" + hours).slice(-2) + "":"" + (""0"" + minutes).slice(-2) + "":"" + (""0"" + seconds).slice(-2) + ""."" + ms
                el.innerText = text;
            }
        </script>
    </body>
</html>
";

        #region config popup

        const string ROOTCANVAS = "Canvas";
        private static GameObject _blocker;
        private static Rect _windowRect;
        private static bool _isOpen = false;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _selected;
        private static GUIStyle _notSelected;

        private static void InitPopup()
        {
            _windowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 114, 400, 260);
            _blocker = buildBLocker();
            IsOpen = false;

            Plugin.onGUI += (o, e) =>
            {
                if (!IsOpen)
                    return;

                if (Input.GetKeyDown(KeyCode.Escape))
                    CloseConfig();
                else
                    DrawWindow();
            };
        }

        private static bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                _blocker.SetActive(value);
            }
        }

        private static void OpenConfig()
        {
            IsOpen = true;
        }

        private static void CloseConfig()
        {
            IsOpen = false;
        }

        private static void DrawWindow()
        {
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = new Color32(50, 50, 50, 255).CreateSolidColorTexture(_windowRect);

                _titleStyle = new GUIStyle("label");
                _titleStyle.alignment = TextAnchor.MiddleCenter;

                _notSelected = new GUIStyle("button");
                _notSelected.normal.background = Texture2D.blackTexture;

                _selected = new GUIStyle("button");
                _selected.normal.background = Texture2D.whiteTexture;
                _selected.normal.textColor = Color.black;
                _selected.hover.background = Texture2D.whiteTexture;
                _selected.hover.textColor = Color.black;
            }

            GUILayout.BeginArea(_windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Configure Remote Triggers</b>", _titleStyle);
            if (GUILayout.Button("×", GUILayout.Width(25))) CloseConfig();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Auto Boost");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", _autoBoostEnabled ? _selected : _notSelected)) _autoBoostEnabled = true;
            if (GUILayout.Button("disabled", !_autoBoostEnabled ? _selected : _notSelected)) _autoBoostEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Auto Merge");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", _autoMergeEnabled ? _selected : _notSelected)) _autoMergeEnabled = true;
            if (GUILayout.Button("disabled", !_autoMergeEnabled ? _selected : _notSelected)) _autoMergeEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Toss Gold");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", _tossGoldEnabled ? _selected : _notSelected)) _tossGoldEnabled = true;
            if (GUILayout.Button("disabled", !_tossGoldEnabled ? _selected : _notSelected)) _tossGoldEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Fight Boss");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", _fightBossEnabled ? _selected : _notSelected)) _fightBossEnabled = true;
            if (GUILayout.Button("disabled", !_fightBossEnabled ? _selected : _notSelected)) _fightBossEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // modeled after https://github.com/AppertaFoundation/IXN_IBM_MIND/blob/8aa21d78bfe90e6feeab3cce6256e7fbb6036734/Whack-A-Mole/Library/PackageCache/com.unity.textmeshpro%402.0.0/Scripts/Runtime/TMP_Dropdown.cs#L847
        private static GameObject buildBLocker()
        {
            var blocker = new GameObject("Blocker");

            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(GameObject.Find(ROOTCANVAS).transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            blocker.AddComponent<GraphicRaycaster>();

            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;
            //blockerImage.color = new Color(0, 0, 0, .5f);

            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(() =>
            {
                if (!_windowRect.Contains(Event.current.mousePosition))
                    CloseConfig();
            });

            return blocker;
        }

        #endregion
    }
}
