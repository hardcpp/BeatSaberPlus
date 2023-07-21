
using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CFloatingPanel factory
    /// </summary>
    public interface IFloatingPanelFactory
    {
        /// <summary>
        /// Create an CFloatingPanel into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CFloatingPanel Create(string p_Name, Transform p_Parent);
    }
}
