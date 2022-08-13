#if CP_SDK_UNITY
using UnityEngine;

namespace CP_SDK.Unity.Extensions
{
    /// <summary>
    /// Unity Color tools
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Get color with alpha
        /// </summary>
        /// <param name="p_This">Source color</param>
        /// <param name="p_Alpha">Target alpha</param>
        /// <returns></returns>
        public static Color WithAlpha(this Color p_This, float p_Alpha)
        {
            p_This.a = p_Alpha;
            return p_This;
        }
    }
}
#endif
