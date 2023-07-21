using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// IXUIElement OnReady functor interface
    /// </summary>
    /// <typeparam name="t_Base">Return type for daisy chaining</typeparam>
    /// <typeparam name="t_Component">Element type</typeparam>
    public interface IXUIElementReady<t_Base, t_Component>
        where t_Base      : IXUIElement
        where t_Component : MonoBehaviour
    {
        /// <summary>
        /// On ready, append callback functor
        /// </summary>
        /// <param name="p_Functor">Functor to add</param>
        /// <returns></returns>
        t_Base OnReady(Action<t_Component> p_Functor);
    }
}
