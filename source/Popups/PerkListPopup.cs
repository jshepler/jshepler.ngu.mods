using System.Collections.Generic;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class PerkListPopup : BasePopup
    {
        const float WIDTH = 660f;
        const float HEIGHT = 340f;

        internal delegate bool CanBuyPerkDelegate(int perkId);
        internal delegate void BuyPerkDelegate(int perkId);
        internal delegate void BulkBuyPerkDelegate(int perkId);
        internal delegate void MovePerkDelegate(int perkId, bool moveDown);
        internal delegate void RemovePerkDelegate(int perkId);

        private List<int> _perkIDs;
        private CanBuyPerkDelegate _canBuyPerk;
        private BuyPerkDelegate _buyPerk;
        private BulkBuyPerkDelegate _bulkBuyPerk;
        private MovePerkDelegate _movePerk;
        private RemovePerkDelegate _removePerk;

        internal PerkListPopup(CanBuyPerkDelegate CanBuyPerk, BuyPerkDelegate BuyPerk, BulkBuyPerkDelegate BulkBuyPerk, MovePerkDelegate MovePerk, RemovePerkDelegate RemovePerk)
            : base(WIDTH, HEIGHT)
        {
            _canBuyPerk = CanBuyPerk;
            _buyPerk = BuyPerk;
            _bulkBuyPerk = BulkBuyPerk;
            _movePerk = MovePerk;
            _removePerk = RemovePerk;
        }

        internal void Open(List<int> perkIDs)
        {
            _perkIDs = perkIDs;
            base.Open();
        }

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
            GUILayout.BeginHorizontal();
            GUILayout.Label("PERK LIST", _titleLabelStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();
        }

        private static Vector2 _scrollView;
        private void drawList()
        {
            var itopod = Plugin.Character.adventureController.itopod;
            var level = Plugin.Character.adventure.itopod.perkLevel;

            _scrollView = GUILayout.BeginScrollView(_scrollView, false, false, GUILayout.ExpandHeight(true));

            for (var x = 0; x < _perkIDs.Count; x++)
            {
                var perkId = _perkIDs[x];

                GUILayout.BeginHorizontal("box");

                GUILayout.Label($"({perkId}) {itopod.perkName[perkId]}");
                GUILayout.Label($"{level[perkId]}/{itopod.maxLevel[perkId]}", GUILayout.ExpandWidth(false));

                if (!_canBuyPerk(perkId))
                    GUI.enabled = false;

                if (GUILayout.Button("Buy", GUILayout.ExpandWidth(false)))
                    _buyPerk(perkId);

                if (GUILayout.Button("Bulk", GUILayout.ExpandWidth(false)))
                    _bulkBuyPerk(perkId);

                GUI.enabled = true;

                if (GUILayout.Button(Assets.Arrow_up, GUILayout.ExpandWidth(false)))
                    _movePerk(perkId, false);

                if (GUILayout.Button(Assets.Arrow_down, GUILayout.ExpandWidth(false)))
                    _movePerk(perkId, true);

                if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                    _removePerk(perkId);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
