using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Chat request settings filters right screen
    /// </summary>
    internal class SettingsRight : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsRight>
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
            var l_NewRectMin  = new Vector2(0.10f, -0.05f);
            var l_NewRectMax  = new Vector2(0.92f, 1.05f);

            /// Left
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NoBeatSageToggle,      l_Event,                                                                  CRConfig.Instance.Filters.NoBeatSage,      true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NPSMinToggle,          l_Event,                                                                  CRConfig.Instance.Filters.NPSMin,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NPSMaxToggle,          l_Event,                                                                  CRConfig.Instance.Filters.NPSMax,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NJSMinToggle,          l_Event,                                                                  CRConfig.Instance.Filters.NJSMin,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NJSMaxToggle,          l_Event,                                                                  CRConfig.Instance.Filters.NJSMax,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DurationMaxToggle,     l_Event,                                                                  CRConfig.Instance.Filters.DurationMax,     true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_VoteMinToggle,         l_Event,                                                                  CRConfig.Instance.Filters.VoteMin,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DateMinToggle,         l_Event,                                                                  CRConfig.Instance.Filters.DateMin,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DateMaxToggle,         l_Event,                                                                  CRConfig.Instance.Filters.DateMax,         true);

            /// Right
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_NPSMin,                l_Event, null,                                                            CRConfig.Instance.Filters.NPSMinV,         true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_NPSMax,                l_Event, null,                                                            CRConfig.Instance.Filters.NPSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_NJSMin,                l_Event, null,                                                            CRConfig.Instance.Filters.NJSMinV,         true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_NJSMax,                l_Event, null,                                                            CRConfig.Instance.Filters.NJSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_DurationMax,           l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Minutes,              CRConfig.Instance.Filters.DurationMaxV,    true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_VoteMin,               l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,           CRConfig.Instance.Filters.VoteMinV,        true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_DateMin,               l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.DateMonthFrom2018,    CRConfig.Instance.Filters.DateMinV,        true, true, l_NewRectMin, l_NewRectMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_DateMax,               l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.DateMonthFrom2018,    CRConfig.Instance.Filters.DateMaxV,        true, true, l_NewRectMin, l_NewRectMax);

            /// Update interactable
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NPSMin,        m_NPSMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NPSMax,        m_NPSMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NJSMin,        m_NJSMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NJSMax,        m_NJSMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DurationMax,   m_DurationMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_VoteMin,       m_VoteMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DateMin,       m_DateMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DateMax,       m_DateMaxToggle.Value);
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
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NPSMin,       m_NPSMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NPSMax,       m_NPSMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NJSMin,       m_NJSMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_NJSMax,       m_NJSMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DurationMax,  m_DurationMaxToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_VoteMin,      m_VoteMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DateMin,      m_DateMinToggle.Value);
            BeatSaberPlus.SDK.UI.SliderSetting.SetInteractable(m_DateMax,      m_DateMaxToggle.Value);

            /// Left
            CRConfig.Instance.Filters.NoBeatSage   = m_NoBeatSageToggle.Value;
            CRConfig.Instance.Filters.NPSMin       = m_NPSMinToggle.Value;
            CRConfig.Instance.Filters.NPSMax       = m_NPSMaxToggle.Value;
            CRConfig.Instance.Filters.NJSMin       = m_NJSMinToggle.Value;
            CRConfig.Instance.Filters.NJSMax       = m_NJSMaxToggle.Value;
            CRConfig.Instance.Filters.DurationMax  = m_DurationMaxToggle.Value;
            CRConfig.Instance.Filters.VoteMin      = m_VoteMinToggle.Value;
            CRConfig.Instance.Filters.DateMin      = m_DateMinToggle.Value;
            CRConfig.Instance.Filters.DateMax      = m_DateMaxToggle.Value;

            /// Right
            CRConfig.Instance.Filters.NPSMinV       = (int)m_NPSMin.slider.value;
            CRConfig.Instance.Filters.NPSMaxV       = (int)m_NPSMax.slider.value;
            CRConfig.Instance.Filters.NJSMinV       = (int)m_NJSMin.slider.value;
            CRConfig.Instance.Filters.NJSMaxV       = (int)m_NJSMax.slider.value;
            CRConfig.Instance.Filters.DurationMaxV  = (int)m_DurationMax.slider.value;
            CRConfig.Instance.Filters.VoteMinV      = m_VoteMin.slider.value;
            CRConfig.Instance.Filters.DateMinV      = (int)m_DateMin.slider.value;
            CRConfig.Instance.Filters.DateMaxV      = (int)m_DateMax.slider.value;
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
            m_NoBeatSageToggle.Value    = CRConfig.Instance.Filters.NoBeatSage;
            m_NPSMinToggle.Value        = CRConfig.Instance.Filters.NPSMin;
            m_NPSMaxToggle.Value        = CRConfig.Instance.Filters.NPSMax;
            m_NJSMinToggle.Value        = CRConfig.Instance.Filters.NJSMin;
            m_NJSMaxToggle.Value        = CRConfig.Instance.Filters.NJSMax;
            m_DurationMaxToggle.Value   = CRConfig.Instance.Filters.DurationMax;
            m_VoteMinToggle.Value       = CRConfig.Instance.Filters.VoteMin;
            m_DateMinToggle.Value       = CRConfig.Instance.Filters.DateMin;
            m_DateMaxToggle.Value       = CRConfig.Instance.Filters.DateMax;

            /// Right
            m_NPSMin.slider.value       = CRConfig.Instance.Filters.NPSMinV;
            m_NPSMax.slider.value       = CRConfig.Instance.Filters.NPSMaxV;
            m_NJSMin.slider.value       = CRConfig.Instance.Filters.NJSMinV;
            m_NJSMax.slider.value       = CRConfig.Instance.Filters.NJSMaxV;
            m_DurationMax.slider.value  = CRConfig.Instance.Filters.DurationMaxV;
            m_VoteMin.slider.value      = CRConfig.Instance.Filters.VoteMinV;
            m_DateMin.slider.value      = CRConfig.Instance.Filters.DateMinV;
            m_DateMax.slider.value      = CRConfig.Instance.Filters.DateMaxV;

            m_PreventChanges = false;

            /// Update sliders
            OnSettingChanged(null);
        }
    }
}
