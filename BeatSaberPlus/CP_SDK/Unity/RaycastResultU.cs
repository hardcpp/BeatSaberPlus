using System;
using UnityEngine.EventSystems;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Raycast utilities
    /// </summary>
    public static class RaycastResultU
    {
        /// <summary>
        /// Comparer instance
        /// </summary>
        public static Comparison<RaycastResult> Comparer { get; private set; }
            = new Comparison<RaycastResult>(DoComparer);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Compare two Raycast results
        /// </summary>
        /// <param name="p_Left">Left result</param>
        /// <param name="p_Right">Right result</param>
        /// <returns></returns>
        private static int DoComparer(RaycastResult p_Left, RaycastResult p_Right)
        {
            if (p_Left.module != p_Right.module)
            {
                var l_LeftCamera = p_Left.module.eventCamera;
                var l_RightCamera = p_Right.module.eventCamera;

                if (l_LeftCamera != null && l_RightCamera != null && l_LeftCamera.depth != l_RightCamera.depth)
                {
                    if (l_LeftCamera.depth < l_RightCamera.depth)
                        return 1;

                    return l_LeftCamera.depth == l_RightCamera.depth ? 0 : -1;
                }

                if (p_Left.module.sortOrderPriority != p_Right.module.sortOrderPriority)
                    return p_Right.module.sortOrderPriority.CompareTo(p_Left.module.sortOrderPriority);

                if (p_Left.module.renderOrderPriority != p_Right.module.renderOrderPriority)
                    return p_Right.module.renderOrderPriority.CompareTo(p_Left.module.renderOrderPriority);
            }

            if (p_Left.sortingLayer != p_Right.sortingLayer)
                return global::UnityEngine.SortingLayer.GetLayerValueFromID(p_Right.sortingLayer).CompareTo(global::UnityEngine.SortingLayer.GetLayerValueFromID(p_Left.sortingLayer));

            if (p_Left.sortingOrder != p_Right.sortingOrder)
                return p_Right.sortingOrder.CompareTo(p_Left.sortingOrder);

            if (p_Left.depth != p_Right.depth && p_Left.module.rootRaycaster == p_Right.module.rootRaycaster)
                return p_Right.depth.CompareTo(p_Left.depth);

            if (p_Left.distance != p_Right.distance)
                return p_Right.distance.CompareTo(p_Left.distance);

            return p_Left.index.CompareTo(p_Right.index);
        }
    }
}
