using Newtonsoft.Json;

namespace BeatSaberPlus_ChatRequest
{
    internal class CRConfig : BeatSaberPlus.SDK.Config.JsonConfig<CRConfig>
    {
        internal class _Filters
        {
            [JsonProperty] internal bool NoBeatSage = false;

            [JsonProperty] internal bool NPSMin = false;
            [JsonProperty] internal int NPSMinV = 0;

            [JsonProperty] internal bool NPSMax = false;
            [JsonProperty] internal int NPSMaxV = 30;

            [JsonProperty] internal bool NJSMin = false;
            [JsonProperty] internal int NJSMinV = 0;

            [JsonProperty] internal bool NJSMax = false;
            [JsonProperty] internal int NJSMaxV = 30;

            [JsonProperty] internal bool DurationMax = false;
            [JsonProperty] internal int DurationMaxV = 3;

            [JsonProperty] internal bool VoteMin = false;
            [JsonProperty] internal float VoteMinV = 0.5f;

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
            [JsonProperty] internal bool   BSRCommandEnabled        = true;
            [JsonProperty] internal string BSRCommand               = "bsr";
            [JsonProperty] internal string BSRCommand_UserBanned    = "@$UserName you are not allowed to make requests!";
            [JsonProperty] internal string BSRCommand_QueueClosed   = "@$UserName the queue is closed!";
            [JsonProperty] internal string BSRCommand_Search0Result = "@$UserName your search $Search produced 0 results!";
            [JsonProperty] internal string BSRCommand_SearchResults = "@$UserName your search $Search produced $Count results: $Results";
            [JsonProperty] internal string BSRCommand_Blacklisted   = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is blacklisted!";
            [JsonProperty] internal string BSRCommand_AlreadyQueued = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is already in queue!";
            [JsonProperty] internal string BSRCommand_RequestLimit  = "@$UserName you already have $UserRequestCount on the queue. $UserType are limited to $UserTypeLimit request(s).";
            [JsonProperty] internal string BSRCommand_AlreadyPlayed = "@$UserName this song was already requested this session!";
            [JsonProperty] internal string BSRCommand_NotFound      = "@$UserName map $BSRKey not found.";
            [JsonProperty] internal string BSRCommand_MapperBanned  = "@$UserName $UploaderName's maps are not allowed!";
            [JsonProperty] internal string BSRCommand_RequestOK     = "(bsr $BSRKey) $SongName / $LevelAuthorName $Vote% requested by @$UserName added to queue.";

            [JsonProperty] internal bool   BSRHelpCommandEnabled    = true;
            [JsonProperty] internal string BSRHelpCommand           = "bsrhelp";
            [JsonProperty] internal string BSRHelpCommand_Reply     = "@$UserName To request a song, go to https://beatsaver.com/search and find a song, Click on \"Copy !bsr\" button and paste this on the stream and I'll play it soon!.";

            [JsonProperty] internal bool   LinkCommandEnabled       = true;
            [JsonProperty] internal string LinkCommand              = "link";
            [JsonProperty] internal string LinkCommand_NoSong       = "@$UserName no song is being played right now!";
            [JsonProperty] internal string LinkCommand_LastSong     = "@$UserName last song : $SongInfo";
            [JsonProperty] internal string LinkCommand_CurrentSong  = "@$UserName current song : $SongInfo";

            [JsonProperty] internal bool   QueueCommandEnabled      = true;
            [JsonProperty] internal string QueueCommand             = "queue";
            [JsonProperty] internal string QueueCommand_Cooldown    = "@$UserName queue command is on cooldown!";
            [JsonProperty] internal string QueueCommand_Empty       = "Song queue is empty!";

            [JsonProperty] internal bool   QueueStatusCommandEnabled = true;
            [JsonProperty] internal string QueueStatusCommand        = "queuestatus";

            [JsonProperty] internal bool   WrongCommandEnabled      = true;
            [JsonProperty] internal string WrongCommand             = "wrong,oops,wrongsong";
            [JsonProperty] internal string WrongCommand_NoSong      = "@$UserName you have no song in queue!";
            [JsonProperty] internal string WrongCommand_NoSongFound = "@$UserName you have no song in queue with the specified code!!";

            [JsonProperty] internal bool   Moderator_ModAddCommandEnabled   = true;
            [JsonProperty] internal string Moderator_ModAddCommand          = "modadd";

            [JsonProperty] internal bool   Moderator_OpenCommandEnabled = true;
            [JsonProperty] internal string Moderator_OpenCommand        = "open";

            [JsonProperty] internal bool   Moderator_CloseCommandEnabled    = true;
            [JsonProperty] internal string Moderator_CloseCommand           = "close";

            [JsonProperty] internal bool   Moderator_SabotageCommandEnabled = true;
            [JsonProperty] internal string Moderator_SabotageCommand        = "sabotage";

            [JsonProperty] internal bool   Moderator_SongMessageCommandEnabled  = true;
            [JsonProperty] internal string Moderator_SongMessageCommand         = "songmsg";

            [JsonProperty] internal bool   Moderator_MoveToTopCommandEnabled    = true;
            [JsonProperty] internal string Moderator_MoveToTopCommand           = "mtt";

            [JsonProperty] internal bool   Moderator_AddToTopCommandEnabled = true;
            [JsonProperty] internal string Moderator_AddToTopCommand        = "att";

            [JsonProperty] internal bool   Moderator_RemoveCommandEnabled   = true;
            [JsonProperty] internal string Moderator_RemoveCommand          = "remove";

            [JsonProperty] internal bool   Moderator_BsrBanCommandEnabled   = true;
            [JsonProperty] internal string Moderator_BsrBanCommand          = "bsrban";

            [JsonProperty] internal bool   Moderator_BsrUnbanCommandEnabled = true;
            [JsonProperty] internal string Moderator_BsrUnbanCommand        = "bsrunban";

            [JsonProperty] internal bool   Moderator_BsrBanMapperCommandEnabled = true;
            [JsonProperty] internal string Moderator_BsrBanMapperCommand        = "bsrbanmapper";

            [JsonProperty] internal bool   Moderator_BsrUnbanMapperCommandEnabled   = true;
            [JsonProperty] internal string Moderator_BsrUnbanMapperCommand          = "bsrunbanmapper";

            [JsonProperty] internal bool   Moderator_RemapCommandEnabled    = true;
            [JsonProperty] internal string Moderator_RemapCommand           = "remap";

            [JsonProperty] internal bool   Moderator_AllowCommandEnabled    = true;
            [JsonProperty] internal string Moderator_AllowCommand           = "allow";

            [JsonProperty] internal bool   Moderator_BlockCommandEnabled    = true;
            [JsonProperty] internal string Moderator_BlockCommand           = "block";
        }

        [JsonProperty] internal bool Enabled = true;

        [JsonProperty] internal bool SafeMode = false;

        [JsonProperty] internal bool QueueOpen = true;

        [JsonProperty] internal int UserMaxRequest = 2;
        [JsonProperty] internal int VIPBonusRequest = 2;
        [JsonProperty] internal int SubscriberBonusRequest = 3;

        [JsonProperty] internal int HistorySize = 50;

        [JsonProperty] internal bool PlayPreviewMusic = true;

        [JsonProperty] internal bool ModeratorPower = true;

        [JsonProperty] internal int QueueCommandShowSize = 4;
        [JsonProperty] internal int QueueCommandCooldown = 10;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Filters Filters = new _Filters();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _OverlayIntegration OverlayIntegration = new _OverlayIntegration();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Commands Commands = new _Commands();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/ChatRequest/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            Save();
        }
    }
}
