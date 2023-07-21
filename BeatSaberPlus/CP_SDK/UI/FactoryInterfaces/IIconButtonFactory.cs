using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CIconButton factory
    /// </summary>
    public interface IIconButtonFactory
    {
        /// <summary>
        /// Create an CIconButton into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CIconButton Create(string p_Name, Transform p_Parent);
    }
}
