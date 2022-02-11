using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_ChatEmoteRain.UI
{
    /// <summary>
    /// Chat Emote Rain settings right view
    /// </summary>
    internal class SettingsRight : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SubRainPanel")]
        private GameObject m_SubRainPanel;

        [UIComponent("SubRainPanel_EnableToggle")]
        private ToggleSetting m_SubRainPanel_EnableToggle;
        [UIComponent("SubRainPanel_EmoteCountSlider")]
        private SliderSetting m_SubRainPanel_EmoteCountSlider;
        [UIObject("SubRainPanel_InfoBackground")]
        private GameObject m_SubRainPanel_InfoBackground;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("ComboModePanel")]
        private GameObject m_ComboModePanel;

        [UIComponent("ComboModePanel_EnableToggle")]
        private ToggleSetting m_ComboModePanel_EnableToggle;
        [UIComponent("ComboModePanel_ComboTypeList")]
        private ListSetting m_ComboModePanel_ComboTypeList;
        [UIValue("ComboModePanel_ComboTypeList_Choices")]
        private List<object> m_ComboModePanel_ComboTypeList_Choices = new List<object>() { "Emote count trigger", "User count trigger" };
        [UIValue("ComboModePanel_ComboTypeList_Value")]
        private string m_ComboModePanel_ComboTypeList_Value;
        [UIComponent("ComboModePanel_ComboTimerSlider")]
        private SliderSetting m_ComboModePanel_ComboTimerSlider;
        [UIComponent("ComboModePanel_ComboCountSlider")]
        private SliderSetting m_ComboModePanel_ComboCountSlider;
        [UIObject("ComboModePanel_InfoBackground")]
        private GameObject m_ComboModePanel_InfoBackground;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;
        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        internal SettingsRight()
        {
            int l_TypeIndex = CERConfig.Instance.ComboModeType % m_ComboModePanel_ComboTypeList_Choices.Count;
            if (l_TypeIndex >= 0)
                m_ComboModePanel_ComboTypeList_Value = m_ComboModePanel_ComboTypeList_Choices[l_TypeIndex] as string;
            else
            {
                CERConfig.Instance.ComboModeType = 0;
                m_ComboModePanel_ComboTypeList_Value = m_ComboModePanel_ComboTypeList_Choices[0] as string;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Create event
            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_AnchorMin = new Vector2(0.15f, -0.05f);
            var l_AnchorMax = new Vector2(0.85f, 1.05f);

            /// Create type selector
            m_TypeSegmentControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false, new string[] { "SubRain", "ComboMode" });
            m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

            /// SubRain panel
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_SubRainPanel_InfoBackground, 0.5f);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SubRainPanel_EnableToggle,         l_Event,         CERConfig.Instance.SubRain,           true);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SubRainPanel_EmoteCountSlider,     l_Event, null,   CERConfig.Instance.SubRainEmoteCount, true, true, l_AnchorMin, l_AnchorMax);

            /// Combo panel
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ComboModePanel_InfoBackground, 0.5f);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ComboModePanel_EnableToggle,       l_Event,        CERConfig.Instance.ComboMode,         true);
            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_ComboModePanel_ComboTypeList,        l_Event,                                              true);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_ComboModePanel_ComboTimerSlider,   l_Event, null,  CERConfig.Instance.ComboTimer,        true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_ComboModePanel_ComboCountSlider,   l_Event, null,  CERConfig.Instance.ComboCount,        true, true, l_AnchorMin, l_AnchorMax);

            /// Force change to tab SubRain
            OnTypeChanged(null, 0);
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
            if (m_PreventChanges)
                return;

            /// SubRain panel
            CERConfig.Instance.SubRain            = m_SubRainPanel_EnableToggle.Value;
            CERConfig.Instance.SubRainEmoteCount  = (int)m_SubRainPanel_EmoteCountSlider.slider.value;

            /// Combo panel
            CERConfig.Instance.ComboMode          = m_ComboModePanel_EnableToggle.Value;
            CERConfig.Instance.ComboModeType      = m_ComboModePanel_ComboTypeList_Choices.IndexOf(m_ComboModePanel_ComboTypeList.Value as string);
            CERConfig.Instance.ComboTimer         = m_ComboModePanel_ComboTimerSlider.slider.value;
            CERConfig.Instance.ComboCount         = (int)m_ComboModePanel_ComboCountSlider.slider.value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// SubRain panel
            m_SubRainPanel_EnableToggle.Value               = CERConfig.Instance.SubRain;
            BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_SubRainPanel_EmoteCountSlider, CERConfig.Instance.SubRainEmoteCount);

            /// Combo panel
            m_ComboModePanel_EnableToggle.Value             = CERConfig.Instance.ComboMode;
            m_ComboModePanel_ComboTypeList.Value            = m_ComboModePanel_ComboTypeList_Choices[CERConfig.Instance.ComboModeType % m_ComboModePanel_ComboTypeList_Choices.Count];
            BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_ComboModePanel_ComboTimerSlider, CERConfig.Instance.ComboTimer);
            BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_ComboModePanel_ComboCountSlider, CERConfig.Instance.ComboCount);

            m_PreventChanges = false;
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
