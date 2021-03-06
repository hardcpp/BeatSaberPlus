﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatRequest.UI
{
    /// <summary>
    /// Chat request settings filters right screen
    /// </summary>
    internal class SettingsRight : SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIComponent("nobeatsage-toggle")]
        private ToggleSetting m_NoBeatSageToggle;
        [UIComponent("npsmin-toggle")]
        private ToggleSetting m_NPSMinToggle;
        [UIComponent("npsmax-toggle")]
        private ToggleSetting m_NPSMaxToggle;
        [UIComponent("njsmin-toggle")]
        private ToggleSetting m_NJSMinToggle;
        [UIComponent("njsmax-toggle")]
        private ToggleSetting m_NJSMaxToggle;
        [UIComponent("durationmax-toggle")]
        private ToggleSetting m_DurationMaxToggle;
        [UIComponent("votemin-toggle")]
        private ToggleSetting m_VoteMinToggle;
        [UIComponent("datemin-toggle")]
        private ToggleSetting m_DateMinToggle;
        [UIComponent("datemax-toggle")]
        private ToggleSetting m_DateMaxToggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("npsmin-slider")]
        private SliderSetting m_NPSMin;
        [UIComponent("npsmax-slider")]
        private SliderSetting m_NPSMax;
        [UIComponent("njsmin-slider")]
        private SliderSetting m_NJSMin;
        [UIComponent("njsmax-slider")]
        private SliderSetting m_NJSMax;
        [UIComponent("durationmax-slider")]
        private SliderSetting m_DurationMax;
        [UIComponent("votemin-slider")]
        private SliderSetting m_VoteMin;
        [UIComponent("datemin-slider")]
        private SliderSetting m_DateMin;
        [UIComponent("datemax-slider")]
        private SliderSetting m_DateMax;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event       = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged),            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_NewRectMin  = new Vector2(0.71f, -0.05f);
            var l_NewRectMax  = new Vector2(0.90f, 1.05f);

            /// Left
            SDK.UI.ToggleSetting.Setup(m_NoBeatSageToggle,      l_Event,                                                    Config.ChatRequest.NoBeatSage,      true);
            SDK.UI.ToggleSetting.Setup(m_NPSMinToggle,          l_Event,                                                    Config.ChatRequest.NPSMin,          true);
            SDK.UI.ToggleSetting.Setup(m_NPSMaxToggle,          l_Event,                                                    Config.ChatRequest.NPSMax,          true);
            SDK.UI.ToggleSetting.Setup(m_NJSMinToggle,          l_Event,                                                    Config.ChatRequest.NJSMin,          true);
            SDK.UI.ToggleSetting.Setup(m_NJSMaxToggle,          l_Event,                                                    Config.ChatRequest.NJSMax,          true);
            SDK.UI.ToggleSetting.Setup(m_DurationMaxToggle,     l_Event,                                                    Config.ChatRequest.DurationMax,     true);
            SDK.UI.ToggleSetting.Setup(m_VoteMinToggle,         l_Event,                                                    Config.ChatRequest.VoteMin,         true);
            SDK.UI.ToggleSetting.Setup(m_DateMinToggle,         l_Event,                                                    Config.ChatRequest.DateMin,         true);
            SDK.UI.ToggleSetting.Setup(m_DateMaxToggle,         l_Event,                                                    Config.ChatRequest.DateMax,         true);

            /// Right
            SDK.UI.SliderSetting.Setup(m_NPSMin,                l_Event, null,                                              Config.ChatRequest.NPSMinV,         true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_NPSMax,                l_Event, null,                                              Config.ChatRequest.NPSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_NJSMin,                l_Event, null,                                              Config.ChatRequest.NJSMinV,         true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_NJSMax,                l_Event, null,                                              Config.ChatRequest.NJSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_DurationMax,           l_Event, SDK.UI.BSMLSettingFormartter.Minutes,              Config.ChatRequest.DurationMaxV,    true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_VoteMin,               l_Event, SDK.UI.BSMLSettingFormartter.Percentage,           Config.ChatRequest.VoteMinV,        true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_DateMin,               l_Event, SDK.UI.BSMLSettingFormartter.DateMonthFrom2018,    Config.ChatRequest.DateMinV,        true, true, l_NewRectMin, l_NewRectMax);
            SDK.UI.SliderSetting.Setup(m_DateMax,               l_Event, SDK.UI.BSMLSettingFormartter.DateMonthFrom2018,    Config.ChatRequest.DateMaxV,        true, true, l_NewRectMin, l_NewRectMax);

            /// Update interactable
            SDK.UI.SliderSetting.SetInteractable(m_NPSMin,        m_NPSMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NPSMax,        m_NPSMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NJSMin,        m_NJSMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NJSMax,        m_NJSMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DurationMax,   m_DurationMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_VoteMin,       m_VoteMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DateMin,       m_DateMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DateMax,       m_DateMaxToggle.Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update interactable
            SDK.UI.SliderSetting.SetInteractable(m_NPSMin,       m_NPSMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NPSMax,       m_NPSMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NJSMin,       m_NJSMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_NJSMax,       m_NJSMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DurationMax,  m_DurationMaxToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_VoteMin,      m_VoteMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DateMin,      m_DateMinToggle.Value);
            SDK.UI.SliderSetting.SetInteractable(m_DateMax,      m_DateMaxToggle.Value);

            /// Left
            Config.ChatRequest.NoBeatSage   = m_NoBeatSageToggle.Value;
            Config.ChatRequest.NPSMin       = m_NPSMinToggle.Value;
            Config.ChatRequest.NPSMax       = m_NPSMaxToggle.Value;
            Config.ChatRequest.NJSMin       = m_NJSMinToggle.Value;
            Config.ChatRequest.NJSMax       = m_NJSMaxToggle.Value;
            Config.ChatRequest.DurationMax  = m_DurationMaxToggle.Value;
            Config.ChatRequest.VoteMin      = m_VoteMinToggle.Value;
            Config.ChatRequest.DateMin      = m_DateMinToggle.Value;
            Config.ChatRequest.DateMax      = m_DateMaxToggle.Value;

            /// Right
            Config.ChatRequest.NPSMinV       = (int)m_NPSMin.slider.value;
            Config.ChatRequest.NPSMaxV       = (int)m_NPSMax.slider.value;
            Config.ChatRequest.NJSMinV       = (int)m_NJSMin.slider.value;
            Config.ChatRequest.NJSMaxV       = (int)m_NJSMax.slider.value;
            Config.ChatRequest.DurationMaxV  = (int)m_DurationMax.slider.value;
            Config.ChatRequest.VoteMinV      = (int)m_VoteMin.slider.value;
            Config.ChatRequest.DateMinV      = (int)m_DateMin.slider.value;
            Config.ChatRequest.DateMaxV      = (int)m_DateMax.slider.value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Left
            m_NoBeatSageToggle.Value    = Config.ChatRequest.NoBeatSage;
            m_NPSMinToggle.Value        = Config.ChatRequest.NPSMin;
            m_NPSMaxToggle.Value        = Config.ChatRequest.NPSMax;
            m_NJSMinToggle.Value        = Config.ChatRequest.NJSMin;
            m_NJSMaxToggle.Value        = Config.ChatRequest.NJSMax;
            m_DurationMaxToggle.Value   = Config.ChatRequest.DurationMax;
            m_VoteMinToggle.Value       = Config.ChatRequest.VoteMin;
            m_DateMinToggle.Value       = Config.ChatRequest.DateMin;
            m_DateMaxToggle.Value       = Config.ChatRequest.DateMax;

            /// Right
            m_NPSMin.slider.value       = Config.ChatRequest.NPSMinV;
            m_NPSMax.slider.value       = Config.ChatRequest.NPSMaxV;
            m_NJSMin.slider.value       = Config.ChatRequest.NJSMinV;
            m_NJSMax.slider.value       = Config.ChatRequest.NJSMaxV;
            m_DurationMax.slider.value  = Config.ChatRequest.DurationMaxV;
            m_VoteMin.slider.value      = Config.ChatRequest.VoteMinV;
            m_DateMin.slider.value      = Config.ChatRequest.DateMinV;
            m_DateMax.slider.value      = Config.ChatRequest.DateMaxV;

            m_PreventChanges = false;

            /// Update sliders
            OnSettingChanged(null);
        }
    }
}
