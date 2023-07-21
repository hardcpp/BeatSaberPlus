using System.Linq;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus.SDK.UI.DefaultComponentsOverrides.Subs
{
    /// <summary>
    /// Floating panel mover
    /// </summary>
    internal class SubFloatingPanelMover : MonoBehaviour
    {
        protected const float MinScrollDistance = 0.25f;
        protected const float MaxLaserDistance  = 50f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private BS_CFloatingPanel           m_FloatingPanel;
        private VRPointer                   m_VRPointer;
        private VRController                m_GrabbingController;
        private Vector3                     m_GrabPosition;
        private Quaternion                  m_GrabRotation;
        private FirstPersonFlyingController m_FPFC;
        private RaycastHit[]                m_RaycastBuffer = new RaycastHit[10];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        private void Awake()
        {
            m_VRPointer = GetComponent<VRPointer>();
            m_FPFC      = Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component destroy
        /// </summary>
        private void OnDestroy()
        {
            m_VRPointer             = null;
            m_FloatingPanel         = null;
            m_GrabbingController    = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            var l_IsFPFC        = IsFPFC();

#if BEATSABER_1_29_4_OR_NEWER
            var l_VRController          = m_VRPointer?.lastSelectedVrController;
#else
            var l_VRController          = m_VRPointer?.vrController;
#endif
            var l_VRControllerTransform = l_VRController?.transform;
            var l_ButtonDown            = l_VRController?.triggerValue > 0.9f || (l_IsFPFC && Input.GetMouseButton(0));

            if (l_VRController != null && l_ButtonDown)
            {
                if (m_GrabbingController != null)
                    return;

                var l_HitCount = Physics.RaycastNonAlloc(   l_VRControllerTransform.position,
                                                            l_VRControllerTransform.forward,
                                                            m_RaycastBuffer,
                                                            MaxLaserDistance,
                                                            1 << CP_SDK.UI.UISystem.UILayer);

                for (var l_I = 0; l_I < l_HitCount; ++l_I)
                {
                    var l_SubFloatingPanelMoverHandle = m_RaycastBuffer[l_I].transform?.GetComponent<SubFloatingPanelMoverHandle>();
                    if (!l_SubFloatingPanelMoverHandle)
                        continue;


                    m_FloatingPanel         = l_SubFloatingPanelMoverHandle.FloatingPanel;
                    m_GrabbingController    = l_VRController;
                    m_GrabPosition          = l_VRControllerTransform.InverseTransformPoint(m_FloatingPanel.RTransform.position);
                    m_GrabRotation          = Quaternion.Inverse(l_VRControllerTransform.rotation) * m_FloatingPanel.RTransform.rotation;

                    m_FloatingPanel.FireOnGrab();
                    break;
                }
            }

            if (m_GrabbingController != null && !l_ButtonDown)
            {
                m_FloatingPanel.FireOnRelease();

                m_FloatingPanel         = null;
                m_GrabbingController    = null;
            }
        }
        /// <summary>
        /// On frame (late)
        /// </summary>
        private void LateUpdate()
        {
            if (m_GrabbingController == null)
                return;

#if BEATSABER_1_29_4_OR_NEWER
            var l_Delta = m_GrabbingController.thumbstick.y * Time.unscaledDeltaTime;
#else
            var l_Delta = m_GrabbingController.verticalAxisValue * Time.unscaledDeltaTime;
#endif
            if (m_GrabPosition.magnitude > MinScrollDistance)   m_GrabPosition -= Vector3.forward * l_Delta;
            else                                                m_GrabPosition -= Vector3.forward * Mathf.Clamp(l_Delta, float.MinValue, 0f);

            var l_RealPosition = m_GrabbingController.transform.TransformPoint(m_GrabPosition);
            var l_RealRotation = m_GrabbingController.transform.rotation * m_GrabRotation;

            m_FloatingPanel.RTransform.position = Vector3.Lerp(m_FloatingPanel.RTransform.position,       l_RealPosition, 10f * Time.unscaledDeltaTime);
            m_FloatingPanel.RTransform.rotation = Quaternion.Slerp(m_FloatingPanel.RTransform.rotation,   l_RealRotation,  5f * Time.unscaledDeltaTime);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is in FPFC
        /// </summary>
        /// <returns></returns>
        private bool IsFPFC()
        {
            if (m_FPFC != null)
                return m_FPFC.enabled;

            return false;
        }
    }
}
