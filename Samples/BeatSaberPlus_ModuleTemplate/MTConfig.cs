using Newtonsoft.Json;

namespace BeatSaberPlus_ModuleTemplate
{
    /// <summary>
    /// Config class helper
    /// </summary>
    internal class MTConfig : CP_SDK.Config.JsonConfig<MTConfig>
    {
        [JsonProperty] internal bool Enabled = true;
        [JsonProperty] internal bool TemplateSetting = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/ModuleTemplate/Config";

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
