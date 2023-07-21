using Newtonsoft.Json;
using System;
using System.IO;

namespace CP_SDK.Chat
{
    /// <summary>
    /// Chat Settings config
    /// </summary>
    public class ChatModSettings : Config.JsonConfig<ChatModSettings>
    {
        internal class _Emotes
        {
            [JsonProperty] internal bool ParseBTTVEmotes        = true;
            [JsonProperty] internal bool ParseFFZEmotes         = true;
            [JsonProperty] internal bool Parse7TVEmotes         = true;
            [JsonProperty] internal bool ParseEmojis            = true;
            [JsonProperty] internal bool ParseTemporaryChannels = true;
        }

        [JsonProperty] internal bool LaunchWebAppOnStartup = true;
        [JsonProperty] internal int WebAppPort = 8339;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Emotes Emotes = new _Emotes();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [JsonIgnore] public bool Emotes_ParseBTTVEmotes         => Emotes.ParseBTTVEmotes;
        [JsonIgnore] public bool Emotes_ParseFFZEmotes          => Emotes.ParseFFZEmotes;
        [JsonIgnore] public bool Emotes_Parse7TVEmotes          => Emotes.Parse7TVEmotes;
        [JsonIgnore] public bool Emotes_ParseEmojis             => Emotes.ParseEmojis;
        [JsonIgnore] public bool Emotes_ParseTemporaryChannels  => Emotes.ParseTemporaryChannels;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "ChatModSettings";
        /// <summary>
        /// Get full config path
        /// </summary>
        /// <returns></returns>
        public override string GetFullPath()
            => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $".ChatPlex/{GetRelativePath()}.json"));
    }
}
