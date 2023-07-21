#if CP_SDK_XR_INPUT
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.XRInput
{
    /// <summary>
    /// Graphic ray caster for XR UI
    /// </summary>
    public class XRGraphicRaycaster : BaseRaycaster
    {
        /// <summary>
        /// Registered raycasters
        /// </summary>
        public static List<XRGraphicRaycaster> s_GraphicRaycasters = new List<XRGraphicRaycaster>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Result for canvas ray casts
        /// </summary>
        private struct XRGraphicRaycastResult
        {
            internal Graphic    HitGraphic;
            internal float      HitDistance;
            internal Vector3    HitWorldPosition;
            internal Vector2    InsideRootCanvasHitPosition;
            internal int        Depth;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Canvas                                      m_Canvas            = null;
        private InputInternals.FrameCachedPhysicsRaycaster  m_PhysicsRaycaster  = null;
        private List<XRGraphicRaycastResult>                m_RaycastResults    = new List<XRGraphicRaycastResult>();
        private RaycastHit2D[]                              m_Raycast2DBuffer   = new RaycastHit2D[1];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event camera for BaseRaycaster
        /// </summary>
        public override Camera eventCamera => (Camera)null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On creation
        /// </summary>
        protected override void Awake()
        {
            s_GraphicRaycasters.Remove(this);
            s_GraphicRaycasters.Add(this);

            base.Awake();
        }
        /// <summary>
        /// On destroy
        /// </summary>
        protected override void OnDestroy()
        {
            base.Awake();

            s_GraphicRaycasters.Remove(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component enabling
        /// </summary>
        protected override void OnEnable()
        {
            /// Call base method
            base.OnEnable();

            /// Grab required components
            m_Canvas            = GetComponent<Canvas>();
            m_PhysicsRaycaster  = InputInternals.FrameCachedPhysicsRaycaster.Instance();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raycast against the Scene.
        /// </summary>
        /// <param name="p_EventData">Current event data</param>
        /// <param name="p_ResultAppendList">List of hit Objects</param>
        public override void Raycast(PointerEventData p_EventData, List<RaycastResult> p_ResultAppendList)
        {
            /// Don't ray cast until the XRApplication is ready or ray casting is disabled
            if (!XRInputSystem.EnableRaycasting || !gameObject.activeInHierarchy)
                return;

            var l_Ray               = new Ray(p_EventData.pointerCurrentRaycast.worldPosition, p_EventData.pointerCurrentRaycast.worldNormal);
            var l_HitDistance       = float.MaxValue;
            var l_SortingLayerID    = m_Canvas.sortingLayerID;
            var l_SortingOrder      = m_Canvas.sortingOrder;

            RaycastResult l_RaycastResult;
            if (m_PhysicsRaycaster.DoRaycast(l_Ray, out var l_HitInfo, XRInputSystem.RaycastingUIMaxDistance, XRInputSystem.RaycastingUILayerMask))
            {
                l_HitDistance = l_HitInfo.distance;

                l_RaycastResult = new RaycastResult();
                l_RaycastResult.gameObject      = l_HitInfo.collider.gameObject;
                l_RaycastResult.module          = this;
                l_RaycastResult.distance        = l_HitDistance;
                l_RaycastResult.screenPosition  = new Vector2(float.MaxValue, float.MaxValue);
                l_RaycastResult.worldPosition   = l_HitInfo.point;
                l_RaycastResult.index           = (float)p_ResultAppendList.Count;
                l_RaycastResult.depth           = 0;
                l_RaycastResult.sortingLayer    = l_SortingLayerID;
                l_RaycastResult.sortingOrder    = l_SortingOrder;

                p_ResultAppendList.Add(l_RaycastResult);
            }

            var l_Hit2DCount = Physics2D.RaycastNonAlloc(l_Ray.origin, l_Ray.direction, m_Raycast2DBuffer, p_EventData.pointerCurrentRaycast.depth, XRInputSystem.RaycastingUILayerMask);
            if (l_Hit2DCount != 0 && m_Raycast2DBuffer[0].collider != null)
            {
                float l_Distance = m_Raycast2DBuffer[0].fraction * (float)p_EventData.pointerCurrentRaycast.depth;

                if ((double)l_Distance < (double)l_HitDistance)
                    l_HitDistance = l_Distance;
            }

            RaycastCanvas(l_Ray, l_HitDistance);

            var l_RaycastCount = m_RaycastResults.Count;
            var l_StartingIndex = p_ResultAppendList.Count;
            for (var l_I = 0; l_I < l_RaycastCount; l_I++)
            {
                var l_Current = m_RaycastResults[l_I];
                l_RaycastResult = new RaycastResult();
                l_RaycastResult.gameObject      = l_Current.HitGraphic.gameObject;
                l_RaycastResult.module          = this;
                l_RaycastResult.distance        = l_Current.HitDistance;
                l_RaycastResult.screenPosition  = l_Current.InsideRootCanvasHitPosition;
                l_RaycastResult.worldPosition   = l_Current.HitWorldPosition;
                l_RaycastResult.index           = (float)(l_StartingIndex + l_I);
                l_RaycastResult.depth           = l_Current.Depth;
                l_RaycastResult.sortingLayer    = l_SortingLayerID;
                l_RaycastResult.sortingOrder    = l_SortingOrder;

                p_ResultAppendList.Add(l_RaycastResult);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raycast all graphics on a canvas
        /// </summary>
        /// <param name="p_Ray">Ray casting ray</param>
        /// <param name="p_HitDistance">Previous hit distance</param>
        private void RaycastCanvas(Ray p_Ray, float p_HitDistance)
        {
            /// Clear previous results
            m_RaycastResults.Clear();

            var l_CanvasTransform   = m_Canvas.transform as RectTransform;
            var l_CanvasPosition    = l_CanvasTransform.position;
            var l_CanvasForward     = l_CanvasTransform.forward;

            float l_Distance;
            Vector3 l_CanvasHitLocalPosition;
            Vector3 l_RootCanvasHitLocalPosition;

            l_Distance = Vector3.Dot(l_CanvasForward, l_CanvasPosition - p_Ray.origin) / Vector3.Dot(l_CanvasForward, p_Ray.direction);

            if ((double)l_Distance < 0.0 || (double)l_Distance >= (double)p_HitDistance)
                return;

            l_CanvasHitLocalPosition        = p_Ray.GetPoint(l_Distance);
            l_RootCanvasHitLocalPosition    = l_CanvasHitLocalPosition;

            /// Iterate over all canvas graphics
            var l_CanvasGraphics    = GraphicRegistry.GetGraphicsForCanvas(m_Canvas);
            var l_GraphicsCount     = l_CanvasGraphics.Count;

            for (var l_I = 0; l_I < l_GraphicsCount; ++l_I)
            {
                var l_CurrentGraphic = l_CanvasGraphics[l_I];

                /// Make sure that casting is enabled on this graphic
                if (!l_CurrentGraphic.raycastTarget)
                    continue;

                var l_Depth = l_CurrentGraphic.depth;
                if (l_CurrentGraphic.depth == -1)
                    continue;

                var l_GraphicRectTranform = (RectTransform)l_CurrentGraphic.transform;
                var l_GraphicLocalHitPosition = l_GraphicRectTranform.InverseTransformPoint(l_RootCanvasHitLocalPosition);

                if (!l_GraphicRectTranform.rect.Contains(l_GraphicLocalHitPosition))
                    continue;

                if (l_CurrentGraphic.Raycast(l_CanvasHitLocalPosition, eventCamera))
                {
                    m_RaycastResults.Add(new XRGraphicRaycastResult()
                    {
                        HitGraphic                  = l_CurrentGraphic,
                        HitDistance                 = l_Distance,
                        HitWorldPosition            = l_CanvasHitLocalPosition,
                        InsideRootCanvasHitPosition = l_RootCanvasHitLocalPosition,
                        Depth                       = l_Depth
                    });
                }
            }

            /// Sort hits by depth
            m_RaycastResults.Sort((p_Left, p_Right) => p_Right.Depth.CompareTo(p_Left.Depth));
        }
    }
}
#endif