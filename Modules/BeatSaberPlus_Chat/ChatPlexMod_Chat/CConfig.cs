using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlexMod_Chat
{
    /// <summary>
    /// Chat configuration
    /// </summary>
    internal class CConfig : CP_SDK.Config.JsonConfig<CConfig>
    {
#if BEATSABER || UNITY_TESTING
        private static Vector3 DefaultChatMenuPosition = new Vector3(  0.00f, 4.10f, 3.50f);
        private static Vector3 DefaultChatMenuRotation = new Vector3(325.00f, 0.00f, 0.00f);

        private static Vector3 DefaultChatPlayingPosition = new Vector3(  0.00f, 4.20f, 5.80f);
        private static Vector3 DefaultChatPlayingRotation = new Vector3(325.00f, 0.00f, 0.00f);
#elif SYNTHRIDERS
        private static Vector3 DefaultChatMenuPosition = new Vector3(  0.00f, 5.00f, 1.70f);
        private static Vector3 DefaultChatMenuRotation = new Vector3(325.00f, 0.00f, 0.00f);

        private static Vector3 DefaultChatPlayingPosition = new Vector3(  0.00f, 4.20f, 5.80f);
        private static Vector3 DefaultChatPlayingRotation = new Vector3(325.00f, 0.00f, 0.00f);
#elif AUDIOTRIP
        private static Vector3 DefaultChatMenuPosition = new Vector3(  0.00f, 4.10f, 3.50f);
        private static Vector3 DefaultChatMenuRotation = new Vector3(325.00f, 0.00f, 0.00f);

        private static Vector3 DefaultChatPlayingPosition = new Vector3(  0.00f, 4.20f, 5.80f);
        private static Vector3 DefaultChatPlayingRotation = new Vector3(325.00f, 0.00f, 0.00f);
#elif BOOMBOX
        private static Vector3 DefaultChatMenuPosition = new Vector3(3.191f,  2.852f, 2.115f);
        private static Vector3 DefaultChatMenuRotation = new Vector3(0.000f, 40.000f, 0.000f);

        private static Vector3 DefaultChatPlayingPosition = new Vector3(2.33f,  2.145f,  3.669f);
        private static Vector3 DefaultChatPlayingRotation = new Vector3(0.00f, 49.4501f, 0.000f);
#elif DANCEDASH
        private static Vector3 DefaultChatMenuPosition = new Vector3(0.00f, 4.10f, 3.50f);
        private static Vector3 DefaultChatMenuRotation = new Vector3(325.00f, 0.00f, 0.00f);

        private static Vector3 DefaultChatPlayingPosition = new Vector3(0.00f, 4.20f, 5.80f);
        private static Vector3 DefaultChatPlayingRotation = new Vector3(325.00f, 0.00f, 0.00f);
#else
#error Missing game implementation
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [JsonProperty] internal bool Enabled = true;

        [JsonProperty] internal Vector2 ChatSize            = new Vector2(120, 140);
        [JsonProperty] internal bool    ReverseChatOrder    = false;
        [JsonProperty] internal bool    PlatformOriginColor = true;

        [JsonProperty] internal float   FontSize            = 3.4f;

        [JsonProperty] internal bool AlignWithFloor             = true;
        [JsonProperty] internal bool ShowLockIcon               = true;
        [JsonProperty] internal bool FollowEnvironementRotation = true;
        [JsonProperty] internal bool ShowViewerCount            = true;
        [JsonProperty] internal bool ShowFollowEvents           = true;
        [JsonProperty] internal bool ShowSubscriptionEvents     = true;
        [JsonProperty] internal bool ShowBitsCheeringEvents     = true;
        [JsonProperty] internal bool ShowChannelPointsEvent     = true;
        [JsonProperty] internal bool FilterViewersCommands      = false;
        [JsonProperty] internal bool FilterBroadcasterCommands  = false;

        [JsonProperty] internal Color BackgroundColor   = new Color(0.00f, 0.00f, 0.00f, 0.90f);
        [JsonProperty] internal Color HighlightColor    = new Color(0.57f, 0.28f, 1.00f, 0.12f);
        [JsonProperty] internal Color TextColor         = new Color(1.00f, 1.00f, 1.00f, 1.00f);
        [JsonProperty] internal Color PingColor         = new Color(0.90f, 0.00f, 0.00f, 0.36f);

        [JsonProperty] internal Vector3 MenuChatPosition = DefaultChatMenuPosition;
        [JsonProperty] internal Vector3 MenuChatRotation = DefaultChatMenuRotation;

        [JsonProperty] internal Vector3 PlayingChatPosition = DefaultChatPlayingPosition;
        [JsonProperty] internal Vector3 PlayingChatRotation = DefaultChatPlayingRotation;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<string> ModerationKeys = new List<string>()
        {

        };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/Chat/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (p_OnCreation)
            {
                ModerationKeys = new List<string>()
                {
                    "/host",
                    "/unban",
                    "/untimeout",
                    "!bsr"
                };
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset chat positions
        /// </summary>
        internal void ResetPosition()
        {
            MenuChatPosition = DefaultChatMenuPosition;
            MenuChatRotation = DefaultChatMenuRotation;

            PlayingChatPosition = DefaultChatPlayingPosition;
            PlayingChatRotation = DefaultChatPlayingRotation;
        }
    }
}
