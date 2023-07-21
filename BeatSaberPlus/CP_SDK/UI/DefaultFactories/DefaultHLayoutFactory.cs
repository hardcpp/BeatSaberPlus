using UnityEngine;

namespace CP_SDK.UI.DefaultFactories
{
    /// <summary>
    /// Default CHLayout factory
    /// </summary>
    public class DefaultHLayoutFactory : FactoryInterfaces.IHLayoutFactory
    {
        /// <summary>
        /// Create an CHLayout into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        public Components.CHLayout Create(string p_Name, Transform p_Parent)
        {
            var l_GameObject = new GameObject(p_Name);
            l_GameObject.transform.SetParent(p_Parent, false);
            l_GameObject.AddComponent(typeof(RectTransform));

            var l_Element = l_GameObject.AddComponent<DefaultComponents.DefaultCHLayout>();
            l_Element.Init();

            return l_Element;
        }
    }
}
