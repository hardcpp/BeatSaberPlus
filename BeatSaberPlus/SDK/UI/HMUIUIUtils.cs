using IPA.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// View controller utils
    /// </summary>
    public static class HMUIUIUtils
    {
        private static MainFlowCoordinator              m_MainFlowCoordinator;
        private static Canvas                           m_CanvasTemplate;
        private static PhysicsRaycasterWithCache        m_PhysicsRaycaster;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static MainFlowCoordinator MainFlowCoordinator { get {
            if (m_MainFlowCoordinator)
                return m_MainFlowCoordinator;

            m_MainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();

            return m_MainFlowCoordinator;
        } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a flow coordinator
        /// </summary>
        /// <typeparam name="t_Base">Flow coordinator type</typeparam>
        /// <returns></returns>
        public static t_Base CreateFlowCoordinator<t_Base>()
            where t_Base : HMUI.FlowCoordinator
        {
            if (m_MainFlowCoordinator == null)
                m_MainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();

            var l_InputModule = m_MainFlowCoordinator._baseInputModule;
            var l_Coordinator = new GameObject(typeof(t_Base).Name).AddComponent<t_Base>();
            l_Coordinator.SetField<HMUI.FlowCoordinator, BaseInputModule>("_baseInputModule", l_InputModule);

            return l_Coordinator;
        }
        /// <summary>
        /// Create a view controller
        /// </summary>
        /// <typeparam name="t_Base">View controller type</typeparam>
        /// <returns></returns>
        public static t_Base CreateViewController<t_Base>()
            where t_Base : HMUI.ViewController
        {
            if (m_CanvasTemplate == null)
                m_CanvasTemplate = Resources.FindObjectsOfTypeAll<Canvas>().First((x) => x.name == "DropdownTableView");

            if (m_PhysicsRaycaster == null)
            {
                m_PhysicsRaycaster = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First().GetComponent<VRGraphicRaycaster>()
                    ._physicsRaycaster;
            }

            var l_GameObject = new GameObject(typeof(t_Base).Name);
            var l_Canvas     = l_GameObject.AddComponent<Canvas>();
            l_Canvas.renderMode                 = m_CanvasTemplate.renderMode;
            l_Canvas.scaleFactor                = m_CanvasTemplate.scaleFactor;
            l_Canvas.referencePixelsPerUnit     = m_CanvasTemplate.referencePixelsPerUnit;
            l_Canvas.overridePixelPerfect       = m_CanvasTemplate.overridePixelPerfect;
            l_Canvas.pixelPerfect               = m_CanvasTemplate.pixelPerfect;
            l_Canvas.planeDistance              = m_CanvasTemplate.planeDistance;
            l_Canvas.overrideSorting            = m_CanvasTemplate.overrideSorting;
            l_Canvas.sortingOrder               = m_CanvasTemplate.sortingOrder;
            l_Canvas.targetDisplay              = m_CanvasTemplate.targetDisplay;
            l_Canvas.sortingLayerID             = m_CanvasTemplate.sortingLayerID;
            l_Canvas.additionalShaderChannels   = m_CanvasTemplate.additionalShaderChannels;
            l_Canvas.sortingLayerName           = m_CanvasTemplate.sortingLayerName;
            l_Canvas.worldCamera                = m_CanvasTemplate.worldCamera;
            l_Canvas.normalizedSortingGridSize  = m_CanvasTemplate.normalizedSortingGridSize;

            l_GameObject.gameObject.AddComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", m_PhysicsRaycaster);
            l_GameObject.gameObject.AddComponent<CanvasGroup>();

            var l_View = l_GameObject.AddComponent<t_Base>();
            l_View.rectTransform.anchorMin          = new Vector2(0.0f, 0.0f);
            l_View.rectTransform.anchorMax          = new Vector2(1.0f, 1.0f);
            l_View.rectTransform.sizeDelta          = new Vector2(0.0f, 0.0f);
            l_View.rectTransform.anchoredPosition   = new Vector2(0.0f, 0.0f);
            l_View.gameObject.SetActive(false);

            return l_View;
        }
    }
}
