using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CSecondaryButton factory
    /// </summary>
    public interface ISecondaryButtonFactory
    {
        /// <summary>
        /// Create an CSecondaryButton into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CSecondaryButton Create(string p_Name, Transform p_Parent);
    }
}
