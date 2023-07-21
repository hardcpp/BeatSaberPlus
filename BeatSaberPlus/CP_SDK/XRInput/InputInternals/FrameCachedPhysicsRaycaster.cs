#if CP_SDK_XR_INPUT
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.XRInput.InputInternals
{
    /// <summary>
    /// Physics ray caster with frame cache
    /// </summary>
    public class FrameCachedPhysicsRaycaster
    {
        /// <summary>
        /// Ray cast cache structure
        /// </summary>
        private struct FrameCachedPhysicsRaycasterResult
        {
            internal bool       wasHit;
            internal Ray        ray;
            internal RaycastHit hitInfo;
            internal float      maxDistance;
            internal int        layerMask;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Instance
        /// </summary>
        private static FrameCachedPhysicsRaycaster m_Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<FrameCachedPhysicsRaycasterResult> m_CachedRaycasts    = new List<FrameCachedPhysicsRaycasterResult>(100);
        private int                                     m_LastFrameCount    = -1;
        private RaycastHit[]                            m_RaycastBuffer     = new RaycastHit[1];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get instance
        /// </summary>
        /// <returns></returns>
        public static FrameCachedPhysicsRaycaster Instance()
        {
            if (m_Instance == null)
                m_Instance = new FrameCachedPhysicsRaycaster();

            return m_Instance;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do ray cast
        /// </summary>
        /// <param name="p_Ray">Source ray</param>
        /// <param name="p_HitInfo">Out result hit info</param>
        /// <param name="p_MaxDistance">Ray max distance</param>
        /// <param name="p_LayerMask">Layers to hit</param>
        /// <returns></returns>
        public bool DoRaycast(Ray p_Ray, out RaycastHit p_HitInfo, float p_MaxDistance, int p_LayerMask)
        {
            /// Handle cache
            if (Time.frameCount != m_LastFrameCount)
            {
                m_LastFrameCount = Time.frameCount;
                m_CachedRaycasts.Clear();
            }

            /// Lookup in cache
            var l_CachedRaycastsCount = m_CachedRaycasts.Count;
            for (var l_I = 0; l_I < l_CachedRaycastsCount; ++l_I)
            {
                var l_CurrentCache = m_CachedRaycasts[l_I];

                if (l_CurrentCache.layerMask != p_LayerMask
                    || l_CurrentCache.ray.origin    != p_Ray.origin
                    || l_CurrentCache.ray.direction != p_Ray.direction)
                    continue;

                if (Mathf.Approximately(l_CurrentCache.maxDistance, p_MaxDistance))
                {
                    p_HitInfo = l_CurrentCache.hitInfo;
                    return l_CurrentCache.wasHit;
                }
            }

            /// Do a new ray cast
            bool l_WasHit = Physics.RaycastNonAlloc(p_Ray, m_RaycastBuffer, p_MaxDistance, p_LayerMask) != 0;
            p_HitInfo = m_RaycastBuffer[0];

            m_CachedRaycasts.Add(new FrameCachedPhysicsRaycasterResult()
            {
                wasHit      = l_WasHit,
                ray         = p_Ray,
                hitInfo     = p_HitInfo,
                maxDistance = p_MaxDistance,
                layerMask   = p_LayerMask
            });

            return l_WasHit;
        }
    }
}
#endif