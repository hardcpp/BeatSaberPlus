using BeatSaberPlus_ChatRequest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using static UnityEngine.EventSystems.EventTrigger;

namespace BeatSaberPlus_ChatRequest
{
    internal class CRConfig : CP_SDK.Config.JsonConfig<CRConfig>
    {
        internal class _Filters
        {
            [JsonProperty] internal bool NoBeatSage = false;

            [JsonProperty] internal bool NoRanked = false;

            [JsonProperty] internal bool NPSMin = false;
            [JsonProperty] internal int NPSMinV = 0;

            [JsonProperty] internal bool NPSMax = false;
            [JsonProperty] internal int NPSMaxV = 30;

            [JsonProperty] internal bool NJSMin = false;
            [JsonProperty] internal int NJSMinV = 0;

            [JsonProperty] internal bool NJSMax = false;
            [JsonProperty] internal int NJSMaxV = 30;

            [JsonProperty] internal bool DurationMin = false;
            [JsonProperty] internal int DurationMinV = 1;

            [JsonProperty] internal bool DurationMax = false;
            [JsonProperty] internal int DurationMaxV = 3;

            [JsonProperty] internal bool VoteMin = false;
            [JsonProperty] internal float VoteMinV = 0.5f;

            [JsonProperty] internal bool IgnoreMinVoteBelow = true;
            [JsonProperty] internal int IgnoreMinVoteBelowV = 4;

            [JsonProperty] internal bool DateMin = false;
            [JsonProperty] internal int DateMinV = 0;

            [JsonProperty] internal bool DateMax = false;
            [JsonProperty] internal int DateMaxV = 36;
        }

        internal class _OverlayIntegration
        {
            [JsonProperty] internal string SimpleQueueFileFormat = "%i - %n by %m";
            [JsonProperty] internal int SimpleQueueFileCount = 10;
            [JsonProperty] internal string SimpleQueueStatusOpen = "Queue is open!";
            [JsonProperty] internal string SimpleQueueStatusClosed = "Queue is closed!";
        }

