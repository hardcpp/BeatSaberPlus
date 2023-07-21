using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CPrimaryButton factory
    /// </summary>
    public interface IPrimaryButtonFactory
    {
        /// <summary>
        /// Create an CPrimaryButton into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CPrimaryButton Create(string p_Name, Transform p_Parent);
    }
}
