using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.Animation
{
    /// <summary>
    /// Animation state updater
    /// </summary>
    public class AnimationStateUpdater : MonoBehaviour
    {
        /// <summary>
        /// Animation controller data
        /// </summary>
        private AnimationControllerInstance m_ControllerDataInstance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Target image component
        /// </summary>
        public Image TargetImage;
        /// <summary>
        /// Controller data instance
        /// </summary>
        public AnimationControllerInstance ControllerDataInstance
        {
            get => m_ControllerDataInstance;
            set
            {
                if (m_ControllerDataInstance != null)
                    OnDisable();

                m_ControllerDataInstance = value;

                if (isActiveAndEnabled)
                    OnEnable();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On destroy
        /// </summary>
        private void OnDestroy()
            => ControllerDataInstance?.Unregister(TargetImage);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On enabled
        /// </summary>
        private void OnEnable()
            => ControllerDataInstance?.Register(TargetImage);
        /// <summary>
        /// On disable
        /// </summary>
        private void OnDisable()
            => ControllerDataInstance?.Unregister(TargetImage);
    }
}