        internal class _Commands
        {
            [Flags]
            internal enum EPermission
            {
                Viewers         = 1 << 1,
                Subscribers     = 1 << 2,
                VIPs            = 1 << 3,
                Moderators      = 1 << 4
            }

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BSRCommandPermissions       = EPermission.Viewers;
            [JsonProperty] internal bool        BSRCommandEnabled           = true;
            [JsonProperty] internal bool        BSRCommandCooldownPerUser   = true;
            [JsonProperty] internal int         BSRCommandCooldown          = 10;
            [JsonProperty] internal string      BSRCommand                  = "bsr";
            [JsonProperty] internal string      BSRCommand_UserBanned       = "@$UserName you are not allowed to make requests!";
            [JsonProperty] internal string      BSRCommand_QueueClosed      = "@$UserName the queue is closed!";
            [JsonProperty] internal string      BSRCommand_SearchDisabled   = "@$UserName Search is disabled";
            [JsonProperty] internal string      BSRCommand_Search0Result    = "@$UserName your search $Search produced 0 results!";
            [JsonProperty] internal string      BSRCommand_SearchResults    = "@$UserName your search $Search produced $Count results: $Results";
            [JsonProperty] internal string      BSRCommand_Blocklisted      = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is blocklisted!";
            [JsonProperty] internal string      BSRCommand_AlreadyQueued    = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is already in queue!";
            [JsonProperty] internal string      BSRCommand_RequestLimit     = "@$UserName you already have $UserRequestCount on the queue. $UserType are limited to $UserTypeLimit request(s).";
            [JsonProperty] internal string      BSRCommand_AlreadyPlayed    = "@$UserName this song was already requested this session!";
            [JsonProperty] internal string      BSRCommand_NotFound         = "@$UserName map $BSRKey not found.";
            [JsonProperty] internal string      BSRCommand_MapperBanned     = "@$UserName $UploaderName's maps are not allowed!";
            [JsonProperty] internal string      BSRCommand_RequestOK        = "(bsr $BSRKey) $SongName / $LevelAuthorName $Vote% requested by @$UserName added to queue.";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BSRHelpCommandPermissions   = EPermission.Viewers;
            [JsonProperty] internal bool        BSRHelpCommandEnabled       = true;
            [JsonProperty] internal string      BSRHelpCommand              = "bsrhelp";
            [JsonProperty] internal string      BSRHelpCommand_Reply        = "@$UserName To request a song, go to https://beatsaver.com/search and find a song, Click on the Twitch icon next to the song and paste this in the stream chat and I'll play it soon!.";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission LinkCommandPermissions  = EPermission.Viewers;
            [JsonProperty] internal bool        LinkCommandEnabled      = true;
            [JsonProperty] internal string      LinkCommand             = "link";
            [JsonProperty] internal string      LinkCommand_NoSong      = "@$UserName no song is being played right now!";
            [JsonProperty] internal string      LinkCommand_LastSong    = "@$UserName last song : $SongInfo $SongLink";
            [JsonProperty] internal string      LinkCommand_CurrentSong = "@$UserName current song : $SongInfo $SongLink";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission QueueCommandPermissions     = EPermission.Viewers;
            [JsonProperty] internal bool        QueueCommandEnabled         = true;
            [JsonProperty] internal int         QueueCommandShowSize        = 4;
            [JsonProperty] internal bool        QueueCommandCooldownPerUser = false;
            [JsonProperty] internal int         QueueCommandCooldown        = 10;
            [JsonProperty] internal string      QueueCommand                = "queue";
            [JsonProperty] internal string      QueueCommand_Empty          = "Song queue is empty!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission QueueStatusCommandPermissions   = EPermission.Viewers;
            [JsonProperty] internal bool        QueueStatusCommandEnabled       = true;
            [JsonProperty] internal string      QueueStatusCommand              = "queuestatus";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission WrongCommandCommandPermissions  = EPermission.Viewers;
            [JsonProperty] internal bool        WrongCommandEnabled             = true;
            [JsonProperty] internal string      WrongCommand                    = "wrong,oops,wrongsong";
            [JsonProperty] internal string      WrongCommand_NoSong             = "@$UserName you have no song in queue!";
            [JsonProperty] internal string      WrongCommand_NoSongFound        = "@$UserName you have no song in queue with the specified code!!";
            [JsonProperty] internal string      WrongCommand_Removed            = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is removed from queue!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission ModAddPermissions     = EPermission.Moderators;
            [JsonProperty] internal bool        ModAddCommandEnabled  = true;
            [JsonProperty] internal string      ModAddCommand         = "modadd";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission OpenCommandPermissions    = EPermission.Moderators;
            [JsonProperty] internal bool        OpenCommandEnabled        = true;
            [JsonProperty] internal string      OpenCommand               = "open";
            [JsonProperty] internal string      OpenCommand_AlreadyOpen   = "@$UserName Queue is already open!";
            [JsonProperty] internal string      OpenCommand_OK            = "Queue is now open!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission CloseCommandPermissions     = EPermission.Moderators;
            [JsonProperty] internal bool        CloseCommandEnabled         = true;
            [JsonProperty] internal string      CloseCommand                = "close";
            [JsonProperty] internal string      CloseCommand_AlreadyClosed  = "@$UserName Queue is already closed!";
            [JsonProperty] internal string      CloseCommand_OK             = "Queue is now closed!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission SabotageCloseCommandPermissions = EPermission.Moderators;
            [JsonProperty] internal bool        SabotageCommandEnabled          = true;
            [JsonProperty] internal string      SabotageCommand                 = "sabotage";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission SongMessageCommandPermissions   = EPermission.Moderators;
            [JsonProperty] internal bool        SongMessageCommandEnabled       = true;
            [JsonProperty] internal string      SongMessageCommand              = "songmsg";
            [JsonProperty] internal string      SongMessage_OK                  = "@$UserName message set!";
            [JsonProperty] internal string      SongMessage_NotFound            = $"@$UserName No song in queue found with the key or username \"$Subject\"!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission MoveToTopCommandPermissions = EPermission.Moderators;
            [JsonProperty] internal bool        MoveToTopCommandEnabled     = true;
            [JsonProperty] internal string      MoveToTopCommand            = "mtt";
            [JsonProperty] internal string      MoveToTopCommand_OK         = $"@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName requested by @$RequesterName is now on top of queue!";
            [JsonProperty] internal string      MoveToTopCommand_NotFound   = $"@$UserName No song in queue found with the key or username \"$Subject\"!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission AddToTopCommandPermissions  = EPermission.Moderators;
            [JsonProperty] internal bool        AddToTopCommandEnabled      = true;
            [JsonProperty] internal string      AddToTopCommand             = "att";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission RemoveCommandPermissions    = EPermission.Moderators;
            [JsonProperty] internal bool        RemoveCommandEnabled        = true;
            [JsonProperty] internal string      RemoveCommand               = "remove";
            [JsonProperty] internal string      RemoveCommand_OK            = $"@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName request by @RequesterName is removed from queue!";
            [JsonProperty] internal string      RemoveCommand_NotFound      = $"@$UserName No song in queue found with the key or username \"$Subject\"!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BsrBanCommandPermissions    = EPermission.Moderators;
            [JsonProperty] internal bool        BsrBanCommandEnabled        = true;
            [JsonProperty] internal string      BsrBanCommand               = "bsrban";
            [JsonProperty] internal string      BsrBanCommand_OK            = $"@$UserName User \"$l_TargetUserName\" was add to the requester ban list!";
            [JsonProperty] internal string      BsrBanCommand_AlreadyIn     = $"@$UserName User \"$l_TargetUserName\" is already in requester ban list!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BsrUnbanCommandPermissions  = EPermission.Moderators;
            [JsonProperty] internal bool        BsrUnbanCommandEnabled      = true;
            [JsonProperty] internal string      BsrUnbanCommand             = "bsrunban";
            [JsonProperty] internal string      BsrUnbanCommand_OK          = $"@$UserName User \"$TargetUserName\" was removed from the requester ban list!";
            [JsonProperty] internal string      BsrUnbanCommand_NotFound    = $"@$UserName User \"$TargetUserName\" is not in requester ban list!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BsrBanMapperCommandPermissions  = EPermission.Moderators;
            [JsonProperty] internal bool        BsrBanMapperCommandEnabled      = true;
            [JsonProperty] internal string      BsrBanMapperCommand             = "bsrbanmapper";
            [JsonProperty] internal string      BsrBanMapperCommand_OK          = $"@$UserName \"$MapperName\" was add to the mapper ban list!";
            [JsonProperty] internal string      BsrBanMapperCommand_AlreadyIn   = $"@$UserName \"$MapperName\" is already in the mapper ban list!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BsrUnbanMapperCommandPermissions    = EPermission.Moderators;
            [JsonProperty] internal bool        BsrUnbanMapperCommandEnabled        = true;
            [JsonProperty] internal string      BsrUnbanMapperCommand               = "bsrunbanmapper";
            [JsonProperty] internal string      BsrUnbanMapperCommand_OK            = $"@$UserName \"$MapperName\" was removed from the mapper ban list!";
            [JsonProperty] internal string      BsrUnbanMapperCommand_NotIn         = $"@$UserName \"$MapperName\" is not in the mapper ban list!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission RemapCommandPermissions = EPermission.Moderators;
            [JsonProperty] internal bool        RemapCommandEnabled    = true;
            [JsonProperty] internal string      RemapCommand           = "remap";
            [JsonProperty] internal string      RemapCommand_OK        = $"@$UserName All $Source requests will remap to $Target!";
            [JsonProperty] internal string      RemapCommand_OKRemoved = $"@$UserName Remap for song $Source removed!";
            [JsonProperty] internal string      RemapCommand_NotFound  = $"@$UserName No remap found for $Source!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission AllowlistCommandPermissions         = EPermission.Moderators;
            [JsonProperty] internal bool        AllowlistCommandEnabled             = true;
            [JsonProperty] internal string      AllowlistCommand                    = "allow";
            [JsonProperty] internal string      AllowlistCommand_OK                 = $"@$UserName All $BSRKey requests will be allowed!";
            [JsonProperty] internal string      AllowlistCommand_AlreadyAllowlisted = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is already allowlisted!";

