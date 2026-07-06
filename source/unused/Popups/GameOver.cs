using System;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.Popups
{
    internal class GameOver
    {
        const string ROOTCANVAS = "Canvas";
        private static GameObject _blocker;
        private static bool _open = false;
        private static Rect _windowRect;
        private static GUIStyle _windowStyle;
        private static GUIStyle _gameOverStyle;

        static GameOver()
        {
            _blocker = buildBLocker();
            Plugin.onGUI += OnGUI;
        }

        internal static void Show()
        {
            _open = true;
            _blocker.SetActive(true);
            _windowRect = BasePopup.MakeCenteredRect(600, 100);
        }

        internal static void Toggle()
        {
            if (_open)
            {
                _open = false;
                _blocker.SetActive(false);
            }
            else
                Show();
        }

        private static void OnGUI(object sender, EventArgs e)
        {
            if (!_open)
                return;

            UIScaler.Begin();

            // these must only happen during OnGUI for some reason
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = CreateSolidColorTexture(_windowRect, new Color32(50, 50, 50, 0));

                _gameOverStyle = new GUIStyle("label");
                _gameOverStyle.fontSize = 72;
                _gameOverStyle.normal.textColor = Color.red;
                _gameOverStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUILayout.BeginArea(_windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.Label("<b>GAME OVER</b>", _gameOverStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            UIScaler.End();
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
            blockerImage.color = new Color(0, 0, 0, .6f);

            return blocker;
        }

        private static Texture2D CreateSolidColorTexture(Rect rect, Color color)
        {
            return CreateSolidColorTexture(rect.size, color);
        }

        private static Texture2D CreateSolidColorTexture(Vector2 size, Color color)
        {
            return CreateSolidColorTexture((int)size.x, (int)size.y, color);
        }

        private static Texture2D CreateSolidColorTexture(int width, int height, Color color)
        {
            var image = new Texture2D((int)width + 1, (int)height + 1, TextureFormat.ARGB32, false);

            for (var x = 0; x < image.width; x++)
                for (var y = 0; y < image.height; y++)
                    image.SetPixel(x, y, color);

            image.Apply();

            return image;
        }
    }
}
