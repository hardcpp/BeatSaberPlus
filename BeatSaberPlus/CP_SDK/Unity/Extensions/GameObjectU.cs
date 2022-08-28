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
    }
}
