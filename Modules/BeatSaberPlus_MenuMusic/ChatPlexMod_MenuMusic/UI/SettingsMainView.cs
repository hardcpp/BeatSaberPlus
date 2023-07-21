using CP_SDK.XUI;
using System.Linq;

namespace ChatPlexMod_MenuMusic.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUIDropdown     m_MusicProvider;
        private XUIToggle       m_ShowPlayerInterface;
        private XUIText         m_PlaybackVolumeLabel;
        private XUISlider       m_PlaybackVolume;

        private XUIToggle       m_PlaySongsFromBeginning;
        private XUIToggle       m_StartANewMusicOnSceneChange;
        private XUIToggle       m_LoopCurrentMusic;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Config = MMConfig.Instance;

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Menu Music | Settings"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Music provider"),
                        XUIDropdown.Make()
                            .SetOptions(Data.MusicProviderType.S).SetValue(Data.MusicProviderType.ToStr(l_Config.MusicProvider))
                            .OnValueChanged((_, __) => OnSettingChanged())
                            .Bind(ref m_MusicProvider),

                        XUIText.Make("Show player interface"),
                        XUIToggle.Make().SetValue(l_Config.ShowPlayer).Bind(ref m_ShowPlayerInterface),

                        XUIText.Make("Playback volume").Bind(ref m_PlaybackVolumeLabel),
                        XUISlider.Make()
                            .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f)
                            .SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                            .SetValue(l_Config.PlaybackVolume).Bind(ref m_PlaybackVolume)
                    )
                    .SetSpacing(1)
                    .SetWidth(60.0f)
                    .OnReady(x => x.HOrVLayoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()))
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Play songs from beginning"),
                        XUIToggle.Make().SetValue(l_Config.StartSongFromBeginning).Bind(ref m_PlaySongsFromBeginning),

                        XUIText.Make("Start a new song on scene change"),
                        XUIToggle.Make().SetValue(l_Config.StartANewMusicOnSceneChange).Bind(ref m_StartANewMusicOnSceneChange),

                        XUIText.Make("Loop current song"),
                        XUIToggle.Make().SetValue(l_Config.LoopCurrentMusic).Bind(ref m_LoopCurrentMusic)
                    )
                    .SetSpacing(1)
                    .SetWidth(60.0f)
                    .OnReady(x => x.HOrVLayoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()))
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            if (CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Audio Tweaker"))
            {
                m_PlaybackVolumeLabel.SetActive(false);
                m_PlaybackVolume.SetActive(false);
            }
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            m_PreventChanges = true;
            m_PlaybackVolume.SetValue(MMConfig.Instance.PlaybackVolume);
            m_PreventChanges = false;
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
            => MMConfig.Instance.Save();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        private void OnSettingChanged()
        {
            if (m_PreventChanges)
                return;

            var l_Config            = MMConfig.Instance;
            var l_OldMusicProvider  = l_Config.MusicProvider;
            l_Config.MusicProvider                  = Data.MusicProviderType.ToEnum(m_MusicProvider.Element.GetValue());
            l_Config.ShowPlayer                     = m_ShowPlayerInterface.Element.GetValue();
            l_Config.PlaybackVolume                 = m_PlaybackVolume.Element.GetValue();

            l_Config.StartSongFromBeginning         = m_PlaySongsFromBeginning.Element.GetValue();
            l_Config.StartANewMusicOnSceneChange    = m_StartANewMusicOnSceneChange.Element.GetValue();
            l_Config.LoopCurrentMusic               = m_LoopCurrentMusic.Element.GetValue();

            /// Update playback volume & player
            MenuMusic.Instance.UpdatePlaybackVolume(true);
            MenuMusic.Instance.UpdatePlayer();

            if (l_OldMusicProvider != l_Config.MusicProvider)
                MenuMusic.Instance.UpdateMusicProvider();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update volume
        /// </summary>
        internal void UpdateVolume()
            => m_PlaybackVolume?.SetValue(MMConfig.Instance.PlaybackVolume, false);
        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            m_PreventChanges = true;

            var l_Config = MMConfig.Instance;
            m_MusicProvider                 .SetValue(Data.MusicProviderType.ToStr(l_Config.MusicProvider));
            m_ShowPlayerInterface           .SetValue(l_Config.ShowPlayer);
            m_PlaybackVolume                .SetValue(l_Config.PlaybackVolume);

            m_PlaySongsFromBeginning        .SetValue(l_Config.StartSongFromBeginning);
            m_StartANewMusicOnSceneChange   .SetValue(l_Config.StartANewMusicOnSceneChange);
            m_LoopCurrentMusic              .SetValue(l_Config.LoopCurrentMusic);

            m_PreventChanges = false;

            /// Update playback volume & player
            MenuMusic.Instance.UpdatePlaybackVolume(true);
            MenuMusic.Instance.UpdatePlayer();
            MenuMusic.Instance.UpdateMusicProvider();
        }
    }
}
