using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using TMPro;
using UnityEngine;

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
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = Config.MenuMusic.BackgroundColor;
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set current played music
        /// </summary>
        /// <param name="p_Name"></param>
        internal void SetPlayingSong(string p_Name)
        {
            if (m_PlayingText != null)
                m_PlayingText.text = p_Name;
        }
        /// <summary>
        /// Update background color
        /// </summary>
        internal void UpdateBackgroundColor()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = Config.MenuMusic.BackgroundColor;
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
        /// When the next button is pressed
        /// </summary>
        [UIAction("next-pressed")]
        internal void OnNextPressed()
        {
            MenuMusic.Instance.StartNextMusic();
        }
    }
}
