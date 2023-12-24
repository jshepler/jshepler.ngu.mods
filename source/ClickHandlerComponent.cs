using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    internal class ClickHandlerComponent : MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> _onRightClick;
        public Action<PointerEventData> _onLeftClick;
        public Action<PointerEventData> _onMiddleClick;
        public Action<ClickEventData> _onClick;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    if (_onLeftClick != null) _onLeftClick(eventData);
                    break;

                case PointerEventData.InputButton.Middle:
                    if (_onMiddleClick != null) _onMiddleClick(eventData);
                    break;

                case PointerEventData.InputButton.Right:
                    if (_onRightClick != null) _onRightClick(eventData);
                    break;
            }

            if (_onClick != null) _onClick(new ClickEventData(eventData));
        }

        public void OnLeftClick(Action<PointerEventData> action)
        {
            _onLeftClick = action;
        }

        public void OnMiddleClick(Action<PointerEventData> action)
        {
            _onMiddleClick = action;
        }

        public void OnRightClick(Action<PointerEventData> action)
        {
            _onRightClick = action;
        }

        public void OnClick(Action<ClickEventData> action)
        {
            _onClick = action;
        }
    }

    internal class ClickEventData
    {
        public PointerEventData PointerEventData;

        public ClickEventData(PointerEventData pointerEventData)
        {
            PointerEventData = pointerEventData;
        }

        public bool IsRightClick { get => this.PointerEventData.button == PointerEventData.InputButton.Right; }
        public bool IsLeftClick { get => this.PointerEventData.button == PointerEventData.InputButton.Left; }
        public bool IsMiddleClick { get => this.PointerEventData.button == PointerEventData.InputButton.Middle; }
    }
}
