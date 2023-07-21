using UnityEngine;

namespace CP_SDK.Unity.Extensions
{
    /// <summary>
    /// Unity GameObject tools
    /// </summary>
    public static class GameObjectU
    {
        /// <summary>
        /// Change the layer of a GameObject and all his childs
        /// </summary>
        /// <param name="p_This">Source object</param>
        /// <param name="p_Layer">Target layer</param>
        public static void ChangerLayerRecursive(this GameObject p_This, int p_Layer)
        {
            if (!p_This)
                return;

            p_This.layer = p_Layer;

            foreach (Transform l_Transform in p_This.transform)
            {
                if (l_Transform.gameObject)
                    ChangerLayerRecursive(l_Transform.gameObject, p_Layer);
            }
        }
        /// <summary>
        /// Find common root between 2 game objects
        /// </summary>
        /// <param name="p_Left"></param>
        /// <param name="p_Right"></param>
        /// <returns></returns>
        public static GameObject FindCommonRoot(this GameObject p_Left, GameObject p_Right)
        {
            if (p_Left == null || p_Right == null)
                return null;

            var l_LeftTransform = p_Left.transform;
            while (l_LeftTransform != null)
            {
                var l_RightTransform = p_Right.transform;
                while (l_RightTransform != null)
                {
                    if (l_LeftTransform == l_RightTransform)
                        return l_LeftTransform.gameObject;

                    l_RightTransform = l_RightTransform.parent;
                }

                l_LeftTransform = l_LeftTransform.parent;
            }

            return null;
        }
        /// <summary>
        /// Destroy child gameobjects
        /// </summary>
        /// <param name="p_This"></param>
        public static void DestroyChilds(this GameObject p_This)
        {
            if (!p_This)
                return;

            var l_ChildCount = p_This.transform.childCount;
            for (var l_I = (l_ChildCount - 1); l_I >= 0; l_I--)
            {
                GameObject.Destroy(p_This.transform.GetChild(l_I).gameObject);
            }
        }
    }
}
