using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus.Modules.Online.UI
{
    /// <summary>
    /// Online settings view controller
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("usebeatsaberpluscustommapsserver-toggle")]
        private ToggleSetting m_UserBeatSaberPlusCustomMapsServer;
        [UIComponent("usebeatsaberpluscustommapserverinmoresongs-toggle")]
        private ToggleSetting m_UserBeatSaberPlusCustomMapsServerInMoreSongs;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            SDK.UI.ToggleSetting.Setup(m_UserBeatSaberPlusCustomMapsServer,              l_Event, Config.Online.UseBSPCustomMapsServer,             true);
            SDK.UI.ToggleSetting.Setup(m_UserBeatSaberPlusCustomMapsServerInMoreSongs,   l_Event, Config.Online.UseBSPCustomMapsServerOnMoreSongs,  true);
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
