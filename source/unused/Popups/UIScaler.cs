using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.Popups
{
    // from ChatGPT
    internal static class UIScaler
    {
        internal static event Action OnScaleChanged;

        private static Matrix4x4? originalMatrix = null;
        private static Dictionary<GUIStyle, int> originalFontSizes = null;

        private static float cachedScale = -1f;
        private static int lastScreenWidth = 0;
        private static int lastScreenHeight = 0;
        private static float lastDPI = 0f;
        private static bool initialized = false;

        public static float CurrentScale(bool applyOptionalScaleMultiplier = true)
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 96f;

            if (cachedScale < 0f
                || Screen.width != lastScreenWidth
                || Screen.height != lastScreenHeight
                || Mathf.Abs(dpi - lastDPI) > 0.1f)
            {
                cachedScale = DetectUIScale();
                if (applyOptionalScaleMultiplier)
                    cachedScale *= Options.Experimental.ModPopupScaling.Value;

                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                lastDPI = dpi;

                if (initialized)
                    OnScaleChanged?.Invoke();
                else
                    initialized = true;
            }

            return cachedScale;
        }

        public static void Begin()
        {
            if (originalMatrix == null)
                originalMatrix = GUI.matrix;

            var scale = CurrentScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            if (originalFontSizes == null)
            {
                originalFontSizes = new Dictionary<GUIStyle, int>();
                foreach (GUIStyle style in GUI.skin)
                    originalFontSizes[style] = style.fontSize;
            }

            foreach (GUIStyle style in GUI.skin)
                if (style.fontSize > 0)
                    style.fontSize = Mathf.RoundToInt(style.fontSize * scale);
        }

        public static void End()
        {
            if (originalMatrix.HasValue)
                GUI.matrix = originalMatrix.Value;

            if (originalFontSizes != null)
                foreach (var kvp in originalFontSizes)
                    kvp.Key.fontSize = kvp.Value;
        }

        private static float DetectUIScale()
        {
            CanvasScaler[] scalers = GameObject.FindObjectsOfType<CanvasScaler>();
            CanvasScaler scaler = scalers != null && scalers.Length > 0
                ? scalers.OrderByDescending(s => s.GetComponent<Canvas>().scaleFactor).FirstOrDefault()
                : null;

            float scale = 1f;
            float dpi = Screen.dpi > 0 ? Screen.dpi : 96f;

            if (scaler != null)
            {
                scale = scaler.GetComponent<Canvas>().scaleFactor;

                // Apply DPI if the CanvasScaler is not already DPI-aware
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize)
                    scale *= dpi / 96f;
            }
            else
            {
                // No canvas → assume game is not DPI-aware, scale by Windows DPI
                scale = dpi / 96f;
            }

            return scale;
        }
    }
}