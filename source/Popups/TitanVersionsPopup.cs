using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class TitanVersionsPopup : BasePopup
    {
        private Character _character;
        
        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _selected;
        private static GUIStyle _notSelected;

        private static Dictionary<int, string> _names = new()
        {
            { 6, "The Beast" },
            { 7, "Greasy Nerd" },
            { 8, "The Godmother" },
            { 9, "The Exile" },
            { 10, "IT HUNGERS" },
            { 11, "Rock Lobster" },
            { 12, "AMALGAMATE" }
        };

        private Dictionary<int, int> _titanVersions = new();

        internal TitanVersionsPopup(Character c)
            : base(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 114, 400, 260))
        {
            _character = c;
        }

        internal override void Open()
        {
            _titanVersions.Clear();
            for (var x = 6; x < 13; x++)
                _titanVersions.Add(x, GetTitanVersion(x));

            base.Open();
        }

        protected override void DrawWindow(Rect windowRect)
        {
            // these must only happen during OnGUI for some reason
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(50, 50, 50, 255));

                _titleStyle = new GUIStyle("label");
                _titleStyle.alignment = TextAnchor.MiddleCenter;

                _notSelected = new GUIStyle("button");
                _notSelected.normal.background = Texture2D.blackTexture;
                //_notSelected.hover.background = Texture2D.redTexture;

                _selected = new GUIStyle("button");
                _selected.normal.background = Texture2D.whiteTexture;
                _selected.normal.textColor = Color.black;
                _selected.hover.background = Texture2D.whiteTexture;
                _selected.hover.textColor = Color.black;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Change Titan Versions</b>", _titleStyle);
            if (GUILayout.Button("×", GUILayout.Width(25))) Close();
            GUILayout.EndHorizontal();

            foreach (var titan in _titanVersions.Keys.ToList())
            {
                DrawTitan(titan, _titanVersions[titan]);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTitan(int titan, int version)
        {
            GUILayout.BeginHorizontal("box");

            GUILayout.Label(_names[titan]);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Easy", version == 0 ? _selected : _notSelected)) SetTitanVersion(titan, 0);
            if (GUILayout.Button("Normal", version == 1 ? _selected : _notSelected)) SetTitanVersion(titan, 1);
            if (GUILayout.Button("Hard", version == 2 ? _selected : _notSelected)) SetTitanVersion(titan, 2);
            if (GUILayout.Button("Brutal", version == 3 ? _selected : _notSelected)) SetTitanVersion(titan, 3);

            GUILayout.EndHorizontal();
        }

        private bool IsTitanUnlocked(int titan)
        {
            return titan switch
            {
                6 => _character.adventure.titan6Unlocked,
                7 => _character.adventure.titan7Unlocked,
                8 => _character.adventure.titan8Unlocked,
                9 => _character.adventure.titan9Unlocked,
                10 => _character.adventure.titan10Unlocked,
                11 => _character.adventure.titan11Unlocked,
                12 => _character.adventure.titan12Unlocked,
                _ => false
            };
        }

        private int GetTitanVersion(int titan)
        {
            return titan switch
            {
                6 => _character.adventure.titan6Version,
                7 => _character.adventure.titan7Version,
                8 => _character.adventure.titan8Version,
                9 => _character.adventure.titan9Version,
                10 => _character.adventure.titan10Version,
                11 => _character.adventure.titan11Version,
                12 => _character.adventure.titan12Version,
                _ => 0
            };
        }

        private void SetTitanVersion(int titan, int version)
        {
            _titanVersions[titan] = version;

            _ = titan switch
            {
                6 => _character.adventure.titan6Version = version,
                7 => _character.adventure.titan7Version = version,
                8 => _character.adventure.titan8Version = version,
                9 => _character.adventure.titan9Version = version,
                10 => _character.adventure.titan10Version = version,
                11 => _character.adventure.titan11Version = version,
                12 => _character.adventure.titan12Version = version,
                _ => 0
            };
        }
    }
}
