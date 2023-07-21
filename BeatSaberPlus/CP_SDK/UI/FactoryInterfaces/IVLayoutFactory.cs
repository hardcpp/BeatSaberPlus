using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CVLayout factory
    /// </summary>
    public interface IVLayoutFactory
    {
        /// <summary>
        /// Create an CVLayout into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CVLayout Create(string p_Name, Transform p_Parent);
    }
}
