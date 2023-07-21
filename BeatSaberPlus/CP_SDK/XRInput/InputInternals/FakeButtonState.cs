#if CP_SDK_XR_INPUT
using UnityEngine.EventSystems;

namespace CP_SDK.XRInput.InputInternals
{
    /// <summary>
    /// Button state
    /// </summary>
    internal class FakeButtonState
    {
        /// <summary>
        /// Related button
        /// </summary>
        internal PointerEventData.InputButton Button;
        /// <summary>
        /// Last event data
        /// </summary>
        internal PointerInputModule.MouseButtonEventData EventData;
        /// <summary>
        /// Press state
        /// </summary>
        internal float PressedValue;
    }
}
#endif