using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    /// <summary>
    /// Chat request settings filters right screen
    /// </summary>
    internal class SettingsRight : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if (p_FirstActivation)
            {
                var l_Event       = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged),            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
                var l_NewRectMin  = new Vector2(0.71f, -0.05f);
                var l_NewRectMax  = new Vector2(0.90f, 1.05f);

                /// Left
                Utils.GameUI.PrepareToggleSetting(m_NoBeatSageToggle,   l_Event, Config.ChatRequest.NoBeatSage,     true);
                Utils.GameUI.PrepareToggleSetting(m_NPSMinToggle,       l_Event, Config.ChatRequest.NPSMin,         true);
                Utils.GameUI.PrepareToggleSetting(m_NPSMaxToggle,       l_Event, Config.ChatRequest.NPSMax,         true);
                Utils.GameUI.PrepareToggleSetting(m_NJSMinToggle,       l_Event, Config.ChatRequest.NJSMin,         true);
                Utils.GameUI.PrepareToggleSetting(m_NJSMaxToggle,       l_Event, Config.ChatRequest.NJSMax,         true);
                Utils.GameUI.PrepareToggleSetting(m_DurationMaxToggle,  l_Event, Config.ChatRequest.DurationMax,    true);
                Utils.GameUI.PrepareToggleSetting(m_VoteMinToggle,      l_Event, Config.ChatRequest.VoteMin,        true);
                Utils.GameUI.PrepareToggleSetting(m_DateMinToggle,      l_Event, Config.ChatRequest.DateMin,        true);
                Utils.GameUI.PrepareToggleSetting(m_DateMaxToggle,      l_Event, Config.ChatRequest.DateMax,        true);

                /// Change value type
                if (Config.ChatRequest.VoteMinV > 1)
                    Config.ChatRequest.VoteMinV /= 100f;

                /// Right
                Utils.GameUI.PrepareSliderSetting(m_NPSMin,         l_Event, null,                                      Config.ChatRequest.NPSMinV,         true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_NPSMax,         l_Event, null,                                      Config.ChatRequest.NPSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_NJSMin,         l_Event, null,                                      Config.ChatRequest.NJSMinV,         true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_NJSMax,         l_Event, null,                                      Config.ChatRequest.NJSMaxV,         true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_DurationMax,    l_Event, Utils.GameUI.Formatter_Minutes,            Config.ChatRequest.DurationMaxV,    true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_VoteMin,        l_Event, Utils.GameUI.Formatter_Percentage,         Config.ChatRequest.VoteMinV,        true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_DateMin,        l_Event, Utils.GameUI.Formatter_DateMonthFrom2018,  Config.ChatRequest.DateMinV,        true, true, l_NewRectMin, l_NewRectMax);
                Utils.GameUI.PrepareSliderSetting(m_DateMax,        l_Event, Utils.GameUI.Formatter_DateMonthFrom2018,  Config.ChatRequest.DateMaxV,        true, true, l_NewRectMin, l_NewRectMax);

                /// Update interactable
                Utils.GameUI.SetSliderInteractable(m_NPSMin,        m_NPSMinToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_NPSMax,        m_NPSMaxToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_NJSMin,        m_NJSMinToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_NJSMax,        m_NJSMaxToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_DurationMax,   m_DurationMaxToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_VoteMin,       m_VoteMinToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_DateMin,       m_DateMinToggle.Value);
                Utils.GameUI.SetSliderInteractable(m_DateMax,       m_DateMaxToggle.Value);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            /// Update interactable
            Utils.GameUI.SetSliderInteractable(m_NPSMin,       m_NPSMinToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_NPSMax,       m_NPSMaxToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_NJSMin,       m_NJSMinToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_NJSMax,       m_NJSMaxToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_DurationMax,  m_DurationMaxToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_VoteMin,      m_VoteMinToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_DateMin,      m_DateMinToggle.Value);
            Utils.GameUI.SetSliderInteractable(m_DateMax,      m_DateMaxToggle.Value);

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
    }
}
