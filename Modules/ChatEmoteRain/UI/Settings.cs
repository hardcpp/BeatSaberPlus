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
        [UIComponent("SongRainToggle")]
        public ToggleSetting m_SongRain;
        [UIComponent("MenuRainSizeSlider")]
        public SliderSetting m_MenuRainSizeSlider;
        [UIComponent("SongRainSizeSlider")]
        public SliderSetting m_SongRainSizeSlider;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("EmoteDelaySlider")]
        public SliderSetting m_EmoteDelay;
        [UIComponent("EmoteFallSpeedSlider")]
        public SliderSetting m_Fallspeed;
        [UIComponent("ModeratorPowerToggle")]
        public ToggleSetting m_ModeratorPowerToggle;
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
            var l_AnchorMin = new Vector2(0.77f, -0.05f);
            var l_AnchorMax = new Vector2(0.91f, 1.05f);

            /// Left
            SDK.UI.ToggleSetting.Setup(m_MenuRain,              l_Event,        Config.ChatEmoteRain.MenuRain,          true);
            SDK.UI.ToggleSetting.Setup(m_SongRain,              l_Event,        Config.ChatEmoteRain.SongRain,          true);
            SDK.UI.SliderSetting.Setup(m_MenuRainSizeSlider,    l_Event, null,  Config.ChatEmoteRain.MenuRainSize,      true, true, l_AnchorMin, l_AnchorMax);
            SDK.UI.SliderSetting.Setup(m_SongRainSizeSlider,    l_Event, null,  Config.ChatEmoteRain.SongRainSize,      true, true, l_AnchorMin, l_AnchorMax);

            /// Right
            SDK.UI.SliderSetting.Setup(m_EmoteDelay,            l_Event, null,  Config.ChatEmoteRain.EmoteDelay,        true, true, l_AnchorMin, l_AnchorMax);
            SDK.UI.SliderSetting.Setup(m_Fallspeed,             l_Event, null,  Config.ChatEmoteRain.EmoteFallSpeed,    true, true, l_AnchorMin, l_AnchorMax);
            SDK.UI.ToggleSetting.Setup(m_ModeratorPowerToggle,  l_Event,        Config.ChatEmoteRain.ModeratorPower,    true);
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

            /// Left
            Config.ChatEmoteRain.MenuRain       = m_MenuRain.Value;
            Config.ChatEmoteRain.SongRain       = m_SongRain.Value;
            Config.ChatEmoteRain.MenuRainSize   = m_MenuRainSizeSlider.slider.value;
            Config.ChatEmoteRain.SongRainSize   = m_SongRainSizeSlider.slider.value;

            /// Right
            Config.ChatEmoteRain.EmoteDelay     = (int)m_EmoteDelay.slider.value;
            Config.ChatEmoteRain.EmoteFallSpeed = m_Fallspeed.slider.value;
            Config.ChatEmoteRain.ModeratorPower = m_ModeratorPowerToggle.Value;

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

            /// Left
            m_MenuRain.Value                    = Config.ChatEmoteRain.MenuRain;
            m_SongRain.Value                    = Config.ChatEmoteRain.SongRain;
            SDK.UI.SliderSetting.SetValue(m_MenuRainSizeSlider, Config.ChatEmoteRain.MenuRainSize);
            SDK.UI.SliderSetting.SetValue(m_SongRainSizeSlider, Config.ChatEmoteRain.SongRainSize);

            /// Right
            SDK.UI.SliderSetting.SetValue(m_EmoteDelay,         Config.ChatEmoteRain.EmoteDelay);
            SDK.UI.SliderSetting.SetValue(m_Fallspeed,          Config.ChatEmoteRain.EmoteFallSpeed);
            m_ModeratorPowerToggle.Value = Config.ChatEmoteRain.ModeratorPower;

            m_PreventChanges = false;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }
    }
}
