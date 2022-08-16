using Newtonsoft.Json;

namespace BeatSaberPlus
{
    /// <summary>
    /// BeatSaberPlus SDK config
    /// </summary>
    internal class BSPConfig : CP_SDK.Config.JsonConfig<BSPConfig>
    {
        [JsonProperty] internal bool FirstRun = true;
        [JsonProperty] internal bool FirstChatCoreRun = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
            => Save();
    }
}