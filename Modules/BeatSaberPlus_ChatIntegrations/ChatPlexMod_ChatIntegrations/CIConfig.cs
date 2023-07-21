using Newtonsoft.Json;
using System.IO;

namespace ChatPlexMod_ChatIntegrations
{
    internal class CIConfig : CP_SDK.Config.JsonConfig<CIConfig>
    {
        [JsonProperty] internal bool Enabled = false;
        [JsonProperty] internal string DataLocation = Path.Combine(CP_SDK.ChatPlexSDK.BasePath, $"UserData/{CP_SDK.ChatPlexSDK.ProductName}/ChatIntegrations/");
        [JsonProperty] internal string LastBackup = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}/ChatIntegrations/Config";

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
