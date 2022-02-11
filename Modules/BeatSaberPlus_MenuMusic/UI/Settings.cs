using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus_MenuMusic.UI
{
    /// <summary>
    /// Settings view controller
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
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
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowPlayerInterface,           l_Event,                                                          MMConfig.Instance.ShowPlayer,                           true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowPlayTime,                  l_Event,                                                          MMConfig.Instance.ShowPlayTime,                         true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_PlayerBackgroundOpacity,    l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   MMConfig.Instance.BackgroundColor.a,                    true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_PlayerBackground,               l_Event,                                                          MMConfig.Instance.BackgroundColor.ColorWithAlpha(1.0f), true);

            /// Right
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PlaySongsFromBeginning,        l_Event,                                                          MMConfig.Instance.StartSongFromBeginning,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_StartANewMusicOnSceneChange,   l_Event,                                                          MMConfig.Instance.StartANewMusicOnSceneChange,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_LoopCurrentMusic,              l_Event,                                                          MMConfig.Instance.LoopCurrentMusic,                     true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PlayOnlyCustomMenuMusics,      l_Event,                                                          MMConfig.Instance.UseOnlyCustomMenuSongsFolder,         true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_PlaybackVolume,             l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   MMConfig.Instance.PlaybackVolume,                       true);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            m_PreventChanges = true;
            m_PlaybackVolume.Value = MMConfig.Instance.PlaybackVolume;
            m_PreventChanges = false;
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            MMConfig.Instance.Save();
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
            MMConfig.Instance.ShowPlayer                     = m_ShowPlayerInterface.Value;
            MMConfig.Instance.ShowPlayTime                   = m_ShowPlayTime.Value;
            MMConfig.Instance.BackgroundColor                = m_PlayerBackground.CurrentColor.ColorWithAlpha(m_PlayerBackgroundOpacity.Value);

            /// Right
            MMConfig.Instance.StartSongFromBeginning         = m_PlaySongsFromBeginning.Value;
            MMConfig.Instance.StartANewMusicOnSceneChange    = m_StartANewMusicOnSceneChange.Value;
            MMConfig.Instance.LoopCurrentMusic               = m_LoopCurrentMusic.Value;
            MMConfig.Instance.UseOnlyCustomMenuSongsFolder   = m_PlayOnlyCustomMenuMusics.Value;
            MMConfig.Instance.PlaybackVolume                 = m_PlaybackVolume.Value;

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
            m_ShowPlayerInterface.Value         = MMConfig.Instance.ShowPlayer;
            m_ShowPlayTime.Value                = MMConfig.Instance.ShowPlayTime;
            m_PlayerBackgroundOpacity.Value     = MMConfig.Instance.BackgroundColor.a;
            m_PlayerBackground.CurrentColor     = MMConfig.Instance.BackgroundColor.ColorWithAlpha(1.0f);

            /// Right
            m_PlaySongsFromBeginning.Value      = MMConfig.Instance.StartSongFromBeginning;
            m_StartANewMusicOnSceneChange.Value = MMConfig.Instance.StartANewMusicOnSceneChange;
            m_LoopCurrentMusic.Value            = MMConfig.Instance.LoopCurrentMusic;
            m_PlayOnlyCustomMenuMusics.Value    = MMConfig.Instance.UseOnlyCustomMenuSongsFolder;
            m_PlaybackVolume.Value              = MMConfig.Instance.PlaybackVolume;

            m_PreventChanges = false;

            /// Update playback volume & player
            MenuMusic.Instance.UpdatePlaybackVolume(true);
            MenuMusic.Instance.UpdatePlayer();
        }
    }
}
