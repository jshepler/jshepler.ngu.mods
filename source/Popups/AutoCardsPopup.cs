using System;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class AutoCardsPopup : BasePopup
    {
        const int WIDTH = 670;
        const int HEIGHT = 295;//346;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _maxYeetEffTextStyle;
        private static GUIStyle _disabledYeetRarities;

        private static string[] _disabledEnabled = ["Disabled", "Enabled"];
        private static string[] _sortBy = ["Rarity", "Type", "Efficiency", "Variance"];
        private static string[] _sortDirections = ["Ascending", "Descending"];
        private static string[] _autoYeetModes = ["Disabled", "Efficency", "Variance", "Rarity"];
        private static string[] _rarities = ["Crappy", "Bad", "Meh", "Okay", "Good", "Great", "Hot Damn"];

        private static string[] _bonuses = ["END", "E-NGU", "M-NGU", "WAND", "AUGS", "TM", "HACKS", "WISHES", "A/D", "ADV", "DC", "GOLD", "DAYCR", "PP", "QP"];
        private static bool[] _alwaysYeet = new bool[15];
        private static bool[] _prevAlwaysYeet = new bool[15];

        private static CardSortBy _lastSortBy;
        private static CardSortDirection _lastSortDirection;

        private static bool AutoSortEnabled
        {
            get => Options.Cards.AutoSortEnabled.Value;
            set => Options.Cards.AutoSortEnabled.Value = value;
        }

        private static CardSortBy AutoSortBy
        {
            get => Options.Cards.AutoSortBy.Value;
            set => Options.Cards.AutoSortBy.Value = value;
        }

        private static CardSortDirection AutoSortDirection
        {
            get => Options.Cards.AutoSortDirection.Value;
            set => Options.Cards.AutoSortDirection.Value = value;
        }

        private static CardYeetMode AutoYeetMode
        {
            get => Options.Cards.AutoYeetMode.Value;
            set => Options.Cards.AutoYeetMode.Value = value;
        }

        private static rarity MaxYeetRarity
        {
            get => Options.Cards.MaxYeetRarity.Value;
            set => Options.Cards.MaxYeetRarity.Value = value;
        }

        private static float MaxYeetEfficiency
        {
            get => Options.Cards.MaxYeetEfficiency.Value;
            set => Options.Cards.MaxYeetEfficiency.Value = value;
        }

        private static string _efficiencyMulti2ModifierString => $"{MaxYeetEfficiency * 100f:0.##}";
        private static string _maxYeetEfficiency;
        //{
        //    get => $"{MaxYeetEfficiency * 100f:0.##}";
        //    set
        //    {
        //        if (float.TryParse(value, out var f))
        //            MaxYeetEfficiency = f / 100f;
        //    }
        //}

        private static float MaxYeetVariance
        {
            get => Options.Cards.MaxYeetVariance.Value;
            set => Options.Cards.MaxYeetVariance.Value = value;
        }

        private static string _varianceMulti2BonusString => $"{(MaxYeetVariance - 1f) * 100f:0.#}";
        private static string _maxYeetVarience;
        //{
        //    get => $"{(1f - MaxYeetVariance) * 100f:0.#}";
        //    set
        //    {
        //        if (float.TryParse(value, out var f))
        //            MaxYeetVariance = Mathf.Clamp(f, -20f, 20f) / 100f + 1f;
        //    }
        //}

        private static bool[] AlwaysYeet
        {
            get => Options.Cards.AlwaysYeetCSV.Value.Split(',').Select(s => s == "1").ToArray();
            set => Options.Cards.AlwaysYeetCSV.Value = value.Select(b => b ? "1" : "0").Join(s => s, ",");
        }

        private static bool AutoProtectChonkers
        {
            get => Options.Cards.AutoProtectChonkers.Value;
            set => Options.Cards.AutoProtectChonkers.Value = value;
        }

        //public AutoCardsPopup()
        //    : base(new Rect(Screen.width / 2 - (WIDTH / 2), Screen.height / 2 - (HEIGHT / 2), WIDTH, HEIGHT))
        //{

        //}

        //protected override void UpdateRect()
        //{
        //    base.UpdateRectCentered(WIDTH, HEIGHT);
        //}

        internal AutoCardsPopup() : base(WIDTH, HEIGHT) { }

        internal override void Open()
        {
            //base.WindowRect.x = Screen.width / 2 - (WIDTH / 2);
            //base.WindowRect.y = Screen.height / 2 - (HEIGHT / 2);

            _lastSortBy = AutoSortBy;
            _lastSortDirection = AutoSortDirection;

            _alwaysYeet = AlwaysYeet;
            Array.Copy(_alwaysYeet, _prevAlwaysYeet, 15);

            _maxYeetEfficiency = _efficiencyMulti2ModifierString;
            _maxYeetVarience = _varianceMulti2BonusString;

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

                _buttonStyle = new GUIStyle("button");
                _buttonStyle.fixedWidth = 90;

                _maxYeetEffTextStyle = new GUIStyle("textField");
                _maxYeetEffTextStyle.alignment = TextAnchor.MiddleRight;

                _disabledYeetRarities = new GUIStyle("box");
                _disabledYeetRarities.margin.top = 62;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Auto Sort/Yeet Options</b>", _titleStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();

            DrawAutoSort();
            DrawAutoProtectChonkers();
            DrawAutoYeet();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void DrawAutoSort()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Auto Sort: {(AutoSortEnabled ? "Enabled" : "Disabled")}");
            GUILayout.FlexibleSpace();
            AutoSortEnabled = GUILayout.SelectionGrid(AutoSortEnabled ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
            GUILayout.EndHorizontal();

            DrawAutoSortBy();
            DrawAutoSortDirection();

            GUILayout.EndVertical();
        }

        private static void DrawAutoSortBy()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"Sort By: {_sortBy[(int)AutoSortBy]}");
            GUILayout.FlexibleSpace();

            AutoSortBy = (CardSortBy)GUILayout.SelectionGrid((int)AutoSortBy, _sortBy, 4, _buttonStyle);
            if (AutoSortBy != _lastSortBy)
            {
                AutoCards.SortCards();
                _lastSortBy = AutoSortBy;
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawAutoSortDirection()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"Sort Direction: {(AutoSortDirection == CardSortDirection.Ascending ? "Ascending" : "Descending")}");
            GUILayout.FlexibleSpace();

            AutoSortDirection = GUILayout.SelectionGrid(AutoSortDirection == CardSortDirection.Ascending ? 0 : 1, _sortDirections, 2, _buttonStyle) == 0 ? CardSortDirection.Ascending : CardSortDirection.Descending;
            if (AutoSortDirection != _lastSortDirection)
            {
                AutoCards.SortCards();
                _lastSortDirection = AutoSortDirection;
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawAutoProtectChonkers()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto-Protect Chonkers When Spawned");
            GUILayout.FlexibleSpace();
            AutoProtectChonkers = GUILayout.SelectionGrid(AutoProtectChonkers ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void DrawAutoYeet()
        {
            GUILayout.BeginVertical("box");

            DrawAutoYeetModes();

            if (AutoYeetMode == CardYeetMode.Efficiency)
                DrawAutoYeetEfficiency();
            else if (AutoYeetMode == CardYeetMode.Rarity)
                DrawAutoYeetRarities();
            else if (AutoYeetMode == CardYeetMode.Variance)
                DrawAutoYeetVariance();

            GUILayout.EndVertical();

            if(AutoYeetMode != CardYeetMode.Disabled)
                DrawAlwaysYeet();
        }

        private static void DrawAutoYeetModes()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Auto Yeet Mode: {_autoYeetModes[(int)AutoYeetMode]}");
            AutoYeetMode = (CardYeetMode)GUILayout.SelectionGrid((int)AutoYeetMode, _autoYeetModes, 4, _buttonStyle);
            GUILayout.EndHorizontal();
        }

        private static bool _hadFocus_maxYeetEfficiency = false;
        private static void DrawAutoYeetEfficiency()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Mayo Efficiency to yeet (0 to 100): ");
            GUI.SetNextControlName("maxYeetEfficiency");
            _maxYeetEfficiency = GUILayout.TextField(_maxYeetEfficiency, _maxYeetEffTextStyle, GUILayout.Width(30f));
            GUILayout.Label("%");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _maxYeetEfficiency = Regex.Replace(_maxYeetEfficiency, @"[^0-9.,]", string.Empty);

            var hasFocus = GUI.GetNameOfFocusedControl() == "maxYeetEfficiency";
            if (hasFocus != _hadFocus_maxYeetEfficiency)
            {
                if (!hasFocus && float.TryParse(_maxYeetEfficiency, out var f))
                {
                    MaxYeetEfficiency = Mathf.Clamp(f, 0, 100) / 100f;
                    _maxYeetEfficiency = _efficiencyMulti2ModifierString;
                }

                _hadFocus_maxYeetEfficiency = hasFocus;
            }
        }

        private static bool _hadFocus_maxYeetVarience = false;
        private static void DrawAutoYeetVariance()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Variance to yeet (-20 to +20): ");
            GUI.SetNextControlName("maxYeetVarience");
            _maxYeetVarience = GUILayout.TextField(_maxYeetVarience, _maxYeetEffTextStyle, GUILayout.Width(30f));
            GUILayout.Label("%");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _maxYeetVarience = Regex.Replace(_maxYeetVarience, @"[^0-9.,-]", string.Empty);

            var hasFocus = GUI.GetNameOfFocusedControl() == "maxYeetVarience";
            if (hasFocus != _hadFocus_maxYeetVarience)
            {
                if (!hasFocus && float.TryParse(_maxYeetVarience, out var f))
                {
                    MaxYeetVariance = Mathf.Clamp(f, -20f, 20f) / 100f + 1f;
                    _maxYeetVarience = _varianceMulti2BonusString;
                }

                _hadFocus_maxYeetVarience = hasFocus;
            }
        }

        private static void DrawAutoYeetRarities()
        {
            GUILayout.BeginHorizontal();
            MaxYeetRarity = (rarity)GUILayout.SelectionGrid((int)MaxYeetRarity, _rarities, 7, _buttonStyle);
            GUILayout.EndHorizontal();
        }

        private static void DrawAlwaysYeet()
        {
            var changed = false;

            //if (MaxYeetEfficiency == 0)
            //    GUILayout.BeginVertical("box");
            //else
            //    GUILayout.BeginVertical(_disabledYeetRarities);
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Always Yeet (ignores {_autoYeetModes[(int)AutoYeetMode]}):");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (var x = 0; x < 8; x++)
            {
                GUILayout.BeginHorizontal("button", GUILayout.Width(78));
                _alwaysYeet[x] = GUILayout.Toggle(_alwaysYeet[x], _bonuses[x]);
                GUILayout.EndHorizontal();

                if (_alwaysYeet[x] != _prevAlwaysYeet[x])
                    changed = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (var x = 8; x < 15; x++)
            {
                GUILayout.BeginHorizontal("button", GUILayout.Width(90));
                _alwaysYeet[x] = GUILayout.Toggle(_alwaysYeet[x], _bonuses[x]);
                GUILayout.EndHorizontal();

                if (_alwaysYeet[x] != _prevAlwaysYeet[x])
                    changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (changed)
            {
                Array.Copy(_alwaysYeet, _prevAlwaysYeet, 15);
                AlwaysYeet = _alwaysYeet;
            }
        }
    }
}
