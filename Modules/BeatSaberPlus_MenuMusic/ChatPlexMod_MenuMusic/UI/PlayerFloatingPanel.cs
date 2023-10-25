using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ChatPlexMod_MenuMusic.UI
{
    /// <summary>
    /// Player floating panel view
    /// </summary>
    internal sealed class PlayerFloatingPanel : CP_SDK.UI.ViewController<PlayerFloatingPanel>
    {
        private XUIHLayout          m_MusicBackground   = null;
        private XUIHLayout          m_MusicCover        = null;
        private XUIText             m_SongTitle         = null;
        private XUIText             m_SongArtist        = null;
        private XUIIconButton       m_PlayPauseButton   = null;
        private XUISlider           m_Volume            = null;
        private XUIPrimaryButton    m_PlayItButton      = null;

        private CP_SDK.Misc.FastCancellationToken   m_CancellationToken = new CP_SDK.Misc.FastCancellationToken();
        private Sprite                              m_PlaySprite        = null;
        private Sprite                              m_PauseSprite       = null;

        private Data.Music                          m_CurrentMusic = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Assembly          = Assembly.GetExecutingAssembly();
            var l_NextSprite        = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Next.png"));
            var l_GlassSprite       = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Glass.png"));
            var l_PauseSprite       = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Pause.png"));
            var l_PlaySprite        = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Play.png"));
            var l_PlaylistSprite    = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Playlist.png"));
            var l_PrevSprite        = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Prev.png"));
            var l_RandSprite        = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Rand.png"));
            var l_SoundSprite       = CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_MenuMusic.Resources.Sound.png"));

            m_PlaySprite    = l_PlaySprite;
            m_PauseSprite   = l_PauseSprite;

            Templates.FullRectLayout(
                XUIHLayout.Make()
                    .SetPadding(0).SetSpacing(0)
                    .SetBackground(true, ColorU.WithAlpha(Color.gray, 0.75f), true)
                    .OnReady(x => {
                        x.CSizeFitter.enabled   = false;
                        x.LElement.ignoreLayout = true;
                        x.RTransform.anchorMin  = Vector2.zero;
                        x.RTransform.anchorMax  = Vector2.one;
                        x.RTransform.sizeDelta  = Vector2.zero;
                    })
                    .Bind(ref m_MusicBackground),

                XUIHLayout.Make(
                    XUIHLayout.Make(
                        XUIHLayout.Make()
                            .SetPadding(0).SetSpacing(0)
                            .SetBackground(true, ColorU.WithAlpha(Color.white, 0.8f)).SetBackgroundSprite(l_GlassSprite)
                            .OnReady(x =>
                            {
                                x.CSizeFitter.enabled = false;
                                x.LElement.ignoreLayout = true;
                                x.RTransform.anchorMin = Vector2.zero;
                                x.RTransform.anchorMax = Vector2.one;
                                x.RTransform.sizeDelta = Vector2.zero;
                            })
                    )
                    .SetBackground(true, Color.white)
                    .SetWidth(18f).SetHeight(18f)
                    .Bind(ref m_MusicCover),

                    XUIVLayout.Make(
                        XUIText.Make("Song name...")    .SetFontSize(3.5f).SetOverflowMode(TMPro.TextOverflowModes.Ellipsis).Bind(ref m_SongTitle),
                        XUIText.Make("Song artist...")  .SetFontSize(3.0f).SetOverflowMode(TMPro.TextOverflowModes.Ellipsis).SetColor(ColorU.ToUnityColor("#A0A0A0")).Bind(ref m_SongArtist),
                        XUIText.Make(" "),
                        XUIHLayout.Make(
                            XUIIconButton.Make(OnPrevPressed)       .SetSprite(l_PrevSprite),
                            XUIIconButton.Make(OnRandPressed)       .SetSprite(l_RandSprite),
                            //XUIIconButton.Make(OnPlaylistPressed)   .SetSprite(l_PlaylistSprite),
                            XUIIconButton.Make(OnPlayPausePressed)  .SetSprite(l_PlaySprite).Bind(ref m_PlayPauseButton),
                            XUIIconButton.Make(OnNextPressed)       .SetSprite(l_NextSprite)
                        )
                        .SetPadding(0)
                        .ForEachDirect<XUIIconButton>(x => x.OnReady((y) => y.RTransform.localScale = 1.5f * Vector3.one))
                    )
                    .SetPadding(0, 0, 0, 1).SetSpacing(0)
                    .OnReady(x => {
                        x.CSizeFitter.enabled           = false;
                        x.LElement.flexibleWidth        = 1000.0f;
                        x.VLayoutGroup.childAlignment   = TextAnchor.UpperLeft;
                    })
                )
                .SetPadding(1).SetSpacing(0)
                .OnReady(x =>
                {
                    x.HLayoutGroup.childForceExpandWidth    = true;
                    x.HLayoutGroup.childForceExpandHeight   = true;
                    x.CSizeFitter.enabled                   = false;
                    x.LElement.ignoreLayout                 = true;
                    x.RTransform.anchorMin                  = Vector2.zero;
                    x.RTransform.anchorMax                  = Vector2.one;
                })
            )
            .SetPadding(0).SetSpacing(0)
            .OnReady(x => {
                x.VLayoutGroup.childForceExpandWidth  = true;
                x.VLayoutGroup.childForceExpandHeight = true;
            })
            .BuildUI(transform);

            XUISlider.Make("Volume")
                .SetMinValue(0.0f).SetMaxValue(1.0f)
                .SetValue(MMConfig.Instance.PlaybackVolume)
                .OnValueChanged((_) => OnSettingChanged())
                .SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                .OnReady((x) =>
                {
                    x.LElement.enabled              = false;
                    x.RTransform.pivot              = new Vector2(  1.00f, 0.00f);
                    x.RTransform.anchorMin          = new Vector2(  1.00f, 0.00f);
                    x.RTransform.anchorMax          = new Vector2(  1.00f, 0.00f);
                    x.RTransform.anchoredPosition   = new Vector2(-11.00f, 1.15f);
                    x.RTransform.sizeDelta          = new Vector2( 35.00f, 5.00f);
                    x.RTransform.localScale         = 0.7f * Vector2.one;
                })
                .Bind(ref m_Volume)
                .BuildUI(transform);

            XUIPrimaryButton.Make("Play it", OnPlayItPressed)
                .OnReady((x) =>
                {
                    x.LElement.enabled              = false;
                    x.RTransform.pivot              = new Vector2( 1.00f, 0.00f);
                    x.RTransform.anchorMin          = new Vector2( 1.00f, 0.00f);
                    x.RTransform.anchorMax          = new Vector2( 1.00f, 0.00f);
                    x.RTransform.anchoredPosition   = new Vector2(-2.00f, 1.15f);
                    x.RTransform.localScale         = 0.7f * Vector2.one;
                })
                .Bind(ref m_PlayItButton)
                .BuildUI(transform);

            OnMusicChanged(null);

            if (CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Audio Tweaker"))
                m_Volume.SetActive(false);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            UpdateVolume();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        private void OnSettingChanged()
        {
            MMConfig.Instance.PlaybackVolume = m_Volume.Element.GetValue();
            MenuMusic.Instance.UpdatePlaybackVolume(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set current played music
        /// </summary>
        /// <param name="p_Name">Current song name</param>
        internal void OnMusicChanged(Data.Music p_Music)
        {
            var l_Str1 = p_Music?.GetSongName() ?? "<alpha=#AA>No name...";
            if (l_Str1.Length > 30)
                l_Str1 = l_Str1.Substring(0, 30) + "...";

            var l_Str2 = p_Music?.GetSongArtist() ?? " ";
            if (l_Str2.Length > 50)
                l_Str2 = l_Str2.Substring(0, 50) + "...";

            if (m_SongTitle?.Element)  m_SongTitle.Element.TMProUGUI.text  = l_Str1;
            if (m_SongArtist?.Element) m_SongArtist.Element.TMProUGUI.text = l_Str2;

            m_CancellationToken.Cancel();
            if (p_Music != null)
            {
                p_Music?.GetCoverBytesAsync(m_CancellationToken, (x) =>
                {
                    Utils.ArtProvider.Prepare(x, m_CancellationToken, (p_Cover, p_Background) =>
                    {
                        m_MusicCover.SetBackgroundSprite(p_Cover);
                        m_MusicBackground.SetBackgroundSprite(p_Background);
                    });
                }, null);

                if (m_PlayItButton != null)
                    m_PlayItButton.SetInteractable(p_Music.MusicProvider.SupportPlayIt);
            }

            m_CurrentMusic = p_Music;
        }
        /// <summary>
        /// Is the music paused
        /// </summary>
        /// <param name="p_IsPaused">New state</param>
        internal void SetIsPaused(bool p_IsPaused)
            => m_PlayPauseButton?.SetSprite(p_IsPaused ? m_PlaySprite : m_PauseSprite);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update volume
        /// </summary>
        internal void UpdateVolume()
            => m_Volume?.SetValue(MMConfig.Instance.PlaybackVolume, false);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the previous button is pressed
        /// </summary>
        private void OnPrevPressed()
            => MenuMusic.Instance.StartPreviousMusic();
        /// <summary>
        /// When the random button is pressed
        /// </summary>
        private void OnRandPressed()
            => MenuMusic.Instance.StartNewMusic(true);
        /// <summary>
        /// When the playlist button is pressed
        /// </summary>
        private void OnPlaylistPressed()
        {

        }
        /// <summary>
        /// When the random button is pressed
        /// </summary>
        private void OnPlayPausePressed()
            => MenuMusic.Instance.TogglePause();
        /// <summary>
        /// When the next button is pressed
        /// </summary>
        private void OnNextPressed()
            => MenuMusic.Instance.StartNextMusic();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On play the map pressed
        /// </summary>
        private void OnPlayItPressed()
        {
            if (m_CurrentMusic == null || !m_CurrentMusic.MusicProvider.SupportPlayIt)
                return;

            if (!m_CurrentMusic.MusicProvider.StartGameSpecificGamePlay(m_CurrentMusic))
                ShowMessageModal("Map not found!");
        }
    }
}
