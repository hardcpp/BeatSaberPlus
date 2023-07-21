using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CHLayout factory
    /// </summary>
    public interface IHLayoutFactory
    {
        /// <summary>
        /// Create an CHLayout into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CHLayout Create(string p_Name, Transform p_Parent);
    }
}
