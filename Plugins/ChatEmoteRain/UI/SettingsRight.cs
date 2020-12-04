using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatEmoteRain.UI
{
    /// <summary>
    /// Chat Emote Rain settings right view
    /// </summary>
    internal class SettingsRight : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

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
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIObject("SubRainPanel")]
        private GameObject m_SubRainPanel;
        [UIObject("ComboModePanel")]
        private GameObject m_ComboModePanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// Subrain Settings
        [UIComponent("SubRainToggle")]
        private ToggleSetting m_SubRain;
        [UIComponent("SubRainEmoteCountSlider")]
        private SliderSetting m_SubRainEmoteCount;
        [UIObject("InfoBackground")]
        private GameObject m_InfoBackground;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// Combo Settings
        [UIObject("infobg2")]
        internal GameObject m_InfoBackground2;
        [UIComponent("combo-mode")]
        public ToggleSetting m_ComboMode;
        [UIValue("combo-mode-type-choices")]
        private List<object> m_ComboModeTypes = new List<object>()
        {
            "Emote count trigger",
            "User count trigger"
        };
        [UIValue("combo-mode-type-choice")]
        private string m_ComboModeTypeValue;
        [UIComponent("combo-mode-type")]
        public ListSetting m_ComboModeType;
        [UIComponent("combo-timer")]
        public SliderSetting m_ComboTimer;
        [UIComponent("combo-count")]
        public SliderSetting m_ComboCount;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;

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
            if (p_FirstActivation)
            {
                int l_TypeIndex = Config.ChatEmoteRain.ComboModeType % m_ComboModeTypes.Count;
                if (l_TypeIndex >= 0)
                    m_ComboModeTypeValue = m_ComboModeTypes[l_TypeIndex] as string;
                else
                {
                    Config.ChatEmoteRain.ComboModeType = 0;
                    m_ComboModeTypeValue = m_ComboModeTypes[0] as string;
                }
            }

            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if (p_FirstActivation)
            {
                /// Create type selector
                m_TypeSegmentControl = BeatSaberPlus.Utils.GameUI.CreateTextSegmentedControl(m_TypeSegmentPanel.transform as RectTransform, false);
                m_TypeSegmentControl.SetTexts(new string[] { "SubRain", "ComboMode" });
                m_TypeSegmentControl.ReloadData();
                m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

                /// Update background color
                Color l_Color = this.m_InfoBackground.GetComponent<ImageView>().color;
                l_Color.a = 0.5f;

                m_InfoBackground.GetComponent<ImageView>().color = l_Color;
                m_InfoBackground2.GetComponent<ImageView>().color = l_Color;

                /// Create event
                var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Set values
                Utils.GameUI.PrepareToggleSetting(m_SubRain,            l_Event,        Config.ChatEmoteRain.SubRain,           false);
                Utils.GameUI.PrepareSliderSetting(m_SubRainEmoteCount,  l_Event, null,  Config.ChatEmoteRain.SubRainEmoteCount, false);
                Utils.GameUI.PrepareToggleSetting(m_ComboMode,          l_Event,        Config.ChatEmoteRain.ComboMode,         false);
                Utils.GameUI.PrepareSliderSetting(m_ComboTimer,         l_Event, null,  Config.ChatEmoteRain.ComboTimer,        false);
                Utils.GameUI.PrepareSliderSetting(m_ComboCount,         l_Event, null,  Config.ChatEmoteRain.ComboCount,        false);

                /// Bind events
                m_ComboModeType.onChange        = l_Event;

                /// Force change to tab SubRain
                OnTypeChanged(null, 0);
            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemDisabling">Is screen system disabling</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            /// Close modals
            m_ParserParams.EmitEvent("CloseAllModals");
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
        /// When the type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnTypeChanged(SegmentedControl p_Sender, int p_Index)
        {
            m_SubRainPanel.SetActive(p_Index == 0);
            m_ComboModePanel.SetActive(p_Index == 1);
        }
        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            /// Update config
            Config.ChatEmoteRain.SubRain            = m_SubRain.Value;
            Config.ChatEmoteRain.SubRainEmoteCount  = (int)m_SubRainEmoteCount.slider.value;
            Config.ChatEmoteRain.ComboMode          = m_ComboMode.Value;
            Config.ChatEmoteRain.ComboModeType      = m_ComboModeTypes.IndexOf(m_ComboModeType.Value as string);
            Config.ChatEmoteRain.ComboTimer         = m_ComboTimer.slider.value;
            Config.ChatEmoteRain.ComboCount         = (int)m_ComboCount.slider.value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On reload button pressed
        /// </summary>
        [UIAction("click-reload-subrain-btn-pressed")]
        private void OnReloadSubRainButton()
        {
            /// Reload sub rain
            ChatEmoteRain.Instance.LoadSubRainFiles();

            /// Show message
            ShowMessageModal("SubRain textures were reloaded!");
        }
        /// <summary>
        /// On test button pressed
        /// </summary>
        [UIAction("click-test-subrain-btn-pressed")]
        private void OnTestSubRainButton()
        {
            ChatEmoteRain.Instance.StartSubRain();
        }
    }
}
