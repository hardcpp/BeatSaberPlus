using Newtonsoft.Json;

namespace BeatSaberPlus_SongOverlay
{
    /// <summary>
    /// Config class helper
    /// </summary>
    internal class SOConfig : CP_SDK.Config.JsonConfig<SOConfig>
    {
        [JsonProperty] internal bool Enabled = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/SongOverlay/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {

        }
    }
}
