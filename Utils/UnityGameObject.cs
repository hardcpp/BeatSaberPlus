using System.Text;
using UnityEngine;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// Unity GameObject extensions
    /// </summary>
    public static class UnityGameObject
    {
        /// <summary>
        /// Returns the full path of a GameObject in the scene hierarchy.
        /// </summary>
        /// <param name="p_GameObject">The instance of a GameObject to generate a path for.</param>
        /// <returns></returns>
        public static string GetFullPath(this GameObject p_GameObject)
        {
            StringBuilder l_Path = new StringBuilder();
            while (true)
            {
                l_Path.Insert(0, "/" + p_GameObject.name);
                if (p_GameObject.transform.parent == null)
                {
                    l_Path.Insert(0, p_GameObject.scene.name);
                    break;
                }

                p_GameObject = p_GameObject.transform.parent.gameObject;
            }
            return l_Path.ToString();
        }
        /// <summary>
        /// Returns the full path of a Component in the scene hierarchy.
        /// </summary>
        /// <param name="p_Component">The instance of a Component to generate a path for.</param>
        /// <returns></returns>
        public static string GetFullPath(this Component p_Component)
        {
            StringBuilder l_Path = new StringBuilder(p_Component.gameObject.GetFullPath());
            l_Path.Append("/" + p_Component.GetType().Name);

            return l_Path.ToString();
        }
    }
}
