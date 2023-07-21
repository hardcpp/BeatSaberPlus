using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CSlider factory
    /// </summary>
    public interface ISliderFactory
    {
        /// <summary>
        /// Create an CSlider into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CSlider Create(string p_Name, Transform p_Parent);
    }
}
