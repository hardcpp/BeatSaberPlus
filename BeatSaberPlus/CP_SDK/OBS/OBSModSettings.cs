using Newtonsoft.Json;
using System;
using System.IO;

namespace CP_SDK.OBS
{
    /// <summary>
    /// OBS Settings config
    /// </summary>
    public class OBSModSettings : Config.JsonConfig<OBSModSettings>
    {
        [JsonProperty] internal bool Enabled = false;
        [JsonProperty] internal string Server = "127.0.0.1:4444";
        [JsonProperty] internal string Password = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "OBSModSettings";
        /// <summary>
        /// Get full config path
        /// </summary>
        /// <returns></returns>
        public override string GetFullPath()
            => System.IO.Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $".ChatPlex/{GetRelativePath()}.json"));
    }
}
