using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CTextInput factory
    /// </summary>
    public interface ITextInputFactory
    {
        /// <summary>
        /// Create an CTextInput into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CTextInput Create(string p_Name, Transform p_Parent);
    }
}
