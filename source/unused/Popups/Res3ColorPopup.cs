using System.Globalization;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class Res3ColorPopup : BasePopupWithReturn<Color?>
    {
        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _selected;
        private static GUIStyle _notSelected;

        private static bool _setFocus;
        private static bool _hexMode = true;

        private static string _r;
        private static string _g;
        private static string _b;

        public Res3ColorPopup() : base(0, 0, 230, 100)
        {
            Plugin.OnUpdate += (o, e) =>
            {
                if (!IsOpen || !Plugin.Character.InMenu(Menu.Settings_Page2) || !Event.current.isKey)
                    return;

                // if the text field has focus, the input system is ignored;
                // use the event system instead
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        SetColorAndClose();
                        break;

                    case KeyCode.Escape:
                        Close();
                        break;
                }

                return;
            };
        }

        internal void OpenAt(float x, float y)
        {
            OpenAt(new Vector2(x, y));
        }

        internal void OpenAt(Vector2 pos)
        {
            InitRGB();
            _setFocus = true;

            pos.y = Screen.height - pos.y - base.WindowRect.height;
            base.WindowRect.position = pos;

            base.Open();
        }

        protected override void DrawWindow(Rect windowRect)
        {
            // these must be done during OnGUI, in which DrawWindow is called
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

                _titleStyle = new GUIStyle("label");
                _titleStyle.alignment = TextAnchor.MiddleCenter;

                _selected = new GUIStyle("button");
                _selected.normal.background = Texture2D.whiteTexture;
                _selected.normal.textColor = Color.black;
                _selected.hover.background = Texture2D.whiteTexture;
                _selected.hover.textColor = Color.black;

                _notSelected = new GUIStyle("button");
                _notSelected.normal.background = Texture2D.blackTexture;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Res3 Color RGB Input", _titleStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("HEX", _hexMode ? _selected : _notSelected))
                ChangeHexMode(true);
            if (GUILayout.Button("DEC", _hexMode ? _notSelected : _selected))
                ChangeHexMode(false);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");

            GUILayout.Label("R:");
            GUI.SetNextControlName("inputR");
            _r = GUILayout.TextField(_r, GUILayout.Width(30f));

            if (_setFocus)
            {
                GUI.FocusControl("inputR");
                _setFocus = false;
            }

            GUILayout.Label(" G:");
            _g = GUILayout.TextField(_g, GUILayout.Width(30f));

            GUILayout.Label(" B:");
            _b = GUILayout.TextField(_b, GUILayout.Width(30f));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK"))
                SetColorAndClose();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        internal void SetColorAndClose()
        {
            Close(ColorFromRGB());
        }

        private Color ColorFromRGB()
        {
            var r = _hexMode ? byte.Parse(_r, NumberStyles.HexNumber) : byte.Parse(_r);
            var g = _hexMode ? byte.Parse(_g, NumberStyles.HexNumber) : byte.Parse(_g);
            var b = _hexMode ? byte.Parse(_b, NumberStyles.HexNumber) : byte.Parse(_b);

            return new Color32(r, g, b, 255);
        }

        private void InitRGB()
        {
            var res3 = Plugin.Character.res3;

            // Color rgb values are floats, the game uses floats
            // Color32 rgb values are bytes, which are easier to convert to hex strings
            Color32 color = new Color(res3.res3R, res3.res3G, res3.res3B);

            _r = _hexMode ? color.r.ToString("X2") : color.r.ToString();
            _g = _hexMode ? color.g.ToString("X2") : color.g.ToString();
            _b = _hexMode ? color.b.ToString("X2") : color.b.ToString();
        }

        private void ChangeHexMode(bool hexMode)
        {
            var r = _hexMode ? byte.Parse(_r, NumberStyles.HexNumber) : byte.Parse(_r);
            var g = _hexMode ? byte.Parse(_g, NumberStyles.HexNumber) : byte.Parse(_g);
            var b = _hexMode ? byte.Parse(_b, NumberStyles.HexNumber) : byte.Parse(_b);

            _r = hexMode ? r.ToString("X2") : r.ToString();
            _g = hexMode ? g.ToString("X2") : g.ToString();
            _b = hexMode ? b.ToString("X2") : b.ToString();

            _hexMode = hexMode;
        }
    }
}
