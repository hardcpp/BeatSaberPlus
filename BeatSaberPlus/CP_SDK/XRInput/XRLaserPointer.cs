#if CP_SDK_XR_INPUT
using UnityEngine;

namespace CP_SDK.XRInput
{
    /// <summary>
    /// Base laser pointer class
    /// </summary>
    public class XRLaserPointer : Unity.PersistentSingleton<XRLaserPointer>
    {
        /// <summary>
        /// Line renderer instance
        /// </summary>
        private LineRenderer m_LineRender;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static bool Render = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current controller having the laser pointer
        /// </summary>
        public XRController CurrentController { get; protected set; } = null;
        /// <summary>
        /// Width of the line
        /// </summary>
        public float LineWidthStart = 0.001f;
        /// <summary>
        /// Width of the line
        /// </summary>
        public float LineWidthEnd = 0.001f;
        /// <summary>
        /// Color of the line
        /// </summary>
        public Color LineColor = Color.green;
        /// <summary>
        /// Line material
        /// </summary>
        public Material LineMaterial = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component init
        /// </summary>
        private void Awake()
        {
            /// Creating line renderer
            m_LineRender = gameObject.AddComponent<LineRenderer>();
            m_LineRender.useWorldSpace      = false;
            m_LineRender.startWidth         = LineWidthStart;
            m_LineRender.endWidth           = LineWidthEnd;
            m_LineRender.startColor         = LineColor;
            m_LineRender.endColor           = LineColor;
            m_LineRender.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_LineRender.receiveShadows     = false;
            m_LineRender.material           = LineMaterial;
            m_LineRender.enabled            = Render && true;

            /// 0 distance at start
            SetDistance(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set attached controller
        /// </summary>
        /// <param name="p_Controller">New controller</param>
        public void SetController(XRController p_Controller)
        {
            /// Call base method
            CurrentController = p_Controller;

            /// Change our origin
            transform.SetParent(p_Controller?.RawTransform ?? null, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        /// <summary>
        /// Set pointer end distance
        /// </summary>
        /// <param name="p_Distance">End distance</param>
        public void SetDistance(float p_Distance)
        {
            m_LineRender.SetPosition(1, new Vector3(0, 0, p_Distance));
        }
        /// <summary>
        /// Set pointer visibility
        /// </summary>
        /// <param name="p_Visible">New visibility</param>
        public void SetVisible(bool p_Visible)
        {
            m_LineRender.enabled = Render && p_Visible;
        }
    }
}
#endif