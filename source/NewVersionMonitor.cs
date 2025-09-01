using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using jshepler.ngu.mods.Popups;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class NewVersionMonitor
    {
        [HarmonyPatch, HarmonyPrepare]
        private static void prep(MethodInfo origin)
        {
            if (origin != null)
                return;

            Plugin.OnSaveLoaded += OnSaveLoaded;
            Plugin.onGUI += OnGUI;
        }

        private static Coroutine _checkForNewVersion;
        private static void OnSaveLoaded(object sender, EventArgs e)
        {
            if (Options.CheckForNewVersion.Enabled.Value == false)
                return;

            if (_checkForNewVersion == null)
                _checkForNewVersion = Plugin.Character.StartCoroutine(CheckForNewVersion());
        }

        private static IEnumerator CheckForNewVersion()
        {
            var delay = new WaitForSeconds(3600f);
            JSONNode data;

            while (true)
            {
                data = null;
                using (var req = UnityWebRequest.Get("https://api.github.com/repos/jshepler/jshepler.ngu.mods/releases/latest"))
                {
                    yield return req.SendWebRequest();

                    if (req.isNetworkError)
                        Plugin.LogInfo($"CheckForUpdate(): network error: {req.error}");

                    else if (req.isHttpError)
                        Plugin.LogInfo($"CheckForUpdate(): http error: {req.responseCode}");

                    else
                        data = JSONNode.Parse(req.downloadHandler.text);
                }

                if (data != null && data.HasKey("tag_name"))
                {
                    var version = data["tag_name"].Value;
                    if (!string.IsNullOrWhiteSpace(version)
                        && version != PluginInfo.PLUGIN_VERSION
                        && version != Options.CheckForNewVersion.Skipped.Value)
                    {
                        _version = version;
                    }
                }

                yield return delay;
            }
        }

        private static void Init()
        {
            _area = new Rect(20, 5, 300, 30);

            _areaStyle = new GUIStyle("box");
            _areaStyle.normal.background = Popup.CreateSolidColorTexture(_area, Color.cyan);

            _labelStyle = new GUIStyle("label");
            _labelStyle.normal.textColor = Color.blue;
            //_labelStyle.fontSize = 12;
        }

        private static string _version;
        private static Rect _area;
        private static GUIStyle _areaStyle;
        private static GUIStyle _labelStyle;

        private static void OnGUI(object sender, EventArgs e)
        {
            if (_version == null)
                return;

            if (_areaStyle == null)
                Init();

            UIScaler.Begin();

            GUILayout.BeginArea(_area, _areaStyle);
            GUILayout.BeginHorizontal();

            GUILayout.Label($"jshepler mods {_version} available", _labelStyle);
            GUILayout.FlexibleSpace();

            if(GUILayout.Button("download", GUILayout.ExpandHeight(true)))
                Application.OpenURL("https://github.com/jshepler/jshepler.ngu.mods/releases/latest");

            if (GUILayout.Button("skip", GUILayout.ExpandHeight(true)))
            {
                Options.CheckForNewVersion.Skipped.Value = _version;
                _version = null;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            UIScaler.End();
        }
    }
}
