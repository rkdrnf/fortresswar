using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using UnityEngine;

namespace FocusManager
{ 
    public abstract class InputFocusManager<T> where T : IComparable
    {
        protected T focus;
        protected Stack<T> oldFocuses;

        public InputFocusManager(T defaultFocus)
        {
            focus = defaultFocus;
            oldFocuses = new Stack<T>();
        }
    
        public bool IsFocused(T focus, params T[] focusList)
        {
            return Util.StateUtil.IsInState<T>(this.focus, focus, focusList);
        }
    
        public void FreeFocus(T freeFocus)
        {
            if (oldFocuses.Count == 0)
                return;

            if (focus.Equals(freeFocus) == false)
                return;
            
            FocusTo(oldFocuses.Pop());
            oldFocuses.Pop();
        }
    
        virtual public void FocusTo(T newFocus)
        {
            if (focus.Equals(newFocus))
                return;

            oldFocuses.Push(focus);
            focus = newFocus;
        }
    }
    
    public class KeyFocusManager : InputFocusManager<InputKeyFocus>
    {
        public KeyFocusManager(InputKeyFocus focus) : base(focus) {}

        public override void FocusTo(InputKeyFocus newFocus)
        {
            base.FocusTo(newFocus);

            switch(newFocus)
            {
                case InputKeyFocus.CHAT_WINDOW:
                    Input.imeCompositionMode = IMECompositionMode.On;
                    break;

                case InputKeyFocus.NAME_SELECTOR:
                    Input.imeCompositionMode = IMECompositionMode.On;
                    break;

                case InputKeyFocus.PLAYER:
                    Input.imeCompositionMode = IMECompositionMode.Off;
                    break;
            }
        }
    }
    
    public class MouseFocusManager : InputFocusManager<InputMouseFocus>
    {
        public MouseFocusManager(InputMouseFocus focus) : base(focus) { }
    }
}