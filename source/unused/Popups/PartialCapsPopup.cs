using System.Collections.Generic;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class PartialCapsPopup : BasePopupWithReturn<long?>
    {
        private static GUIStyle _windowStyle;
        private static GUIStyle _buttonStyle;
        private List<long> _caps;

        internal PartialCapsPopup() : base(0, 0, 150, 250)
        {
        }

        internal void OpenAt(float x, float y, List<long> caps)
        {
            OpenAt(new Vector2(x, y), caps);
        }

        internal void OpenAt(Vector2 pos, List<long> caps)
        {
            _caps = caps;

            var additionalScaling = Options.Experimental.ModPopupScaling.Value;
            pos.x = pos.x * additionalScaling + 30f;
            pos.y = pos.y * additionalScaling;

            base.MoveTo(pos);
            base.Open();
        }

        protected override void DrawWindow(Rect windowRect)
        {
            // these must be done during OnGUI, in which DrawWindow is called
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

                _buttonStyle = new GUIStyle("button");
                //_buttonStyle.padding.top = 10;
                _buttonStyle.margin.top = 6;
                _buttonStyle.margin.bottom = 6;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            for (var x = 0; x < _caps.Count; x++)
                if (GUILayout.Button($"{_caps[x]:e3} ({100 / (x + 2)}%)", _buttonStyle))
                    Close(_caps[x]);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
