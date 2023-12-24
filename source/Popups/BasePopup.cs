using System;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.Popups
{
    internal abstract class BasePopup
    {
        const string ROOTCANVAS = "Canvas";
        private GameObject _blocker;

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

        internal BasePopup(Rect windowRect)
        {
            WindowRect = windowRect;
            _blocker = buildBLocker();
            IsOpen = false;

            Plugin.onGUI += (o, e) =>
            {
                if (!IsOpen)
                    return;

                if (Input.GetKeyDown(KeyCode.Escape))
                    Close();
                else
                    DrawWindow(WindowRect);
            };
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
                if (!WindowRect.Contains(Event.current.mousePosition))
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
