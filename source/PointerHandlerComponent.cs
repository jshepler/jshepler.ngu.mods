using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    internal class PointerHandlerComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action<PointerEventData> _onPointerEnter;
        private Action<PointerEventData> _onPointerExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_onPointerEnter != null)
                _onPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_onPointerExit != null)
                _onPointerExit(eventData);
        }

        internal PointerHandlerComponent OnPointerEnter(Action<PointerEventData> action)
        {
            _onPointerEnter = action;
            return this;
        }

        internal PointerHandlerComponent OnPointerExit(Action<PointerEventData> action)
        {
            _onPointerExit = action;
            return this;
        }
    }
}
