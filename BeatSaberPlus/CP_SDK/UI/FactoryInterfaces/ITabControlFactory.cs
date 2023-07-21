using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CTabControl factory
    /// </summary>
    public interface ITabControlFactory
    {
        /// <summary>
        /// Create an CTabControl into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CTabControl Create(string p_Name, Transform p_Parent);
    }
}
