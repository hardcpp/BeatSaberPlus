using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CVVList factory
    /// </summary>
    public interface IVVListFactory
    {
        /// <summary>
        /// Create an CVVList into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CVVList Create(string p_Name, Transform p_Parent);
    }
}
