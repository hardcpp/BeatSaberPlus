using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CToggle factory
    /// </summary>
    public interface IToggleFactory
    {
        /// <summary>
        /// Create an CToggle into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CToggle Create(string p_Name, Transform p_Parent);
    }
}
