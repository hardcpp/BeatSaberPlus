using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Abstract base modal component
    /// </summary>
    public abstract class IModal : MonoBehaviour
    {
        private RectTransform   m_RTransform;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public RectTransform   RTransform  => m_RTransform;
        public IViewController VController;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        private void Awake()
        {
            m_RTransform  = transform as RectTransform;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public abstract void OnShow();
        /// <summary>
        /// On modal close
        /// </summary>
        public abstract void OnClose();
    }
}
