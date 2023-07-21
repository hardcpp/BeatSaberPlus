using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CTextSegmentedControl factory
    /// </summary>
    public interface ITextSegmentedControlFactory
    {
        /// <summary>
        /// Create an CTextSegmentedControl into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CTextSegmentedControl Create(string p_Name, Transform p_Parent);
    }
}
