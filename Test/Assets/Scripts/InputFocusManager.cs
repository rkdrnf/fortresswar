using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;

namespace FocusManager
{ 
    public abstract class InputFocusManager<T> where T : IComparable
    {
        protected T focus;
    
        public bool IsFocused(T focus, params T[] focusList)
        {
            return Util.StateUtil.IsInState<T>(this.focus, focus, focusList);
        }
    
        public abstract void FreeFocus();
    
        public void FocusTo(T focus)
        {
            this.focus = focus;
        }
    }
    
    public class KeyFocusManager : InputFocusManager<InputKeyFocus>
    {
        override public void FreeFocus()
        {
            focus = InputKeyFocus.PLAYER;
        }
    }
    
    public class MouseFocusManager : InputFocusManager<InputMouseFocus>
    {
        override public void FreeFocus()
        {
            focus = InputMouseFocus.PLAYER;
        }
    }
}