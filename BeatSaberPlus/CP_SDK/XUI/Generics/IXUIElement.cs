using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// Element interface
    /// </summary>
    public abstract class IXUIElement
    {
        protected string m_InitialName;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public abstract RectTransform RTransform { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Initial name</param>
        protected IXUIElement(string p_Name) => m_InitialName = p_Name;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public abstract void BuildUI(Transform p_Parent);
    }
}
