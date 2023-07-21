using System;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents.Subs
{
    /// <summary>
    /// Toggle with callbacks component
    /// </summary>
    public class SubToggleWithCallbacks : Toggle
    {
        /// <summary>
        /// Selection state enum
        /// </summary>
        public new enum SelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// State did change event
        /// </summary>
        public event Action<SelectionState> StateDidChangeEvent;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do state transition
        /// </summary>
        /// <param name="p_State">New state</param>
        /// <param name="p_Instant">Is instant</param>
        protected override void DoStateTransition(Selectable.SelectionState p_State, bool p_Instant)
        {
            base.DoStateTransition(p_State, p_Instant);
            StateDidChangeEvent?.Invoke((SelectionState)p_State);
        }
    }
}
