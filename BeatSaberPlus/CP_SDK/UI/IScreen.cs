using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Screen slot enum
    /// </summary>
    public enum EScreenSlot
    {
        Left,
        Middle,
        Right
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Abstract screen
    /// </summary>
    public abstract class IScreen : MonoBehaviour
    {
        public abstract IViewController CurrentViewController { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Replace active view controller
        /// </summary>
        /// <param name="p_ViewController">New view controller</param>
        public abstract void SetViewController(IViewController p_ViewController);
    }
}
