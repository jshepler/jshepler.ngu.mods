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
        private static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

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
                yield return new WaitForFixedUpdate();
            }

            _fullscreen = !_fullscreen;
            Screen.fullScreenMode = _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        private static IEnumerator toggleMaximized()
        {
            if (!_maximized && _fullscreen)
            {
                yield return toggleFullscreen();
                yield return new WaitForFixedUpdate();
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
            if (current == 0)
            {
                var error = Marshal.GetLastWin32Error();
                Plugin.LogInfo($"error calling GetWindowLongPtr: {error}");
                return;
            }

            var modified = SetWindowLongPtr(_windowHandle, GWL_STYLE, (current | WS_MAXIMIZEBOX));
            if (modified == 0)
            {
                var error = Marshal.GetLastWin32Error();
                Plugin.LogInfo($"error calling GetWindowLongPtr: {error}");
                return;
            }
        }
    }
}
