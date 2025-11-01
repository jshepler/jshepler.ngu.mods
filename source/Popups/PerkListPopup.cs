using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class PerkListPopup : BasePopup
    {
        const float WIDTH = 660f;
        const float HEIGHT = 340f;

        internal PerkListPopup() : base(WIDTH, HEIGHT) { }

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
            var controller = Plugin.Character.adventureController.itopod;

            GUILayout.BeginHorizontal();
            GUILayout.Label("PERK LIST", _titleLabelStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            PerkList.FilterEnabled = GUILayout.Toggle(PerkList.FilterEnabled, "Filtered   ");
            if (GUI.changed)
                controller.onFilterChange();

            PerkList.OrderEnabled = GUILayout.Toggle(PerkList.OrderEnabled, "Ordered   ");
            if (GUI.changed)
            {
                controller.onOrderChange();
                controller.updateFilters();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear"))
                PerkList.ClearPerks();

            GUILayout.EndHorizontal();
        }

        private static Vector2 _scrollView;
        private void drawList()
        {
            var itopod = Plugin.Character.adventureController.itopod;
            var level = Plugin.Character.adventure.itopod.perkLevel;

            _scrollView = GUILayout.BeginScrollView(_scrollView, false, false, GUILayout.ExpandHeight(true));

            // can't do foreach because the list can be changed, causing foreach to throw an exception
            // and technically, yes, the list displayed could be incorrect, but would only be for 1 frame
            // I'm ok with sacrificing that 1 frame of incorrectness to save the complexity of a more proper implementation
            for (var x = 0; x < PerkList.Perks.Count; x++)
            {
                var perkId = PerkList.Perks[x];

                GUILayout.BeginHorizontal("box");

                GUILayout.Label($"({perkId}) {itopod.perkName[perkId]}");
                GUILayout.Label($"{level[perkId]}/{itopod.maxLevel[perkId]}", GUILayout.ExpandWidth(false));

                if (!PerkList.CanBuyPerk(perkId))
                    GUI.enabled = false;

                if (GUILayout.Button("Buy", GUILayout.ExpandWidth(false)))
                    PerkList.BuyPerk(perkId);

                if (GUILayout.Button("Bulk", GUILayout.ExpandWidth(false)))
                    PerkList.BulkBuyPerk(perkId);

                GUI.enabled = true;

                if (GUILayout.Button(Assets.Arrow_up, GUILayout.ExpandWidth(false)))
                    PerkList.MovePerk(perkId, false);

                if (GUILayout.Button(Assets.Arrow_down, GUILayout.ExpandWidth(false)))
                    PerkList.MovePerk(perkId, true);

                if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                    PerkList.RemovePerk(perkId);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
