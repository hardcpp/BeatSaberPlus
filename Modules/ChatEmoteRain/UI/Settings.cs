using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatEmoteRain.UI
{
    /// <summary>
    /// Chat Emote Rain settings main view
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("MenuRainToggle")]
        public ToggleSetting m_MenuRain;
        [UIComponent("MenuRainSizeSlider")]
        public SliderSetting m_MenuRainSizeSlider;
        [UIComponent("MenuFallSpeedSlider")]
        public SliderSetting m_MenuFallSpeedSlider;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SongRainToggle")]
        public ToggleSetting m_SongRain;
        [UIComponent("SongRainSizeSlider")]
        public SliderSetting m_SongRainSizeSlider;
        [UIComponent("SongFallSpeedSlider")]
        public SliderSetting m_SongFallSpeedSlider;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("ModeratorPowerToggle")]
        public ToggleSetting m_ModeratorPowerToggle;
        [UIComponent("VIPPowerToggle")]
        public ToggleSetting m_VIPPowerToggle;
        [UIComponent("SubscriberPowerToggle")]
        public ToggleSetting m_SubscriberPowerToggle;
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
            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_AnchorMin = new Vector2(1.15f, -0.05f);
            var l_AnchorMax = new Vector2(0.85f, 1.05f);

            /// First row
            SDK.UI.ToggleSetting.Setup(m_MenuRain,              l_Event,        Config.ChatEmoteRain.MenuRain,          true);
            SDK.UI.SliderSetting.Setup(m_MenuRainSizeSlider,    l_Event, null,  Config.ChatEmoteRain.MenuRainSize,      true, true, l_AnchorMin, l_AnchorMax);
            SDK.UI.SliderSetting.Setup(m_MenuFallSpeedSlider,   l_Event, null,  Config.ChatEmoteRain.MenuFallSpeed,     true, true, l_AnchorMin, l_AnchorMax);

            /// Second row
            SDK.UI.ToggleSetting.Setup(m_SongRain,              l_Event,        Config.ChatEmoteRain.SongRain,          true);
            SDK.UI.SliderSetting.Setup(m_SongRainSizeSlider,    l_Event, null,  Config.ChatEmoteRain.SongRainSize,      true, true, l_AnchorMin, l_AnchorMax);
            SDK.UI.SliderSetting.Setup(m_SongFallSpeedSlider,   l_Event, null,  Config.ChatEmoteRain.SongFallSpeed,     true, true, l_AnchorMin, l_AnchorMax);

            /// Third row
            SDK.UI.ToggleSetting.Setup(m_ModeratorPowerToggle,  l_Event,        Config.ChatEmoteRain.ModeratorPower,    true);
            SDK.UI.ToggleSetting.Setup(m_VIPPowerToggle,        l_Event,        Config.ChatEmoteRain.VIPPower,          true);
            SDK.UI.ToggleSetting.Setup(m_SubscriberPowerToggle, l_Event,        Config.ChatEmoteRain.SubscriberPower,   true);
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

            /// First row
            Config.ChatEmoteRain.MenuRain           = m_MenuRain.Value;
            Config.ChatEmoteRain.MenuRainSize       = m_MenuRainSizeSlider.slider.value;
            Config.ChatEmoteRain.MenuFallSpeed      = m_MenuFallSpeedSlider.slider.value;

            /// Second row
            Config.ChatEmoteRain.SongRain           = m_SongRain.Value;
            Config.ChatEmoteRain.SongRainSize       = m_SongRainSizeSlider.slider.value;
            Config.ChatEmoteRain.SongFallSpeed      = m_SongFallSpeedSlider.slider.value;

            /// Third row
            Config.ChatEmoteRain.ModeratorPower     = m_ModeratorPowerToggle.Value;
            Config.ChatEmoteRain.VIPPower           = m_VIPPowerToggle.Value;
            Config.ChatEmoteRain.SubscriberPower    = m_SubscriberPowerToggle.Value;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// First row
            m_MenuRain.Value                = Config.ChatEmoteRain.MenuRain;
            SDK.UI.SliderSetting.SetValue(m_MenuRainSizeSlider,     Config.ChatEmoteRain.MenuRainSize);
            SDK.UI.SliderSetting.SetValue(m_MenuFallSpeedSlider,    Config.ChatEmoteRain.MenuFallSpeed);

            /// Second row
            m_SongRain.Value                = Config.ChatEmoteRain.SongRain;
            SDK.UI.SliderSetting.SetValue(m_SongRainSizeSlider,     Config.ChatEmoteRain.SongRainSize);
            SDK.UI.SliderSetting.SetValue(m_SongFallSpeedSlider,    Config.ChatEmoteRain.SongFallSpeed);

            /// Third row
            m_ModeratorPowerToggle.Value    = Config.ChatEmoteRain.ModeratorPower;
            m_VIPPowerToggle.Value          = Config.ChatEmoteRain.VIPPower;
            m_SubscriberPowerToggle.Value   = Config.ChatEmoteRain.SubscriberPower;

            m_PreventChanges = false;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }
    }
}
