#if CP_SDK_XR_INPUT
using UnityEngine.EventSystems;

namespace CP_SDK.XRInput.InputInternals
{
    /// <summary>
    /// State holder for virtual mouse
    /// </summary>
    internal class FakeMouseState
    {
        /// <summary>
        /// Button states
        /// </summary>
        private FakeButtonState[] m_TrackedFakeButtons = new FakeButtonState[] {
            new FakeButtonState()
            {
                Button          = PointerEventData.InputButton.Left,
                EventData       = new PointerInputModule.MouseButtonEventData(),
                PressedValue    = 0.0f
            },
            new FakeButtonState()
            {
                Button          = PointerEventData.InputButton.Right,
                EventData       = new PointerInputModule.MouseButtonEventData(),
                PressedValue    = 0.0f
            },
            new FakeButtonState()
            {
                Button          = PointerEventData.InputButton.Middle,
                EventData       = new PointerInputModule.MouseButtonEventData(),
                PressedValue    = 0.0f
            }
        };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Any pressed button this frame?
        /// </summary>
        /// <returns></returns>
        internal bool AnyPressesThisFrame()
        {
            for (var l_I = 0; l_I < m_TrackedFakeButtons.Length; ++l_I)
            {
                if (m_TrackedFakeButtons[l_I].EventData.PressedThisFrame())
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Any released button this frame?
        /// </summary>
        /// <returns></returns>
        internal bool AnyReleasesThisFrame()
        {
            for (var l_I = 0; l_I < m_TrackedFakeButtons.Length; ++l_I)
            {
                if (m_TrackedFakeButtons[l_I].EventData.ReleasedThisFrame())
                    return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get specific button state
        /// </summary>
        /// <param name="p_Button">Requested button</param>
        /// <returns></returns>
        internal FakeButtonState GetFakeButtonState(PointerEventData.InputButton p_Button)
        {
            return m_TrackedFakeButtons[(int)p_Button];
        }
        /// <summary>
        /// Set button state
        /// </summary>
        /// <param name="p_Button">Target</param>
        /// <param name="p_StateForMouseButton">New state</param>
        /// <param name="p_Data">Event data</param>
        internal void SetFakeButtonState(   PointerEventData.InputButton        p_Button,
                                            PointerEventData.FramePressState    p_StateForMouseButton,
                                            PointerEventData                    p_Data)
        {
            var l_ButtonState = GetFakeButtonState(p_Button);
            l_ButtonState.EventData.buttonState = p_StateForMouseButton;
            l_ButtonState.EventData.buttonData  = p_Data;
        }
    }
}
#endif