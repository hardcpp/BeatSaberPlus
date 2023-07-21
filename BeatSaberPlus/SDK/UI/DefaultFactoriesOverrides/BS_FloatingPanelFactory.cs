using UnityEngine;

namespace BeatSaberPlus.SDK.UI.DefaultFactoriesOverrides
{
    /// <summary>
    /// BeatSaber CFloatingPanel factory
    /// </summary>
    public class BS_FloatingPanelFactory : CP_SDK.UI.FactoryInterfaces.IFloatingPanelFactory
    {
        /// <summary>
        /// Create an CFloatingPanel into the parent
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Parent">Parent transform</param>
        /// <returns></returns>
        public CP_SDK.UI.Components.CFloatingPanel Create(string p_Name, Transform p_Parent)
        {
            var l_GameObject = new GameObject(p_Name, typeof(RectTransform));
            l_GameObject.transform.SetParent(p_Parent, false);

            var l_Element = l_GameObject.AddComponent<DefaultComponentsOverrides.BS_CFloatingPanel>();
            l_Element.Init();

            return l_Element;
        }
    }
}
