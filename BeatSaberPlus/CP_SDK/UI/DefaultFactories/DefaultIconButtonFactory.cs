using UnityEngine;

namespace CP_SDK.UI.DefaultFactories
{
    /// <summary>
    /// Default CIconButton factory
    /// </summary>
    public class DefaultIconButtonFactory : FactoryInterfaces.IIconButtonFactory
    {
        /// <summary>
        /// Create an CIconButton into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        public Components.CIconButton Create(string p_Name, Transform p_Parent)
        {
            var l_GameObject = new GameObject(p_Name);
            l_GameObject.transform.SetParent(p_Parent, false);
            l_GameObject.AddComponent(typeof(RectTransform));

            var l_Element = l_GameObject.AddComponent<DefaultComponents.DefaultCIconButton>();
            l_Element.Init();

            return l_Element;
        }
    }
}
