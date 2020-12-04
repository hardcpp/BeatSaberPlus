using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;

namespace BeatSaberPlus.Plugins.Online.UI
{
    /// <summary>
    /// Online settings view controller
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
        [UIComponent("usebeatsaberpluscustommapsserver-toggle")]
        public ToggleSetting m_UserBeatSaberPlusCustomMapsServer;
        [UIComponent("usebeatsaberpluscustommapserverinmoresongs-toggle")]
        public ToggleSetting m_UserBeatSaberPlusCustomMapsServerInMoreSongs;
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

            if (p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                Utils.GameUI.PrepareToggleSetting(m_UserBeatSaberPlusCustomMapsServer,              l_Event, Config.Online.UseBSPCustomMapsServer,              false);
                Utils.GameUI.PrepareToggleSetting(m_UserBeatSaberPlusCustomMapsServerInMoreSongs,   l_Event, Config.Online.UseBSPCustomMapsServerOnMoreSongs,   false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            /// Update config
            Config.Online.UseBSPCustomMapsServer            = m_UserBeatSaberPlusCustomMapsServer.Value;
            Config.Online.UseBSPCustomMapsServerOnMoreSongs = m_UserBeatSaberPlusCustomMapsServerInMoreSongs.Value;

            /// Update patches
            Patches.PBeatSaverSharp_BeatSaver.SetUseBeatSaberPlusCustomMapsServer(Config.Online.UseBSPCustomMapsServer);
        }
    }
}
