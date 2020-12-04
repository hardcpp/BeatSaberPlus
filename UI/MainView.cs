using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Main view controller
    /// </summary>
    internal class MainView : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button creator
        /// </summary>
        private BeatSaberMarkupLanguage.Tags.ButtonTag m_ButtonCreator = null;
        /// <summary>
        /// Plugin buttons
        /// </summary>
        private Dictionary<Plugins.PluginBase, Button> m_PluginsButton = new Dictionary<Plugins.PluginBase, Button>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("ButtonGrid")]
        public GameObject m_ButtonGrid;
        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// Initial setup
            if (p_FirstActivation)
            {
                m_ButtonCreator = new BeatSaberMarkupLanguage.Tags.ButtonTag();

                foreach (var l_Plugin in Plugin.Instance.Plugins)
                {
                    var l_Button = AddButton(l_Plugin.Name, () => { l_Plugin.ShowUI(); });
                    m_PluginsButton.Add(l_Plugin, l_Button);
                }
            }

            /// Refresh button states
            foreach (KeyValuePair<Plugins.PluginBase, Button> l_Current in m_PluginsButton)
                l_Current.Value.interactable = l_Current.Key.IsEnabled;

            /// Show welcome message
            if (Config.FirstRun)
            {
                StartCoroutine(ShowTutorialMessage());
                Config.FirstRun = false;
            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            m_ParserParams.EmitEvent("CloseAllModals");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the tutorial message
        /// </summary>
        /// <returns></returns>
        IEnumerator ShowTutorialMessage()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !isInTransition);

            if (!isInViewControllerHierarchy)
                yield break;

            ShowMessageModal("Welcome to BeatSaberPlus!\nBy default all modules are disabled, you can enable/disable them any time\nby clicking the Settings button below");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to settings
        /// </summary>
        [UIAction("click-btn-settings")]
        private void OnSettingsPressed()
        {
            ViewFlowCoordinator.Instance.SwitchToSettingsView();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a button to the grid
        /// </summary>
        /// <param name="p_Name">Button caption</param>
        /// <param name="p_Action">Button callback</param>
        private Button AddButton(string p_Name, Action p_Action)
        {
            var l_Button = m_ButtonCreator.CreateObject(m_ButtonGrid.transform);
            l_Button.GetComponentInChildren<TextMeshProUGUI>().text = p_Name;
            l_Button.GetComponent<Button>().onClick.RemoveAllListeners();
            l_Button.GetComponent<Button>().onClick.AddListener(() => p_Action());
            l_Button.GetComponent<LayoutElement>().preferredWidth = 35f;
            l_Button.GetComponent<LayoutElement>().preferredHeight = 10f;

            HoverHint l_HoverHint = l_Button.gameObject.AddComponent<HoverHint>();
            l_HoverHint.text = p_Name;
            l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HoverHintController>().First());

            return l_Button.GetComponent<Button>();
        }
    }
}
