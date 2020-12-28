using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.MenuMusic.UI
{
    /// <summary>
    /// Player UI window
    /// </summary>
    internal class Player : SDK.UI.ResourceViewController<Player>
    {
#pragma warning disable CS0649
        [UIComponent("Playing")]
        private TextMeshProUGUI m_PlayingText = null;
        [UIObject("buttons")]
        private GameObject m_Buttons = null;
        [UIComponent("PlayPauseButton")]
        private Button m_PlayPauseButton = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update coroutine
        /// </summary>
        private Coroutine m_UpdateCoroutine = null;
        /// <summary>
        /// Play sprite
        /// </summary>
        private Sprite m_PlaySprite = null;
        /// <summary>
        /// Pause sprite
        /// </summary>
        private Sprite m_PauseSprite = null;
        /// <summary>
        /// Current song name
        /// </summary>
        private string m_CurrentSongName = "Playing...";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = Config.MenuMusic.BackgroundColor;

            if (m_PlaySprite == null)
                m_PlaySprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("BeatSaberPlus.Modules.MenuMusic.Resources.Play.png");
            if (m_PauseSprite == null)
                m_PauseSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("BeatSaberPlus.Modules.MenuMusic.Resources.Pause.png");

            m_UpdateCoroutine = StartCoroutine(UpdateCoroutine());
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            /// Fix buttons disappearing when a multi player level is done
            foreach (Transform l_Transform in m_Buttons.transform)
                l_Transform.gameObject.SetActive(true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            if (m_UpdateCoroutine != null)
            {
                StopCoroutine(m_UpdateCoroutine);
                m_UpdateCoroutine = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set current played music
        /// </summary>
        /// <param name="p_Name">Current song name</param>
        internal void SetPlayingSong(string p_Name)
        {
            m_CurrentSongName = p_Name;

            UpdateText();
        }
        /// <summary>
        /// Is the music paused
        /// </summary>
        /// <param name="p_IsPaused">New state</param>
        internal void SetIsPaused(bool p_IsPaused)
        {
            if (!UICreated)
                return;

            if (p_IsPaused)
                m_PlayPauseButton.gameObject.GetComponent<ButtonIconImage>().image.sprite = m_PlaySprite;
            else
                m_PlayPauseButton.gameObject.GetComponent<ButtonIconImage>().image.sprite = m_PauseSprite;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update background color
        /// </summary>
        internal void UpdateBackgroundColor()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = Config.MenuMusic.BackgroundColor;
        }
        /// <summary>
        /// Update status text
        /// </summary>
        internal void UpdateText()
        {
            if (!UICreated)
                return;

            if (Config.MenuMusic.ShowPlayTime)
            {
                var l_Text = "<size=80%>" + m_CurrentSongName;
                l_Text += "\n<size=70%><#7F7F7F>";

                int l_TotalMinutes = (int)(MenuMusic.Instance.CurrentDuration / 60);
                int l_TotalSeconds = (int)MenuMusic.Instance.CurrentDuration - (l_TotalMinutes * 60);

                int l_CurrentMinutes = (int)(MenuMusic.Instance.CurrentPosition / 60);
                int l_CurrentSeconds = (int)MenuMusic.Instance.CurrentPosition - (l_CurrentMinutes * 60);

                l_Text += string.Format("{0}:{1} / {2}:{3}", l_CurrentMinutes, l_CurrentSeconds.ToString().PadLeft(2, '0'), l_TotalMinutes, l_TotalSeconds.ToString().PadLeft(2, '0'));

                m_PlayingText.text = l_Text;
            }
            else
                m_PlayingText.text = m_CurrentSongName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings button pressed
        /// </summary>
        [UIAction("settings-pressed")]
        internal void OnSettingsPressed()
        {
            var l_Items = MenuMusic.Instance.GetSettingsUI();
            BeatSaberPlus.UI.MainViewFlowCoordinator.Instance().ChangeView(l_Items.Item1, l_Items.Item2, l_Items.Item3);
        }
        /// <summary>
        /// When the previous button is pressed
        /// </summary>
        [UIAction("prev-pressed")]
        internal void OnPrevPressed()
        {
            MenuMusic.Instance.StartPreviousMusic();
        }
        /// <summary>
        /// When the random button is pressed
        /// </summary>
        [UIAction("rand-pressed")]
        internal void OnRandPressed()
        {
            MenuMusic.Instance.StartNewMusic(true);
        }
        /// <summary>
        /// When the random button is pressed
        /// </summary>
        [UIAction("playpause-pressed")]
        internal void OnPlayPausePressed()
        {
            MenuMusic.Instance.TogglePause();
        }
        /// <summary>
        /// When the next button is pressed
        /// </summary>
        [UIAction("next-pressed")]
        internal void OnNextPressed()
        {
            MenuMusic.Instance.StartNextMusic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateCoroutine()
        {
            do
            {
                UpdateText();
                yield return new WaitForSeconds(1f);
            } while (CanBeUpdated);
        }
    }
}