            [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
                           internal EPermission BlocklistCommandPermissions         = EPermission.Moderators;
            [JsonProperty] internal bool        BlocklistCommandEnabled             = true;
            [JsonProperty] internal string      BlocklistCommand                    = "block";
            [JsonProperty] internal string      BlocklistCommand_AlreadyBlocklisted = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is already blacklisted!";
        }

        internal class _Messages
        {
            [JsonProperty] internal string NoPermissions = "@$UserName You have no power here!";
            [JsonProperty] internal string CommandFailed = "@$UserName command failed!";
            [JsonProperty] internal string OnCooldown    = "@$UserName command is on cooldown!";
            [JsonProperty] internal string BadSyntax     = "@$UserName invalid command, syntax is: $Syntax";
            [JsonProperty] internal string NextSong      = $"$SongName / $LevelAuthorName $Vote% (bsr $BSRKey) requested by @$RequesterName is next!";
        }

        [JsonProperty] internal bool Enabled = true;

        [JsonProperty] internal bool SafeMode2 = true;

        [JsonProperty] internal bool QueueOpen = true;

        [JsonProperty] internal int UserMaxRequest = 2;
        [JsonProperty] internal int VIPBonusRequest = 2;
        [JsonProperty] internal int SubscriberBonusRequest = 3;

        [JsonProperty] internal int HistorySize = 50;

        [JsonProperty] internal bool PlayPreviewMusic   = true;
        [JsonProperty] internal bool BigCoverArt        = true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Filters Filters = new _Filters();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _OverlayIntegration OverlayIntegration = new _OverlayIntegration();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Commands Commands = new _Commands();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Messages Messages = new _Messages();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/ChatRequest/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (Commands != null && !Commands.LinkCommand_LastSong.Contains("$SongLink"))
                Commands.LinkCommand_LastSong += " $SongLink";

            if (Commands != null && !Commands.LinkCommand_CurrentSong.Contains("$SongLink"))
                Commands.LinkCommand_CurrentSong += " $SongLink";
        }
    }
}
