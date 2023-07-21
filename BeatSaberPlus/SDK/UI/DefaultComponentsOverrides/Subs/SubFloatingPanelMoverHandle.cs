using UnityEngine;

namespace BeatSaberPlus.SDK.UI.DefaultComponentsOverrides.Subs
{
    /// <summary>
    /// Floating panel mover handle
    /// </summary>
    internal class SubFloatingPanelMoverHandle : MonoBehaviour
    {
        /// <summary>
        /// Floating panel instance
        /// </summary>
        internal BS_CFloatingPanel FloatingPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        private void Awake()
        {
            gameObject.AddComponent<BoxCollider>().size = new Vector3(1f, 1f, 0.1f);
            gameObject.layer = CP_SDK.UI.UISystem.UILayer;
        }
    }
}
