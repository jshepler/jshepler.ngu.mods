using System;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal abstract class BasePopupWithReturn<TReturn> : BasePopup
    {
        internal new EventHandler<PopupClosedEventArgs<TReturn>> Closed;

        internal BasePopupWithReturn(Rect windowRect) : base(windowRect)
        {
        }

        internal override void Close()
        {
            Close(default);
        }

        protected virtual void Close(TReturn value)
        {
            IsOpen = false;
            Closed?.Invoke(this, PopupClosedEventArgs<TReturn>.From(value));
        }
    }

    internal class PopupClosedEventArgs<T>
    {
        internal T Value;

        public PopupClosedEventArgs(T value)
        {
            this.Value = value;
        }

        public static PopupClosedEventArgs<T> From(T value)
        {
            return new PopupClosedEventArgs<T>(value);
        }
    }
}
