using System;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.Popups
{
    internal abstract class BasePopup
    {
        const string ROOTCANVAS = "Canvas";
        private GameObject _blocker;

        private Rect designRect;
        private bool isCentered = false;
        private bool needsRecalc = true;
        protected Rect WindowRect;

        private bool _isOpen = false;
        internal protected bool IsOpen
        {
            get => _isOpen;
            protected set
            {
                _isOpen = value;
                _blocker.SetActive(value);
            }
        }

#pragma warning disable CS0649 // Field 'BasePopup.Closed' is never assigned to, and will always have its default value null
        internal EventHandler Closed;
#pragma warning restore CS0649 // Field 'BasePopup.Closed' is never assigned to, and will always have its default value null

        internal BasePopup(float width, float height)
        {
            _blocker = buildBLocker();
            IsOpen = false;

            designRect = new Rect(0, 0, width, height);
            isCentered = true;

            Plugin.onGUI += OnGUI;
            UIScaler.OnScaleChanged += () => needsRecalc = true;
        }

        internal BasePopup(float x, float y, float width, float height)
        {
            _blocker = buildBLocker();
            IsOpen = false;

            designRect = new Rect(x, y, width, height);
            isCentered = false;

            Plugin.onGUI += OnGUI;
            UIScaler.OnScaleChanged += () => needsRecalc = true;
        }

        protected void RecalcRect()
        {
            if (!needsRecalc)
                return;

            var scale = UIScaler.CurrentScale();
            var screenWidthDesign = Screen.width / scale;
            var screenHeightDesign = Screen.height / scale;

            WindowRect = designRect;

            if (isCentered)
            {
                WindowRect.x = (screenWidthDesign - designRect.width) / 2f;
                WindowRect.y = (screenHeightDesign - designRect.height) / 2f;
            }
            else
            {
                var additionalScaling = Options.Experimental.ModPopupScaling.Value;
                var x = WindowRect.x / additionalScaling;
                var y = WindowRect.y / additionalScaling;

                WindowRect.x = Mathf.Clamp(x, 0, (screenWidthDesign - designRect.width));
                WindowRect.y = Mathf.Clamp(y, 0, (screenHeightDesign - designRect.height));
            }

                needsRecalc = false;
        }

        internal static Rect MakeCenteredRect(float width, float height)
        {
            var scale = UIScaler.CurrentScale();

            var x = (Screen.width / scale - width) / 2f;
            var y = (Screen.height / scale - height) / 2f;

            return new Rect(x, y, width, height);
        }

        protected void MoveTo(Vector2 screenPosition)
        {
            designRect.position = screenPosition / UIScaler.CurrentScale();
            needsRecalc = true;
        }

        private void OnGUI(object sender, EventArgs e)
        {
            if (!IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            RecalcRect();
            UIScaler.Begin();
            DrawWindow(WindowRect);
            UIScaler.End();
        }

        protected abstract void DrawWindow(Rect windowRect);

        internal virtual void Open()
        {
            IsOpen = true;
        }

        internal virtual void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        internal virtual void Close()
        {
            IsOpen = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        // modeled after https://github.com/AppertaFoundation/IXN_IBM_MIND/blob/8aa21d78bfe90e6feeab3cce6256e7fbb6036734/Whack-A-Mole/Library/PackageCache/com.unity.textmeshpro%402.0.0/Scripts/Runtime/TMP_Dropdown.cs#L847
        private GameObject buildBLocker()
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
                var unscaledMousePosition = Event.current.mousePosition / UIScaler.CurrentScale();
                if (!WindowRect.Contains(unscaledMousePosition))
                    Close();
            });

            return blocker;
        }
    }

    internal static class Popup
    {
        internal static Texture2D CreateSolidColorTexture(Rect rect, Color color)
        {
            return CreateSolidColorTexture(rect.size, color);
        }

        internal static Texture2D CreateSolidColorTexture(Vector2 size, Color color)
        {
            return CreateSolidColorTexture((int)size.x, (int)size.y, color);
        }

        internal static Texture2D CreateSolidColorTexture(int width, int height, Color color)
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
