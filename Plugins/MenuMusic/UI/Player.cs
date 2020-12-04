using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Plugins.MenuMusic.UI
{
    /// <summary>
    /// Player UI window
    /// </summary>
    internal class Player : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("Playing")]
        private TextMeshProUGUI m_PlayingText = null;
        [UIObject("buttons")]
        private GameObject m_Buttons = null;

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

            if (p_FirstActivation)
            {
                /// Update background color
                var l_Color = GetComponentInChildren<ImageView>().color;
                l_Color.a = 0.5f;
                l_Color.r = 0f;
                l_Color.g = 0f;
                l_Color.b = 0f;
                GetComponentInChildren<ImageView>().color = l_Color;
            }

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings button pressed
        /// </summary>
        [UIAction("settings-pressed")]
        internal void OnSettingsPressed()
        {
            MenuMusic.Instance.ShowUI();
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
