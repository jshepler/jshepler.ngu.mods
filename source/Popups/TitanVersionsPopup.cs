using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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

        private static List<Titan> _titans = new()
        {
            new Titan(6, "The Beast"),
            new Titan(7, "Greasy Nerd"),
            new Titan(8, "The Godmother"),
            new Titan(9, "The Exile"),
            new Titan(10, "IT HUNGERS"),
            new Titan(11, "Rock Lobster"),
            new Titan(12, "AMALGAMATE"),
        };

        private static List<Titan> _killedTitans;

        internal TitanVersionsPopup(Character c)
            : base(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 114, 400, 260))
        {
            _character = c;
        }

        internal override void Open()
        {
            _killedTitans = _titans.Where(t => t.HasKilled()).ToList();
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

            _killedTitans.Do(DrawTitan);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTitan(Titan titan)
        {
            GUILayout.BeginHorizontal("box");

            GUILayout.Label(titan.name);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Easy", titan.version == 0 ? _selected : _notSelected))
                titan.version = 0;

            if (GUILayout.Button("Normal", titan.version == 1 ? _selected : _notSelected))
                titan.version = 1;

            if (GUILayout.Button("Hard", titan.version == 2 ? _selected : _notSelected))
                titan.version = 2;

            if (GUILayout.Button("Brutal", titan.version == 3 ? _selected : _notSelected))
                titan.version = 3;

            GUILayout.EndHorizontal();
        }

        private class Titan
        {
            private int id;

            internal string name;

            internal Titan(int id, string name)
            {
                this.id = id;
                this.name = name;
            }

            internal int version
            {
                get
                {
                    return id switch
                    {
                        6 => Plugin.Character.adventure.titan6Version,
                        7 => Plugin.Character.adventure.titan7Version,
                        8 => Plugin.Character.adventure.titan8Version,
                        9 => Plugin.Character.adventure.titan9Version,
                        10 => Plugin.Character.adventure.titan10Version,
                        11 => Plugin.Character.adventure.titan11Version,
                        12 => Plugin.Character.adventure.titan12Version,
                        _ => 0
                    };
                }

                set
                {
                    _ = id switch
                    {
                        6 => Plugin.Character.adventure.titan6Version = value,
                        7 => Plugin.Character.adventure.titan7Version = value,
                        8 => Plugin.Character.adventure.titan8Version = value,
                        9 => Plugin.Character.adventure.titan9Version = value,
                        10 => Plugin.Character.adventure.titan10Version = value,
                        11 => Plugin.Character.adventure.titan11Version = value,
                        12 => Plugin.Character.adventure.titan12Version = value,
                        _ => 0
                    };
                }
            }

            internal bool HasKilled()
            {
                return id switch
                {
                    6 => Plugin.Character.adventure.boss6Kills > 0,
                    7 => Plugin.Character.adventure.boss7Kills > 0,
                    8 => Plugin.Character.adventure.boss8Kills > 0,
                    9 => Plugin.Character.adventure.boss9Kills > 0,
                    10 => Plugin.Character.adventure.boss10Kills > 0,
                    11 => Plugin.Character.adventure.boss11Kills > 0,
                    12 => Plugin.Character.adventure.boss12Kills > 0,
                    _ => false
                };
            }
        }
    }
}
