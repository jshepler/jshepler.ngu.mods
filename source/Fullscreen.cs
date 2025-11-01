using System;
using System.Collections;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEngine;

// some other resources haven't tried yet
// https://github.com/melak47/BorderlessWindow/blob/3b6978d88c0eef47f79c0ac125ec154bf701375c/BorderlessWindow/src/BorderlessWindow.cpp#L122
// https://stackoverflow.com/questions/75826970/winapi-making-a-window-fullscreen-borderless-from-another-unity-program-using
// https://gist.github.com/oktomus/7bdf92b3ccee221c3f19f6e9f75720c8

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Fullscreen
    {
        // modeled after https://forum.unity.com/threads/maximized-window-mode-launches-in-fullscreen.799620/#post-6973247
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // https://blog.magnusmontin.net/2014/11/30/disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-wpf-window/
        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongptra
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        private static IntPtr _windowHandle = IntPtr.Zero;
        private static bool _fullscreen = false;
        private static bool _maximized = false;

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Update")]
        private static void Character_Update_postfix(Character __instance)
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    __instance.StartCoroutine(toggleMaximized());
                else
                    __instance.StartCoroutine(toggleFullscreen());
            }
        }

        private static IEnumerator toggleFullscreen()
        {
            if (!_fullscreen && _maximized)
            {
                yield return toggleMaximized();
                yield return null;
                yield return null;
            }

            _fullscreen = !_fullscreen;
            Screen.fullScreenMode = _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        private static IEnumerator toggleMaximized()
        {
            if (!_maximized && _fullscreen)
            {
                yield return toggleFullscreen();
                yield return null;
                yield return null;
            }

            if (_windowHandle == IntPtr.Zero)
                GetWindowHandle();

            EnableMaximizeBox();

            _maximized = !_maximized;
            ShowWindow(_windowHandle, _maximized ? SW_MAXIMIZE : SW_RESTORE);
        }

        private static void GetWindowHandle()
        {
            var currentProcId = System.Diagnostics.Process.GetCurrentProcess().Id;
            int windowProcId = 0;

            EnumWindows((h, p) =>
            {
                GetWindowThreadProcessId(h, out windowProcId);
                if (windowProcId == currentProcId)
                {
                    _windowHandle = h;
                    return false;
                }

                return true;
            }, IntPtr.Zero);
        }

        private static void EnableMaximizeBox()
        {
            var current = GetWindowLongPtr(_windowHandle, GWL_STYLE);
            if (current == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                Plugin.LogInfo($"error calling GetWindowLongPtr: {error}");
                return;
            }

            var style = new IntPtr(current.ToInt64() | WS_MAXIMIZEBOX);
            var modified = SetWindowLongPtr(_windowHandle, GWL_STYLE, style);
            if (modified == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                Plugin.LogInfo($"error calling GetWindowLongPtr: {error}");
                return;
            }
        }

        internal static void SetWindowTitle(string title)
        {
            if (_windowHandle == IntPtr.Zero)
                GetWindowHandle();

            SetWindowText(_windowHandle, title);
        }
    }
}
