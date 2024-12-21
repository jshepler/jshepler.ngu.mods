using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class AutoCardsPopup : BasePopup
    {
        const int WIDTH = 670;
        const int HEIGHT = 346;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _maxYeetEffTextStyle;
        private static GUIStyle _disabledYeetRarities;

        private static string[] _disabledEnabled = ["Disabled", "Enabled"];
        private static string[] _sortBy = ["Rarity", "Type", "Efficiency", "Variance"];
        private static string[] _sortDirections = ["Ascending", "Descending"];
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

        private static bool AutoYeetEnabled
        {
            get => Options.Cards.AutoYeetEnabled.Value;
            set => Options.Cards.AutoYeetEnabled.Value = value;
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

        public AutoCardsPopup()
            : base(new Rect(Screen.width / 2 - (WIDTH / 2), Screen.height / 2 - (HEIGHT / 2), WIDTH, HEIGHT))
        {

        }

        internal override void Open()
        {
            base.WindowRect.x = Screen.width / 2 - (WIDTH / 2);
            base.WindowRect.y = Screen.height / 2 - (HEIGHT / 2);

            _lastSortBy = AutoSortBy;
            _lastSortDirection = AutoSortDirection;

            _alwaysYeet = AlwaysYeet;
            Array.Copy(_alwaysYeet, _prevAlwaysYeet, 15);

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
            if (GUILayout.Button("×", GUILayout.Width(25))) Close();
            GUILayout.EndHorizontal();

            DrawAutoSort();
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

        private static string _maxYeetEff
        {
            get => $"{MaxYeetEfficiency * 100f:0.##}";
            set
            {
                if (float.TryParse(value, out var f))
                    MaxYeetEfficiency = f / 100f;
            }
        }

        private static void DrawAutoYeet()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto-Protect Chonkers When Spawned");
            GUILayout.FlexibleSpace();
            AutoProtectChonkers = GUILayout.SelectionGrid(AutoProtectChonkers ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Auto Yeet: {(AutoYeetEnabled ? "Enabled" : "Disabled")}");
            GUILayout.FlexibleSpace();
            AutoYeetEnabled = GUILayout.SelectionGrid(AutoYeetEnabled ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Efficiency to Yeet (0 = disabled, if > 0 disables max rarity): ");
            _maxYeetEff = GUILayout.TextField(_maxYeetEff, _maxYeetEffTextStyle, GUILayout.Width(30f));
            GUILayout.Label("%");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (MaxYeetEfficiency == 0f)
                DrawAutoYeetRarities();

            DrawAlwaysYeet();
        }

        private static void DrawAutoYeetRarities()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Max Rarity to Yeet: {_rarities[(int)MaxYeetRarity]}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            MaxYeetRarity = (rarity)GUILayout.SelectionGrid((int)MaxYeetRarity, _rarities, 7, _buttonStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void DrawAlwaysYeet()
        {
            var changed = false;

            if (MaxYeetEfficiency == 0)
                GUILayout.BeginVertical("box");
            else
                GUILayout.BeginVertical(_disabledYeetRarities);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Always Yeet (ignores efficiency and rarity):");
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

            //GUILayout.BeginHorizontal();
            //GUILayout.BeginHorizontal("button", GUILayout.Width(90));
            //_alwaysYeet[0] = GUILayout.Toggle(_alwaysYeet[0], _bonuses[0]);
            //GUILayout.EndHorizontal();
            //GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (changed)// || _alwaysYeet[0] != _prevAlwaysYeet[0])
            {
                Array.Copy(_alwaysYeet, _prevAlwaysYeet, 15);
                AlwaysYeet = _alwaysYeet;
            }
        }
    }
}
