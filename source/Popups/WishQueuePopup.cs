using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class WishQueuePopup : BasePopup
    {
        private Character _character;

        private static GUIStyle _windowStyle;
        private static GUIStyle _labelStyle_VertCentered;
        private static Vector2 _scrollView;

        internal WishQueuePopup(Character c)
            : base(new Rect(Screen.width * .375f, Screen.height * .2f, Screen.width * .35f, Screen.height * .68f))
        {
            _character = c;
        }

        protected override void DrawWindow(Rect windowRect)
        {
            // these can only be set during OnGUI
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

                _labelStyle_VertCentered = new GUIStyle("label");
                _labelStyle_VertCentered.alignment = TextAnchor.MiddleLeft;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Wish Queue");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("clear")) WishQueue.Queue.Clear();
            if (GUILayout.Button("×")) Close();
            GUILayout.EndHorizontal();

            _scrollView = GUILayout.BeginScrollView(_scrollView, false, true, GUILayout.ExpandHeight(true));
            for (var x = 0; x < WishQueue.Queue.Count; x++)
            {
                DrawSlot(x);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSlot(int index)
        {
            var wishId = WishQueue.Queue[index];
            var wish = _character.wishes.wishes[wishId];

            GUILayout.BeginHorizontal("box");

            GUILayout.Label($"({wishId})", _labelStyle_VertCentered, GUILayout.Width(30));
            GUILayout.Label(_character.wishesController.properties[wishId].wishName);

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("<<"))
                MoveTop(index);

            if (GUILayout.Button("<"))
                MoveUp(index);

            if (GUILayout.Button(">"))
                MoveDown(index);

            if (GUILayout.Button(">>"))
                MoveBottom(index);

            if (GUILayout.Button("×"))
                WishQueue.Queue.RemoveAt(index);

            GUILayout.EndHorizontal();
        }

        private void MoveTop(int index)
        {
            var v = WishQueue.Queue[index];
            WishQueue.Queue.RemoveAt(index);
            WishQueue.Queue.Insert(0, v);
        }

        private void MoveUp(int index)
        {
            var v = WishQueue.Queue[index];
            WishQueue.Queue.RemoveAt(index);
            WishQueue.Queue.Insert(index - 1, v);
        }

        private void MoveDown(int index)
        {
            var v = WishQueue.Queue[index];
            WishQueue.Queue.RemoveAt(index);
            WishQueue.Queue.Insert(index + 1, v);
        }

        private void MoveBottom(int index)
        {
            var v = WishQueue.Queue[index];
            WishQueue.Queue.RemoveAt(index);
            WishQueue.Queue.Add(v);
        }
    }
}
