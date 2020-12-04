using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;

namespace BeatSaberPlus.Plugins.ChatEmoteRain.UI
{
    /// <summary>
    /// Chat Emote Rain settings main view
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("MenuRainToggle")]
        public ToggleSetting m_MenuRain;
        [UIComponent("SongRainToggle")]
        public ToggleSetting m_SongRain;
        [UIComponent("MenuRainSizeSlider")]
        public SliderSetting m_MenuRainSizeSlider;
        [UIComponent("SongRainSizeSlider")]
        public SliderSetting m_SongRainSizeSlider;
        [UIComponent("EmoteDelaySlider")]
        public SliderSetting m_EmoteDelay;
        [UIComponent("EmoteFallSpeedSlider")]
        public SliderSetting m_Fallspeed;
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
                var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Set values
                Utils.GameUI.PrepareToggleSetting(m_MenuRain,           l_Event,        Config.ChatEmoteRain.MenuRain,          false);
                Utils.GameUI.PrepareToggleSetting(m_SongRain,           l_Event,        Config.ChatEmoteRain.SongRain,          false);
                Utils.GameUI.PrepareSliderSetting(m_MenuRainSizeSlider, l_Event, null,  Config.ChatEmoteRain.MenuRainSize,      false);
                Utils.GameUI.PrepareSliderSetting(m_SongRainSizeSlider, l_Event, null,  Config.ChatEmoteRain.SongRainSize,      false);
                Utils.GameUI.PrepareSliderSetting(m_EmoteDelay,         l_Event, null,  Config.ChatEmoteRain.EmoteDelay,        false);
                Utils.GameUI.PrepareSliderSetting(m_Fallspeed,          l_Event, null,  Config.ChatEmoteRain.EmoteFallSpeed,    false);
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
            /// Update config
            Config.ChatEmoteRain.MenuRain       = m_MenuRain.Value;
            Config.ChatEmoteRain.SongRain       = m_SongRain.Value;
            Config.ChatEmoteRain.MenuRainSize   = m_MenuRainSizeSlider.slider.value;
            Config.ChatEmoteRain.SongRainSize   = m_SongRainSizeSlider.slider.value;
            Config.ChatEmoteRain.EmoteDelay     = (int)m_EmoteDelay.slider.value;
            Config.ChatEmoteRain.EmoteFallSpeed = m_Fallspeed.slider.value;
        }
    }
}
