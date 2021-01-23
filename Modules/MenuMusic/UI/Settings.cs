using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus.Modules.MenuMusic.UI
{
    /// <summary>
    /// Settings view controller
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("ShowPlayerInterfaceToggle")]
        private ToggleSetting m_ShowPlayerInterface;
        [UIComponent("ShowPlayTimeToggle")]
        private ToggleSetting m_ShowPlayTime;
        [UIComponent("PlayerBackgroundOpacityIncrement")]
        private IncrementSetting m_PlayerBackgroundOpacity;
        [UIComponent("PlayerBackgroundColor")]
        private ColorSetting m_PlayerBackground;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("PlaySongsFromBeginningToggle")]
        private ToggleSetting m_PlaySongsFromBeginning;
        [UIComponent("StartANewMusicOnSceneChangeToggle")]
        private ToggleSetting m_StartANewMusicOnSceneChange;
        [UIComponent("LoopCurrentMusicToggle")]
        private ToggleSetting m_LoopCurrentMusic;


        [UIComponent("PlayOnlyCustomMenuMusicsToggle")]
        private ToggleSetting m_PlayOnlyCustomMenuMusics;
        [UIComponent("PlaybackVolumeIncrement")]
        private IncrementSetting m_PlaybackVolume;
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
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Left
            SDK.UI.ToggleSetting.Setup(m_ShowPlayerInterface,           l_Event,                                            Config.MenuMusic.ShowPlayer,                    true);
            SDK.UI.ToggleSetting.Setup(m_ShowPlayTime,                  l_Event,                                            Config.MenuMusic.ShowPlayTime,                  true);
            SDK.UI.IncrementSetting.Setup(m_PlayerBackgroundOpacity,    l_Event, SDK.UI.BSMLSettingFormarter.Percentage,    Config.MenuMusic.BackgroundA,                   true);
            SDK.UI.ColorSetting.Setup(m_PlayerBackground,               l_Event,                                            Config.MenuMusic.BackgroundColor,               true);

            /// Right
            SDK.UI.ToggleSetting.Setup(m_PlaySongsFromBeginning,        l_Event,                                            Config.MenuMusic.StartSongFromBeginning,        true);
            SDK.UI.ToggleSetting.Setup(m_StartANewMusicOnSceneChange,   l_Event,                                            Config.MenuMusic.StartANewMusicOnSceneChange,   true);
            SDK.UI.ToggleSetting.Setup(m_LoopCurrentMusic,              l_Event,                                            Config.MenuMusic.LoopCurrentMusic,              true);
            SDK.UI.ToggleSetting.Setup(m_PlayOnlyCustomMenuMusics,      l_Event,                                            Config.MenuMusic.UseOnlyCustomMenuSongsFolder,  true);
            SDK.UI.IncrementSetting.Setup(m_PlaybackVolume,             l_Event, SDK.UI.BSMLSettingFormarter.Percentage,    Config.MenuMusic.PlaybackVolume,                true);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            m_PreventChanges = true;
            m_PlaybackVolume.Value = Config.MenuMusic.PlaybackVolume;
            m_PreventChanges = false;
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
            Config.MenuMusic.ShowPlayer                     = m_ShowPlayerInterface.Value;
            Config.MenuMusic.ShowPlayTime                   = m_ShowPlayTime.Value;
            Config.MenuMusic.BackgroundA                    = m_PlayerBackgroundOpacity.Value;
            Config.MenuMusic.BackgroundR                    = m_PlayerBackground.CurrentColor.r;
            Config.MenuMusic.BackgroundG                    = m_PlayerBackground.CurrentColor.g;
            Config.MenuMusic.BackgroundB                    = m_PlayerBackground.CurrentColor.b;

            /// Right
            Config.MenuMusic.StartSongFromBeginning         = m_PlaySongsFromBeginning.Value;
            Config.MenuMusic.StartANewMusicOnSceneChange    = m_StartANewMusicOnSceneChange.Value;
            Config.MenuMusic.LoopCurrentMusic               = m_LoopCurrentMusic.Value;
            Config.MenuMusic.UseOnlyCustomMenuSongsFolder   = m_PlayOnlyCustomMenuMusics.Value;
            Config.MenuMusic.PlaybackVolume                 = m_PlaybackVolume.Value;

            /// Update playback volume & player
            MenuMusic.Instance.UpdatePlaybackVolume(true);
            MenuMusic.Instance.UpdatePlayer();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            m_PreventChanges = true;

            /// Left
            m_ShowPlayerInterface.Value         = Config.MenuMusic.ShowPlayer;
            m_ShowPlayTime.Value                = Config.MenuMusic.ShowPlayTime;
            m_PlayerBackgroundOpacity.Value     = Config.MenuMusic.BackgroundA;
            m_PlayerBackground.CurrentColor     = new Color(Config.MenuMusic.BackgroundR, Config.MenuMusic.BackgroundG, Config.MenuMusic.BackgroundB, 1f);

            /// Right
            m_PlaySongsFromBeginning.Value      = Config.MenuMusic.StartSongFromBeginning;
            m_StartANewMusicOnSceneChange.Value = Config.MenuMusic.StartANewMusicOnSceneChange;
            m_LoopCurrentMusic.Value            = Config.MenuMusic.LoopCurrentMusic;
            m_PlayOnlyCustomMenuMusics.Value    = Config.MenuMusic.UseOnlyCustomMenuSongsFolder;
            m_PlaybackVolume.Value              = Config.MenuMusic.PlaybackVolume;

            m_PreventChanges = false;

            /// Update playback volume & player
            MenuMusic.Instance.UpdatePlaybackVolume(true);
            MenuMusic.Instance.UpdatePlayer();
        }
    }
}
