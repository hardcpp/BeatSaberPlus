using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using UnityEngine;

namespace ChatPlexMod_ChatEmoteRain.UI
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
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Create event
            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_AnchorMin = new Vector2(0.15f, -0.05f);
            var l_AnchorMax = new Vector2(0.85f, 1.05f);

            /// Create type selector
            m_TypeSegmentControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false, new string[] { "SubRain" });
            m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

            /// SubRain panel
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_SubRainPanel_InfoBackground, 0.5f);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SubRainPanel_EnableToggle,         l_Event,         CERConfig.Instance.SubRain,           true);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SubRainPanel_EmoteCountSlider,     l_Event, null,   CERConfig.Instance.SubRainEmoteCount, true, true, l_AnchorMin, l_AnchorMax);

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
