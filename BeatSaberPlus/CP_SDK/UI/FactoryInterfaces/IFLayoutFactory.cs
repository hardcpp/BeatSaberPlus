using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CFLayout factory
    /// </summary>
    public interface IFLayoutFactory
    {
        /// <summary>
        /// Create an CFLayout into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CFLayout Create(string p_Name, Transform p_Parent);
    }
}
