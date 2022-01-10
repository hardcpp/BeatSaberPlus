using Newtonsoft.Json;
using System.Collections.Generic;

namespace BeatSaberPlus.Modules.ChatEmoteRain
{
    internal class CERConfig : SDK.Config.JsonConfig<CERConfig>
    {
        internal class _Emitter
        {
            [JsonProperty] internal bool Enabled = true;
            [JsonProperty] internal string Name = "New emitter";

            [JsonProperty] internal float Size = 1f;
            [JsonProperty] internal float Speed = 1f;

            [JsonProperty] internal float PosX =   0.00f;
            [JsonProperty] internal float PosY =  11.00f;
            [JsonProperty] internal float PosZ =   3.50f;
            [JsonProperty] internal float RotX =  90.00f;
            [JsonProperty] internal float RotY =   0.00f;
            [JsonProperty] internal float RotZ =   0.00f;
            [JsonProperty] internal float SizeX = 10.00f;
            [JsonProperty] internal float SizeY =  1.25f;
            [JsonProperty] internal float SizeZ =  2.00f;
        }

        internal class _ChatCommands
        {
            [JsonProperty] internal bool ModeratorPower = true;
            [JsonProperty] internal bool VIPPower = false;
            [JsonProperty] internal bool SubscriberPower = false;
        }

        [JsonProperty] internal bool Enabled = false;

        [JsonProperty] internal bool EnableMenu = true;
        [JsonProperty] internal float MenuSize = 0.4f;
        [JsonProperty] internal float MenuSpeed = 3f;

        [JsonProperty] internal bool EnableSong = true;
        [JsonProperty] internal float SongSize = 0.4f;
        [JsonProperty] internal float SongSpeed = 3f;

        [JsonProperty] internal int EmoteDelay = 8;

        [JsonProperty] internal bool SubRain = true;
        [JsonProperty] internal int SubRainEmoteCount = 20;

        [JsonProperty] internal bool ComboMode = false;
        [JsonProperty] internal int ComboModeType = 0;
        [JsonProperty] internal float ComboTimer = 5f;
        [JsonProperty] internal int ComboCount = 3;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<_Emitter> MenuEmitters = new List<_Emitter>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<_Emitter> SongEmitters = new List<_Emitter>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _ChatCommands ChatCommands = new _ChatCommands();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/ChatEmoteRain/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (p_OnCreation)
                Enabled = Config.ChatEmoteRain.Enabled;

            if (p_OnCreation)
            {
                MenuEmitters.Add(new _Emitter()
                {
                    Enabled = true,
                    Name = "Top",
                    Size =   1.00f,
                    Speed =  1.00f,
                    PosX =   0.00f,
                    PosY =  11.00f,
                    PosZ =   3.50f,
                    RotX =  90.00f,
                    RotY =   0.00f,
                    RotZ =   0.00f,
                    SizeX = 10.00f,
                    SizeY =  1.25f,
                    SizeZ =  2.00f
                });
                SongEmitters.Add(new _Emitter()
                {
                    Enabled = true,
                    Name = "LeftSide",
                    Size =   1.00f,
                    Speed =  1.00f,
                    PosX =  -5.10f,
                    PosY =   8.60f,
                    PosZ =   9.50f,
                    RotX =  90.00f,
                    RotY = 118.00f,
                    RotZ =   0.00f,
                    SizeX =  8.30f,
                    SizeY =  4.50f,
                    SizeZ =  4.40f
                });
                SongEmitters.Add(new _Emitter()
                {
                    Enabled = true,
                    Name = "RightSide",
                    Size =   1.00f,
                    Speed =  1.00f,
                    PosX =   5.10f,
                    PosY =   8.60f,
                    PosZ =   9.50f,
                    RotX =  90.00f,
                    RotY = 242.00f,
                    RotZ =   0.00f,
                    SizeX =  8.30f,
                    SizeY =  4.50f,
                    SizeZ =  4.40f
                });
            }

            ////////////////////////////////////////////////////////////////////////////

            Save();
        }
    }
}
