using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace BeatSaberPlus.Plugins.MenuMusic.UI
{
    /// <summary>
    /// Settings view controller
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("interface-bool")]
        public ToggleSetting m_ShowPlayer;
        [UIComponent("beginning-bool")]
        public ToggleSetting m_PlaySongsFromBeginning;
        [UIComponent("customonly-bool")]
        public ToggleSetting m_PlayOnlyCustomMenuMusics;
        [UIComponent("volume-increment")]
        public IncrementSetting m_PlaybackVolume;
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
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged),        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Set values
                Utils.GameUI.PrepareToggleSetting(m_ShowPlayer,                  l_Event,                                       Config.MenuMusic.ShowPlayer,                       false);
                Utils.GameUI.PrepareToggleSetting(m_PlaySongsFromBeginning,      l_Event,                                       Config.MenuMusic.StartSongFromBeginning,           false);
                Utils.GameUI.PrepareToggleSetting(m_PlayOnlyCustomMenuMusics,    l_Event,                                       Config.MenuMusic.UseOnlyCustomMenuSongsFolder,     false);
                Utils.GameUI.PrepareIncrementSetting(m_PlaybackVolume,           l_Event, Utils.GameUI.Formatter_Percentage,    Config.MenuMusic.PlaybackVolume,                   false);
            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            /// Close modals
            m_ParserParams.EmitEvent("CloseAllModals");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            /// Handle player UI
            if (Config.MenuMusic.ShowPlayer && !m_ShowPlayer.Value)
                MenuMusic.Instance.DestroyFloatingPlayer();
            else if (!Config.MenuMusic.ShowPlayer && m_ShowPlayer.Value)
                MenuMusic.Instance.CreateFloatingPlayer();

            /// Update config
            Config.MenuMusic.ShowPlayer                     = m_ShowPlayer.Value;
            Config.MenuMusic.StartSongFromBeginning         = m_PlaySongsFromBeginning.Value;
            Config.MenuMusic.UseOnlyCustomMenuSongsFolder   = m_PlayOnlyCustomMenuMusics.Value;
            Config.MenuMusic.PlaybackVolume                 = m_PlaybackVolume.Value;

            /// Update playback volume
            MenuMusic.Instance.UpdatePlaybackVolume();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reload songs
        /// </summary>
        [UIAction("reload-btn-pressed")]
        private void OnReloadButton()
        {
            /// Reload songs
            MenuMusic.Instance.RefreshSongs();

            /// Start music
            MenuMusic.Instance.StartNewMusic();

            /// Show modal
            m_ParserParams.EmitEvent("ReloadMessageModal");
        }
    }
}
