using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class AutoCardsPopup : BasePopup
    {
        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _maxYeetEffTextStyle;

        private static string[] _disabledEnabled = ["Disabled", "Enabled"];
        private static string[] _sortBy = ["Rarity", "Type", "Efficiency", "Variance"];
        private static string[] _sortDirections = ["Ascending", "Descending"];
        private static string[] _rarities = ["Crappy", "Bad", "Meh", "Okay", "Good", "Great", "Damn"];

        private static CardSortBy _lastSortBy;
        private static CardSortDirection _lastSortDirection;

        private static bool AutoSortEnabled
        {
            get => Options.AutoCards.AutoSortEnabled.Value;
            set => Options.AutoCards.AutoSortEnabled.Value = value;
        }

        private static CardSortBy AutoSortBy
        {
            get => Options.AutoCards.AutoSortBy.Value;
            set => Options.AutoCards.AutoSortBy.Value = value;
        }

        private static CardSortDirection AutoSortDirection
        {
            get => Options.AutoCards.AutoSortDirection.Value;
            set => Options.AutoCards.AutoSortDirection.Value = value;
        }

        private static bool AutoYeetEnabled
        {
            get => Options.AutoCards.AutoYeetEnabled.Value;
            set => Options.AutoCards.AutoYeetEnabled.Value = value;
        }

        private static rarity MaxYeetRarity
        {
            get => Options.AutoCards.MaxYeetRarity.Value;
            set => Options.AutoCards.MaxYeetRarity.Value = value;
        }

        private static float MaxYeetEfficiency
        {
            get => Options.AutoCards.MaxYeetEfficiency.Value;
            set => Options.AutoCards.MaxYeetEfficiency.Value = value;
        }

        public AutoCardsPopup()
            : base(new Rect(Screen.width / 2f - 300f, Screen.height / 2f - 110f, 670f, 220f))
        {

        }

        internal override void Open()
        {
            _lastSortBy = AutoSortBy;
            _lastSortDirection = AutoSortDirection;
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
                _buttonStyle.fixedWidth = 90f;

                _maxYeetEffTextStyle = new GUIStyle("textField");
                _maxYeetEffTextStyle.alignment = TextAnchor.MiddleRight;
            }

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Auto Sort/Yeet Options</b>", _titleStyle);
            if (GUILayout.Button("×", GUILayout.Width(25))) Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            DrawAutoSortEnabled();
            DrawAutoSortBy();
            DrawAutoSortDirection();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            DrawAutoYeetEnabed();
            DrawAutoYeetThresholds();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void DrawAutoSortEnabled()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"Auto Sort: {(AutoSortEnabled ? "Enabled" : "Disabled")}");
            GUILayout.FlexibleSpace();
            AutoSortEnabled = GUILayout.SelectionGrid(AutoSortEnabled ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
            GUILayout.EndHorizontal();
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

        private static void DrawAutoYeetEnabed()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Auto Yeet: {(AutoYeetEnabled ? "Enabled" : "Disabled")}");
            GUILayout.FlexibleSpace();
            AutoYeetEnabled = GUILayout.SelectionGrid(AutoYeetEnabled ? 1 : 0, _disabledEnabled, 2, _buttonStyle) == 1;
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

        private static void DrawAutoYeetThresholds()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Efficiency to Yeet (disables max rarity if > 0%)");
            GUILayout.FlexibleSpace();
            _maxYeetEff = GUILayout.TextField(_maxYeetEff, _maxYeetEffTextStyle, GUILayout.Width(30f));
            GUILayout.Label("%");
            GUILayout.EndHorizontal();

            if (MaxYeetEfficiency == 0f)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Max Rarity to Yeet: {_rarities[(int)MaxYeetRarity]}");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                MaxYeetRarity = (rarity)GUILayout.SelectionGrid((int)MaxYeetRarity, _rarities, 7, _buttonStyle);
                GUILayout.EndHorizontal();
            }
        }
    }
}
