using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CVScrollView factory
    /// </summary>
    public interface IVScrollViewFactory
    {
        /// <summary>
        /// Create an CVScrollView into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CVScrollView Create(string p_Name, Transform p_Parent);
    }
}
