#if CP_SDK_XR_INPUT
using UnityEngine;

namespace CP_SDK.XRInput
{
    /// <summary>
    /// XR controller class
    /// </summary>
    public abstract class XRController : MonoBehaviour
    {
        /// <summary>
        /// Is left hand
        /// </summary>
        public bool IsLeftHand { get; protected set; }

        /// <summary>
        /// Raw device transform
        /// </summary>
        public Transform RawTransform;

        /// <summary>
        /// Axis 0/Joystick value
        /// </summary>
        public Vector2 Axis0Joystick { get; protected set; } = Vector2.zero;
        /// <summary>
        /// Axis 1/Trigger value
        /// </summary>
        public float Axis1Trigger { get; protected set; } = 0f;
        /// <summary>
        /// Axis 2/Trackpad value
        /// </summary>
        public Vector2 Axis2 { get; protected set; } = Vector2.zero;
        /// <summary>
        /// Grip value
        /// </summary>
        public float Grip { get; protected set; } = 0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Press button data
        /// </summary>
        protected bool[] m_PressButtonData = new bool[(int)EXRInputButton.MAX];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is button pressed
        /// </summary>
        /// <param name="p_Button">Target button</param>
        /// <returns></returns>
        public bool IsButtonPressed(EXRInputButton p_Button)
        {
            if (p_Button < 0 || p_Button > EXRInputButton.MAX)
                return false;

            return m_PressButtonData[(int)p_Button];
        }
    }
}
#endif