using UnityEngine;

namespace CP_SDK.UI.FactoryInterfaces
{
    /// <summary>
    /// CDropdown factory
    /// </summary>
    public interface IDropdownFactory
    {
        /// <summary>
        /// Create an CDropdown into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        Components.CDropdown Create(string p_Name, Transform p_Parent);
    }
}
