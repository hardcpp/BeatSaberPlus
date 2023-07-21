using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// IXUIElement with children abstract class
    /// </summary>
    /// <typeparam name="t_Base">Element type</typeparam>
    public abstract class IXUIElementWithChilds<t_Base>
        : IXUIElement
        where t_Base : IXUIElement
    {
        /// <summary>
        /// Child elements
        /// </summary>
        protected readonly IXUIElement[] m_Childs = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        protected IXUIElementWithChilds(string p_Name, params IXUIElement[] p_Childs)
            : base(p_Name)
        {
            m_Childs = p_Childs;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for children into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        protected void BuildUIChilds(Transform p_Parent)
        {
            if (m_Childs == null || m_Childs.Length == 0)
                return;

            for (var l_I = 0; l_I < m_Childs.Length; ++l_I)
                m_Childs[l_I].BuildUI(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// For each direct XUIElement child of type t_ChildType
        /// </summary>
        /// <typeparam name="t_ChildType">XUIElement type</typeparam>
        /// <param name="p_Functor">Functor callback</param>
        /// <returns></returns>
        public t_Base ForEachDirect<t_ChildType>(Action<t_ChildType> p_Functor)
        {
            for (var l_I = 0; l_I < m_Childs.Length; ++l_I)
            {
                if (!(m_Childs[l_I] is t_ChildType l_Casted))
                    continue;

                p_Functor(l_Casted);
            }

            return this as t_Base;
        }
    }
}
