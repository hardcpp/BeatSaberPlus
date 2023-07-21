using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CColorInput factory
    /// </summary>
    public interface IColorInput
    {
        /// <summary>
        /// Create an CColorInput into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CColorInput Create(string p_Name, Transform p_Parent);
    }
}
