using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ToastNotifications
    {
        private static GameObject _baseTooltip;
        private static List<Toast> _pool = new();
        private static List<Toast> _active = new();
        private static Queue<ToastMessage> _overflow = new();
        private static float _xPos;
        private static float _scaleFactor;

        [HarmonyPostfix, HarmonyPatch(typeof(UIThemeController), "changeTheme")]
        private static void UIThemeController_changeTheme_postfix(int newID)
        {
            _scaleFactor = Plugin.Character.tooltip.canvas.scaleFactor;

            var rect = Traverse.Create(Plugin.Character.tooltip).Field<RectTransform>("tooltipRect").Value.rect;
            _xPos = Screen.width - (rect.width * _scaleFactor) - 5;

            _baseTooltip = GameObject.Instantiate(Plugin.Character.tooltip.tooltip, Plugin.Character.tooltip.tooltip.transform.parent);
            _baseTooltip.transform.position = new Vector3(_xPos, -1000);

            var script = _baseTooltip.GetComponent<HoverTooltip>();
            GameObject.DestroyImmediate(script);

            var image = _baseTooltip.transform.Find("Image");
            GameObject.DestroyImmediate(image.gameObject);

            _pool.Clear();
        }


        // The nuking bosses can generate MANY notications, slowing that frame down, and blocking things until they all go away.
        // I've tried a couple things to try and mitigate that, but didn't play out how I'd like, so instead going to ignore
        // those notifications.
        private static bool _killedBoss = false;

        [HarmonyPrefix, HarmonyPatch(typeof(BossController), "rewardExp")]
        private static void BossController_rewardExp_prefix()
        {
            _killedBoss = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BossController), "rewardExp")]
        private static void BossController_rewardExp_postfix()
        {
            _killedBoss = false;
        }

        // when splash screen is open (offline progress report when loading save, or when clicking build number),
        // it covers everything - including notifications; when that screen is open, toasts get buffered until
        // that screen is closed
        private static bool _splashScreenIsOpen;

        [HarmonyPostfix, HarmonyPatch(typeof(OfflineProgressSplashScreen), "openScreen")]
        private static void OfflineProgressSplashScreen_openScreen_postfix()
        {
            _splashScreenIsOpen = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OfflineProgressSplashScreen), "closeScreen")]
        private static void OfflineProgressSplashScreen_closeScreen_postfix()
        {
            _splashScreenIsOpen = false;
            Plugin.Character.StartCoroutine(CheckOverflow());
        }

        private static IEnumerator CheckOverflow()
        {
            yield return null;
            yield return null;

            while (_overflow.Count > 0)
            {
                var tm = _overflow.Dequeue();
                ShowNotification(tm);
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(HoverTooltip), "showOverrideTooltip", typeof(string), typeof(float))]
        private static bool HoverTooltip_showOverrideTooltip_prefix(string message, float seconds, HoverTooltip __instance)
        {
            if (__instance.character.settings.tutorialState >= 0)
                return true;

            __instance.tutorialKeys.gameObject.SetActive(value: false);
            __instance.log.AddEvent(message);

            ShowNotification(message, seconds);

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HoverTooltip), "showTooltip", typeof(string), typeof(float))]
        private static bool HoverTooltip_showTooltip_prefix(string message, float seconds, HoverTooltip __instance)
        {
            if (__instance.character.settings.tutorialState >= 0)
                return true;

            __instance.tutorialKeys.gameObject.SetActive(value: false);
            __instance.log.AddEvent(message);

            if (Plugin.Character.settings.timedTooltipsOn)
                ShowNotification(message, seconds);

            return false;
        }

        private static Coroutine ShowNotification(string message, float seconds)
        {
            return ShowNotification(new ToastMessage(message, seconds));
        }

        private static Coroutine ShowNotification(ToastMessage tm)
        {
            return Plugin.Character.StartCoroutine(ShowToast(tm));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "not a patch method, nor is tm modified")]
        private static IEnumerator ShowToast(ToastMessage tm)
        {
            // ignore nuking bosses - nuking more than 50 or so bosses has negative impact on UX, imo
            if (_killedBoss && Plugin.Character.bossController.nukeBoss)
                yield break;

            if (_splashScreenIsOpen)
            {
                _overflow.Enqueue(tm);
                yield break;
            }

            var toast = GetAvailableToast();
            toast.Text = tm.Message;
            toast.IsActive = true;

            // setting a GameObject active doesn't happen until next frame, and the ContentSizeFitter component won't do its thing until then
            yield return null;

            var pos = new Vector3(_xPos, Screen.height);
            if (_active.Count > 0)
                pos.y = _active.Last().Position.y;

            pos.y -= (toast.Height * _scaleFactor);
            toast.Position = pos;

            _active.Add(toast);
            yield return new WaitForSeconds(tm.Seconds);

            toast.IsActive = false;
            RemoveActive(toast);

            if (_overflow.Count > 0)
                yield return ShowNotification(_overflow.Dequeue());
        }

        private static void RemoveActive(Toast n)
        {
            var index = _active.IndexOf(n);
            if (index == -1)
                return;

            var height = n.Height * _scaleFactor;
            for (var x = index + 1; x < _active.Count; x++)
            {
                var p = _active[x].Position;
                p.y += height;
                _active[x].Position = p;
            }

            _active.RemoveAt(index);
            _pool.Add(n);
        }

        private static Toast GetAvailableToast()
        {
            if (_pool.Count > 0)
            {
                var n = _pool[0];
                _pool.RemoveAt(0);

                return n;
            }

            var clone = GameObject.Instantiate(_baseTooltip, _baseTooltip.transform.parent);
            return new Toast(clone);
        }
    }

    internal struct ToastMessage
    {
        internal string Message;
        internal float Seconds;

        public ToastMessage(string m, float s)
        {
            Message = m;
            Seconds = s;
        }
    }

    internal class Toast
    {
        private GameObject _gob;
        private Text _text;
        private RectTransform _rect;

        public Toast(GameObject gob)
        {
            _gob = gob;
            _text = _gob.GetComponentInChildren<Text>();
            _rect = _gob.GetComponent<RectTransform>();
        }

        internal float Width => _rect.rect.width;
        internal float Height => _rect.rect.height;

        internal string Text
        {
            get => _text.text;
            set => _text.text = value;
        }

        internal Vector3 Position
        {
            get => _gob.transform.position;
            set => _gob.transform.position = value;
        }

        internal bool IsActive
        {
            get => _gob.activeInHierarchy;
            set => _gob.SetActive(value);
        }
    }
}
