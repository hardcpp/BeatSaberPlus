using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CImage factory
    /// </summary>
    public interface IImageFactory
    {
        /// <summary>
        /// Create an CImage into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CImage Create(string p_Name, Transform p_Parent);
    }
}
