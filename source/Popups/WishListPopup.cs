using System.Collections.Generic;
using System.IO;
using jshepler.ngu.mods.ModSave;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class WishListPopup : BasePopup
    {
        const float RECT_WIDTH = 660f;
        const float RECT_HEIGHT = 340f;

        private static List<int> _wishes => Data.WishList;
        private static List<int> _targets => Data.WishTargets;
        private static List<int> _lastRunning => Data.WishesLastRunning;

        private static bool _enabled
        {
            get => Options.WishList.Enabled.Value;
            set => Options.WishList.Enabled.Value = value;
        }

        private static bool _singleLevelMode
        {
            get => Options.WishList.SingleLevelMode.Value;
            set => Options.WishList.SingleLevelMode.Value = value;
        }

        private static bool _blacklistToggle;
        private static bool _blacklistMode
        {
            get => Options.WishList.BlacklistMode.Value;
            set => Options.WishList.BlacklistMode.Value = value;
        }

        private static bool _autoAdvance
        {
            get => Options.WishList.AutoAdvance.Value;
            set => Options.WishList.AutoAdvance.Value = value;
        }

        //internal WishListPopup() : base(GetCenteredRect(RECT_WIDTH, RECT_HEIGHT))
        //{
        //}

        //protected override void UpdateRect()
        //{
        //    base.UpdateRectCentered(RECT_WIDTH, RECT_HEIGHT);
        //}

        internal WishListPopup() : base(RECT_WIDTH, RECT_HEIGHT) { }

        internal override void Open()
        {
            _blacklistToggle = _blacklistMode;
            base.Open();
        }

        private static GUIStyle _windowStyle;
        private static GUIStyle _activeWishStyle;
        private static GUIStyle _trackedWishStyle;
        private static Vector2 _scrollView;
        private static void initStyles(Rect windowRect)
        {
            _windowStyle = new GUIStyle("box");
            _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

            _activeWishStyle = new GUIStyle("label");
            _activeWishStyle.normal.textColor = Color.green;

            _trackedWishStyle = new GUIStyle("label");
            _trackedWishStyle.normal.textColor = Color.yellow;
        }

        private static Texture2D _arrow_up;
        private static Texture2D _arrow_top;
        private static Texture2D _arrow_down;
        private static Texture2D _arrow_bottom;
        internal static void loadImages()
        {
            var au = Resources.arrow_up;
            using (var ms = new MemoryStream())
            {
                au.Save(ms, au.RawFormat);
                _arrow_up = new Texture2D(au.Width, au.Height);
                _arrow_up.LoadImage(ms.ToArray());
            }

            var at = Resources.arrow_top;
            using (var ms = new MemoryStream())
            {
                at.Save(ms, at.RawFormat);
                _arrow_top = new Texture2D(at.Width, at.Height);
                _arrow_top.LoadImage(ms.ToArray());
            }

            var ad = Resources.arrow_down;
            using (var ms = new MemoryStream())
            {
                ad.Save(ms, ad.RawFormat);
                _arrow_down = new Texture2D(ad.Width, ad.Height);
                _arrow_down.LoadImage(ms.ToArray());
            }

            var ab = Resources.arrow_bottom;
            using (var ms = new MemoryStream())
            {
                ab.Save(ms, ab.RawFormat);
                _arrow_bottom = new Texture2D(ab.Width, ab.Height);
                _arrow_bottom.LoadImage(ms.ToArray());
            }
        }

        protected override void DrawWindow(Rect windowRect)
        {
            // dunno why, but styles can only be created inside OnGUI (this method is called from base.OnGUI)
            if (_windowStyle == null)
                initStyles(windowRect);

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            DrawTitleBar();
            DrawQueue();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTitleBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label("WISH LIST");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("×"))
                Close();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            _enabled = GUILayout.Toggle(_enabled, "Enabled   ");
            _autoAdvance = GUILayout.Toggle(_autoAdvance, "Auto Advance   ");
            _singleLevelMode = GUILayout.Toggle(_singleLevelMode, "Single Level   ");
            _blacklistToggle = GUILayout.Toggle(_blacklistToggle, "Blacklist");

            if (_blacklistMode != _blacklistToggle)
            {
                _blacklistMode = _blacklistToggle;
                Plugin.Character.wishesController.updateAllPods();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("start"))
                WishList.FillOpenWishSlots();

            if (GUILayout.Button("resume"))
                WishList.ResumeWishes();

            if (GUILayout.Button("clear"))
                Clear();

            GUILayout.EndHorizontal();
        }

        private void DrawQueue()
        {
            _scrollView = GUILayout.BeginScrollView(_scrollView, false, false, GUILayout.ExpandHeight(true));

            for (var x = 0; x < _wishes.Count; x++)
                DrawSlot(x);

            GUILayout.EndScrollView();
        }

        private void DrawSlot(int index)
        {
            var wishId = _wishes[index];
            var target = _targets[index];
            var wish = Wishes.AllWishes[wishId];

            GUILayout.BeginHorizontal("box");

            if (wish.IsRunning)
            {
                GUILayout.Label($"({wishId})", _activeWishStyle, GUILayout.Width(30));
                GUILayout.Label(wish.Name, _activeWishStyle);
            }
            else if (_lastRunning.Contains(wishId))
            {
                GUILayout.Label($"({wishId})", _trackedWishStyle, GUILayout.Width(30));
                GUILayout.Label(wish.Name, _trackedWishStyle);
            }
            else
            {
                GUILayout.Label($"({wishId})", GUILayout.Width(30));
                GUILayout.Label(wish.Name);
            }

            GUILayout.FlexibleSpace();

            if (!_blacklistMode)
            {
                GUILayout.Label($"{wish.Level}/{wish.MaxLevel}");

                if (int.TryParse(GUILayout.TextField(target.ToString(), GUILayout.Width(30)), out target))
                    _targets[index] = target;

                if (GUILayout.Button(_arrow_top))
                    MoveTop(index);

                if (GUILayout.Button(_arrow_up))
                    MoveUp(index);

                if (GUILayout.Button(_arrow_down))
                    MoveDown(index);

                if (GUILayout.Button(_arrow_bottom))
                    MoveBottom(index);
            }

            if (GUILayout.Button("×"))
                RemoveAt(index);

            GUILayout.EndHorizontal();
        }

        private void Clear()
        {
            _wishes.Clear();
            _targets.Clear();
        }

        private void MoveTop(int index)
        {
            var v = _wishes[index];
            _wishes.RemoveAt(index);
            _wishes.Insert(0, v);

            var t = _targets[index];
            _targets.RemoveAt(index);
            _targets.Insert(0, t);
        }

        private void MoveUp(int index)
        {
            var v = _wishes[index];
            _wishes.RemoveAt(index);
            _wishes.Insert(index - 1, v);

            var t = _targets[index];
            _targets.RemoveAt(index);
            _targets.Insert(index - 1, t);
        }

        private void MoveDown(int index)
        {
            var v = _wishes[index];
            _wishes.RemoveAt(index);
            _wishes.Insert(index + 1, v);

            var t = _targets[index];
            _targets.RemoveAt(index);
            _targets.Insert(index + 1, t);
        }

        internal void MoveBottom(int index)
        {
            var v = _wishes[index];
            _wishes.RemoveAt(index);
            _wishes.Add(v);

            var t = _targets[index];
            _targets.RemoveAt(index);
            _targets.Add(t);
        }

        private void RemoveAt(int index)
        {
            _wishes.RemoveAt(index);
            _targets.RemoveAt(index);
        }
    }
}
