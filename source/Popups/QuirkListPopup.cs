using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class QuirkListPopup : BasePopup
    {
        const float WIDTH = 660f;
        const float HEIGHT = 340f;

        internal QuirkListPopup() : base(WIDTH, HEIGHT) { }

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleLabelStyle;

        private static void initStyles(Rect windowRect)
        {
            _windowStyle = new GUIStyle("box");
            _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

            _titleLabelStyle = new GUIStyle("label");
            _titleLabelStyle.alignment = TextAnchor.MiddleCenter;
            _titleLabelStyle.wordWrap = false;
        }

        protected override void DrawWindow(Rect windowRect)
        {
            if (_windowStyle == null)
                initStyles(windowRect);

            GUILayout.BeginArea(windowRect, _windowStyle);
            GUILayout.BeginVertical();

            drawTitle();
            drawList();

            GUILayout.EndVertical();
            GUILayout.EndArea();

        }

        private void drawTitle()
        {
            var controller = Plugin.Character.beastQuestPerkController;

            GUILayout.BeginHorizontal();
            GUILayout.Label("QUIRK LIST", _titleLabelStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            QuirkList.FilterEnabled = GUILayout.Toggle(QuirkList.FilterEnabled, "Filtered   ");
            if (GUI.changed)
                controller.onFilterChange();

            QuirkList.OrderEnabled = GUILayout.Toggle(QuirkList.OrderEnabled, "Ordered   ");
            if (GUI.changed)
            {
                controller.onOrderChange();
                controller.updateFilters();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear"))
                QuirkList.ClearQuirks();

            GUILayout.EndHorizontal();
        }

        private static Vector2 _scrollView;
        private void drawList()
        {
            var beastQuest = Plugin.Character.beastQuest;
            var controller = Plugin.Character.beastQuestPerkController;

            _scrollView = GUILayout.BeginScrollView(_scrollView, false, false, GUILayout.ExpandHeight(true));

            for (var x = 0; x < QuirkList.Quirks.Count; x++)
            {
                var quirkId = QuirkList.Quirks[x];

                GUILayout.BeginHorizontal("box");

                GUILayout.Label($"({quirkId}) {controller.quirkName[quirkId]}");
                GUILayout.Label($"{beastQuest.quirkLevel[quirkId]}/{controller.maxLevel[quirkId]}", GUILayout.ExpandWidth(false));

                if (!QuirkList.CanBuyQuirk(quirkId))
                    GUI.enabled = false;

                if (GUILayout.Button("Buy", GUILayout.ExpandWidth(false)))
                    QuirkList.BuyQuirk(quirkId);

                if (GUILayout.Button("Bulk", GUILayout.ExpandWidth(false)))
                    QuirkList.BulkBuyQuirk(quirkId);

                GUI.enabled = true;

                if (GUILayout.Button(Assets.Arrow_up, GUILayout.ExpandWidth(false)))
                    QuirkList.MoveQuirk(quirkId, false);

                if (GUILayout.Button(Assets.Arrow_down, GUILayout.ExpandWidth(false)))
                    QuirkList.MoveQuirk(quirkId, true);

                if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                    QuirkList.RemoveQuirk(quirkId);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
