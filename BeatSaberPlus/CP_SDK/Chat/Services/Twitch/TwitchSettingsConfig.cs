using Newtonsoft.Json;
using System;
using System.IO;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Auth config
    /// </summary>
    internal class TwitchSettingsConfig : Config.JsonConfig<TwitchSettingsConfig>
    {
        internal class _Channel
        {
            [JsonProperty] internal string Name = "New emitter";
            [JsonProperty] internal bool CanSendMessages = true;
        }

        [JsonProperty] internal string  TokenChat          = "";
        [JsonProperty] internal string  TokenChannel       = "";
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Channel[] Channels = new _Channel[] { };
        [JsonProperty] internal bool    ParseTwitchEmotes   = true;
        [JsonProperty] internal bool    ParseCheermotes     = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "TwitchSettings";
        /// <summary>
        /// Get full config path
        /// </summary>
        /// <returns></returns>
        public override string GetFullPath()
            => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $".ChatPlex/{GetRelativePath()}.json"));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (Channels == null)
                Channels = new _Channel[] { };

            Save();
        }
    }
}
