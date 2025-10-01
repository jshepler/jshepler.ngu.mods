using System.Collections.Generic;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class QuirkListPopup : BasePopup
    {
        const float WIDTH = 660f;
        const float HEIGHT = 340f;

        internal delegate bool CanBuyQuirkDelegate(int quirkId);
        internal delegate void BuyQuirkDelegate(int quirkId);
        internal delegate void BulkBuyQuirkDelegate(int quirkId);
        internal delegate void MoveQuirkDelegate(int quirkId, bool moveDown);
        internal delegate void RemoveQuirkDelegate(int quirkId);

        private List<int> _quirkIDs;
        private CanBuyQuirkDelegate _canBuyQuirk;
        private BuyQuirkDelegate _buyQuirk;
        private BulkBuyQuirkDelegate _bulkBuyQuirk;
        private MoveQuirkDelegate _moveQuirk;
        private RemoveQuirkDelegate _removeQuirk;

        internal QuirkListPopup(CanBuyQuirkDelegate CanBuyQuirk, BuyQuirkDelegate BuyQuirk, BulkBuyQuirkDelegate BulkBuyQuirk, MoveQuirkDelegate MoveQuirk, RemoveQuirkDelegate RemoveQuirk)
            : base(WIDTH, HEIGHT)
        {
            _canBuyQuirk = CanBuyQuirk;
            _buyQuirk = BuyQuirk;
            _bulkBuyQuirk = BulkBuyQuirk;
            _moveQuirk = MoveQuirk;
            _removeQuirk = RemoveQuirk;
        }

        internal void Open(List<int> quirkIDs)
        {
            _quirkIDs = quirkIDs;
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
            GUILayout.Label("QUIRK LIST", _titleLabelStyle);
            if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                Close();
            GUILayout.EndHorizontal();
        }

        private static Vector2 _scrollView;
        private void drawList()
        {
            var beastQuest = Plugin.Character.beastQuest;
            var controller = Plugin.Character.beastQuestPerkController;

            _scrollView = GUILayout.BeginScrollView(_scrollView, false, false, GUILayout.ExpandHeight(true));

            for (var x = 0; x < _quirkIDs.Count; x++)
            {
                var quirkId = _quirkIDs[x];

                GUILayout.BeginHorizontal("box");

                GUILayout.Label($"({quirkId}) {controller.quirkName[quirkId]}");
                GUILayout.Label($"{beastQuest.quirkLevel[quirkId]}/{controller.maxLevel[quirkId]}", GUILayout.ExpandWidth(false));

                if (!_canBuyQuirk(quirkId))
                    GUI.enabled = false;

                if (GUILayout.Button("Buy", GUILayout.ExpandWidth(false)))
                    _buyQuirk(quirkId);

                if (GUILayout.Button("Bulk", GUILayout.ExpandWidth(false)))
                    _bulkBuyQuirk(quirkId);

                GUI.enabled = true;

                if (GUILayout.Button(Assets.Arrow_up, GUILayout.ExpandWidth(false)))
                    _moveQuirk(quirkId, false);

                if (GUILayout.Button(Assets.Arrow_down, GUILayout.ExpandWidth(false)))
                    _moveQuirk(quirkId, true);

                if (GUILayout.Button("×", GUILayout.ExpandWidth(false)))
                    _removeQuirk(quirkId);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
