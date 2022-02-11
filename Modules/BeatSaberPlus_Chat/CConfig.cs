using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace BeatSaberPlus_Chat
{
    internal class CConfig : BeatSaberPlus.SDK.Config.JsonConfig<CConfig>
    {
        [JsonProperty] internal bool Enabled = true;

        [JsonProperty] internal Vector2 ChatSize = new Vector2(120, 140);
        [JsonProperty] internal bool ReverseChatOrder = false;
        [JsonProperty] internal string SystemFontName = "Segoe UI";
        [JsonProperty] internal float FontSize = 3.4f;

        [JsonProperty] internal bool AlignWithFloor = true;
        [JsonProperty] internal bool ShowLockIcon = true;
        [JsonProperty] internal bool FollowEnvironementRotation = true;
        [JsonProperty] internal bool ShowViewerCount = true;
        [JsonProperty] internal bool ShowFollowEvents = true;
        [JsonProperty] internal bool ShowSubscriptionEvents = true;
        [JsonProperty] internal bool ShowBitsCheeringEvents = true;
        [JsonProperty] internal bool ShowChannelPointsEvent = true;
        [JsonProperty] internal bool FilterViewersCommands = false;
        [JsonProperty] internal bool FilterBroadcasterCommands = false;

        [JsonProperty] internal Color BackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        [JsonProperty] internal Color HighlightColor = new Color(0.57f, 0.28f, 1f, 0.12f);
        [JsonProperty] internal Color AccentColor = new Color(0.57f, 0.28f, 1f, 1.00f);
        [JsonProperty] internal Color TextColor = new Color(1f, 1f, 1f, 1f);
        [JsonProperty] internal Color PingColor = new Color(1.00f, 0.00f, 0.00f, 0.18f);

        [JsonProperty] internal Vector3 MenuChatPosition = new Vector3(0, 4.10f, 3.50f);
        [JsonProperty] internal Vector3 MenuChatRotation = new Vector3(325f,0,0);

        [JsonProperty] internal Vector3 PlayingChatPosition = new Vector3(0, 4.2f, 5.8f);
        [JsonProperty] internal Vector3 PlayingChatRotation = new Vector3(325f, 0, 0);

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
            => "BeatSaberPlus/Chat/Config";

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

            if (BeatSaberPlus.Config.Chat.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled = BeatSaberPlus.Config.Chat.Enabled;

            ChatSize            = new Vector2(BeatSaberPlus.Config.Chat.ChatWidth, BeatSaberPlus.Config.Chat.ChatHeight);
            ReverseChatOrder    = BeatSaberPlus.Config.Chat.ReverseChatOrder;
            SystemFontName      = BeatSaberPlus.Config.Chat.SystemFontName;
            FontSize            = BeatSaberPlus.Config.Chat.FontSize;

            AlignWithFloor              = BeatSaberPlus.Config.Chat.AlignWithFloor;
            ShowLockIcon                = BeatSaberPlus.Config.Chat.ShowLockIcon;
            FollowEnvironementRotation  = BeatSaberPlus.Config.Chat.FollowEnvironementRotation;
            ShowViewerCount             = BeatSaberPlus.Config.Chat.ShowViewerCount;
            ShowFollowEvents            = BeatSaberPlus.Config.Chat.ShowFollowEvents;
            ShowSubscriptionEvents      = BeatSaberPlus.Config.Chat.ShowSubscriptionEvents;
            ShowBitsCheeringEvents      = BeatSaberPlus.Config.Chat.ShowBitsCheeringEvents;
            ShowChannelPointsEvent      = BeatSaberPlus.Config.Chat.ShowChannelPointsEvent;
            FilterViewersCommands       = BeatSaberPlus.Config.Chat.FilterViewersCommands;
            FilterBroadcasterCommands   = BeatSaberPlus.Config.Chat.FilterBroadcasterCommands;

            BackgroundColor = BeatSaberPlus.Config.Chat.BackgroundColor;
            HighlightColor  = BeatSaberPlus.Config.Chat.HighlightColor;
            AccentColor     = BeatSaberPlus.Config.Chat.AccentColor;
            TextColor       = BeatSaberPlus.Config.Chat.TextColor;
            PingColor       = BeatSaberPlus.Config.Chat.PingColor;

            MenuChatPosition = new Vector3(BeatSaberPlus.Config.Chat.MenuChatPositionX, BeatSaberPlus.Config.Chat.MenuChatPositionY, BeatSaberPlus.Config.Chat.MenuChatPositionZ);
            MenuChatRotation = new Vector3(BeatSaberPlus.Config.Chat.MenuChatRotationX, BeatSaberPlus.Config.Chat.MenuChatRotationY, BeatSaberPlus.Config.Chat.MenuChatRotationZ);

            PlayingChatPosition = new Vector3(BeatSaberPlus.Config.Chat.PlayingChatPositionX, BeatSaberPlus.Config.Chat.PlayingChatPositionY, BeatSaberPlus.Config.Chat.PlayingChatPositionZ);
            PlayingChatRotation = new Vector3(BeatSaberPlus.Config.Chat.PlayingChatRotationX, BeatSaberPlus.Config.Chat.PlayingChatRotationY, BeatSaberPlus.Config.Chat.PlayingChatRotationZ);

            ModerationKeys = BeatSaberPlus.Config.Chat.ModerationKeys.Split(new string[] { BeatSaberPlus.Config.Chat.s_ModerationKeyDefault_Split }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.Chat.OldConfigMigrated = true;
            Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset chat positions
        /// </summary>
        internal void ResetPosition()
        {
            MenuChatPosition = new Vector3(0, 4.10f, 3.50f);
            MenuChatRotation = new Vector3(325f, 0, 0);

            PlayingChatPosition = new Vector3(0, 4.2f, 5.8f);
            PlayingChatRotation = new Vector3(325f, 0, 0);
        }
    }
}
