using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus.SDK.UI.DefaultComponentsOverrides
{
    /// <summary>
    /// BeatSaber CFloatingPanel component
    /// </summary>
    internal class BS_CFloatingPanel : CP_SDK.UI.DefaultComponents.DefaultCFloatingPanel
    {
        /// <summary>
        /// Mover handle
        /// </summary>
        private Subs.SubFloatingPanelMoverHandle m_MoverHandle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public override void Init()
        {
            var l_ShouldContinue = !m_RTransform;
            base.Init();

            if (!l_ShouldContinue)
                return;

            var l_CurvedCanvasSettings = gameObject.AddComponent<CurvedCanvasSettings>();
            l_CurvedCanvasSettings.SetRadius(140f);

            CreateMover();
            SetAllowMovement(false);

            Patches.PVRPointer.OnActivated -= CreateMoverOnPointerCreated;
            Patches.PVRPointer.OnActivated += CreateMoverOnPointerCreated;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set radius on supported games
        /// </summary>
        /// <param name="p_Radius">Canvas radius</param>
        /// <returns></returns>
        public override CP_SDK.UI.Components.CFloatingPanel SetRadius(float p_Radius)
        {
            base.SetRadius(p_Radius);
            gameObject.GetComponent<CurvedCanvasSettings>()?.SetRadius(p_Radius);

            return this;
        }
        /// <summary>
        /// Set size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public override CP_SDK.UI.Components.CFloatingPanel SetSize(Vector2 p_Size)
        {
            base.SetSize(p_Size);
            UpdateMover();
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component destroy
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Patches.PVRPointer.OnActivated -= CreateMoverOnPointerCreated;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create mover
        /// </summary>
        /// <param name="p_VRPointer">VRPointer instance</param>
        private void CreateMoverOnPointerCreated(VRPointer p_VRPointer) => CreateMover(p_VRPointer);
        /// <summary>
        /// Create mover
        /// </summary>
        /// <param name="p_VRPointer">VRPointer instance</param>
        private void CreateMover(VRPointer p_VRPointer = null)
        {
            if (p_VRPointer == null)
                p_VRPointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();

            if (p_VRPointer == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Warning("[BeatSaberPlus.SDK.UI.DefaultComponentsOverrides][BS_CFloatingPanel.CreateMover] Failed to get VRPointer!");
                return;
            }

            if (!p_VRPointer.GetComponent<Subs.SubFloatingPanelMover>())
                p_VRPointer.gameObject.AddComponent<Subs.SubFloatingPanelMover>();

            if (m_MoverHandle == null)
            {
                m_MoverHandle = new GameObject("MoverHandle", typeof(Subs.SubFloatingPanelMoverHandle)).GetComponent<Subs.SubFloatingPanelMoverHandle>();
                m_MoverHandle.transform.SetParent(transform);
                m_MoverHandle.transform.localPosition   = Vector3.zero;
                m_MoverHandle.transform.localRotation   = Quaternion.identity;
                m_MoverHandle.transform.localScale      = Vector3.one;
                m_MoverHandle.FloatingPanel             = this;

                UpdateMover();
            }
        }
        /// <summary>
        /// Update mover collision
        /// </summary>
        private void UpdateMover()
        {
            if (m_MoverHandle == null)
                return;

            var l_Size = m_RTransform.sizeDelta;

            m_MoverHandle.transform.localPosition = new Vector3(0.0f, 0.0f, 0.1f);
            m_MoverHandle.transform.localRotation = Quaternion.identity;
            m_MoverHandle.transform.localScale    = new Vector3(l_Size.x, l_Size.y, 0.1f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set allow movements
        /// </summary>
        /// <param name="p_Allow">Is allowed?</param>
        /// <returns></returns>
        public override CP_SDK.UI.Components.CFloatingPanel SetAllowMovement(bool p_Allow)
        {
            base.SetAllowMovement(p_Allow);

            if (m_MoverHandle)
                m_MoverHandle.gameObject.SetActive(p_Allow);

            if (p_Allow)
            {
                /// Refresh VR pointer due to bug
                var l_VRPointers    = Resources.FindObjectsOfTypeAll<VRPointer>();
                var l_VRPointer     = CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing ? l_VRPointers.LastOrDefault() : l_VRPointers.FirstOrDefault();

                if (l_VRPointer)
                {
                    if (!l_VRPointer.GetComponent<Subs.SubFloatingPanelMover>())
                        l_VRPointer.gameObject.AddComponent<Subs.SubFloatingPanelMover>();
                }
                else
                    CP_SDK.ChatPlexSDK.Logger.Warning("[BeatSaberPlus.SDK.UI.DefaultComponentsOverrides][BS_CFloatingPanel.SetAllowMovement] Failed to get VRPointer!");
            }

            return this;
        }
    }
}
