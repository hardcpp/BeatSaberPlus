using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CGLayout factory
    /// </summary>
    public interface IGLayoutFactory
    {
        /// <summary>
        /// Create an CGLayout into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CGLayout Create(string p_Name, Transform p_Parent);
    }
}
