using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CText factory
    /// </summary>
    public interface ITextFactory
    {
        /// <summary>
        /// Create an CText into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CText Create(string p_Name, Transform p_Parent);
    }
}
