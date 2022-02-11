namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Unity GameObject tools
    /// </summary>
    public class GameObject
    {
        /// <summary>
        /// Change the layer of a GameObject and all his childs
        /// </summary>
        /// <param name="p_Object">Root object</param>
        /// <param name="p_Layer">Target layer</param>
        public static void ChangerLayerRecursive(UnityEngine.GameObject p_Object, int p_Layer)
        {
            if (!p_Object)
                return;

            p_Object.layer = p_Layer;

            foreach (UnityEngine.Transform l_Transform in p_Object.transform)
            {
                if (l_Transform.gameObject)
                    ChangerLayerRecursive(l_Transform.gameObject, p_Layer);
            }
        }
    }
}
