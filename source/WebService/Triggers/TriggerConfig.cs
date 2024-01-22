using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.WebService.Triggers
{
    [HarmonyPatch]
    internal class TriggerConfig
    {
        private static Button _settingsButton;

        internal static bool RemoteTriggersEnabled
        {
            get => Options.RemoteTriggers.Enabled.Value;
            set => Options.RemoteTriggers.Enabled.Value = value;
        }

        internal static bool AutoBoostEnabled
        {
            get => Options.RemoteTriggers.AutoBoost.Enabled.Value;
            set => Options.RemoteTriggers.AutoBoost.Enabled.Value = value;
        }

        internal static bool AutoMergeEnabled
        {
            get => Options.RemoteTriggers.AutoMerge.Enabled.Value;
            set => Options.RemoteTriggers.AutoMerge.Enabled.Value = value;
        }

        internal static bool TossGoldEnabled
        {
            get => Options.RemoteTriggers.TossGold.Enabled.Value;
            set => Options.RemoteTriggers.TossGold.Enabled.Value = value;
        }

        internal static bool FightBossEnabled
        {
            get => Options.RemoteTriggers.FightBoss.Enabled.Value;
            set => Options.RemoteTriggers.FightBoss.Enabled.Value = value;
        }

        internal static bool KittyEnabled
        {
            get => Options.RemoteTriggers.Kitty.Enabled.Value;
            set => Options.RemoteTriggers.Kitty.Enabled.Value = value;
        }

        [HarmonyPrepare, HarmonyPatch]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                _settingsButton = GameObject.Find("Canvas/Player Panel Canvas/Player Panel/Settings").GetComponent<Button>();
                _settingsButton.image.color = RemoteTriggersEnabled ? Plugin.ButtonColor_Green : Color.white;

                _settingsButton.gameObject.AddComponent<ClickHandlerComponent>()
                    .OnRightClick(e => OpenConfig());

                InitPopup();
            };

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.GameHasStarted)
                    return;

                if (Input.GetKeyDown(KeyCode.F10))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        OpenConfig();
                    else
                        ToggleRemoteTriggersEnabled();
                }
            };
        }

        private static void ToggleRemoteTriggersEnabled()
        {
            RemoteTriggersEnabled = !RemoteTriggersEnabled;
            _settingsButton.image.color = RemoteTriggersEnabled ? Plugin.ButtonColor_Green : Color.white;
        }

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
            if (GUILayout.Button("enabled", AutoBoostEnabled ? _selected : _notSelected)) AutoBoostEnabled = true;
            if (GUILayout.Button("disabled", !AutoBoostEnabled ? _selected : _notSelected)) AutoBoostEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Auto Merge");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", AutoMergeEnabled ? _selected : _notSelected)) AutoMergeEnabled = true;
            if (GUILayout.Button("disabled", !AutoMergeEnabled ? _selected : _notSelected)) AutoMergeEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Toss Gold");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", TossGoldEnabled ? _selected : _notSelected)) TossGoldEnabled = true;
            if (GUILayout.Button("disabled", !TossGoldEnabled ? _selected : _notSelected)) TossGoldEnabled = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Fight Boss");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("enabled", FightBossEnabled ? _selected : _notSelected)) FightBossEnabled = true;
            if (GUILayout.Button("disabled", !FightBossEnabled ? _selected : _notSelected)) FightBossEnabled = false;
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
    }
}
