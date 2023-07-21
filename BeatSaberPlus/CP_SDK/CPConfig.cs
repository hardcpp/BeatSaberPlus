using Newtonsoft.Json;

namespace CP_SDK
{
    /// <summary>
    /// ChatPlex SDK config
    /// </summary>
    internal class CPConfig : Config.JsonConfig<CPConfig>
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
            => $"{ChatPlexSDK.ProductName}/Config";

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