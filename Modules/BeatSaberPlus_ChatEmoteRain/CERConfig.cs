using Newtonsoft.Json;
using System.Collections.Generic;

using EmitterConfig = CP_SDK.Unity.Components.EnhancedImageParticleEmitter.EmitterConfig;

namespace ChatPlexMod_ChatEmoteRain
{
    internal class CERConfig : CP_SDK.Config.JsonConfig<CERConfig>
    {
        internal class _ChatCommands
        {
            [JsonProperty] internal bool ModeratorPower = true;
            [JsonProperty] internal bool VIPPower = false;
            [JsonProperty] internal bool SubscriberPower = false;
            [JsonProperty] internal bool UserPower = false;
        }

        [JsonProperty] internal bool Enabled = true;

        [JsonProperty] internal bool EnableMenu = true;
        [JsonProperty] internal float MenuSize = 0.4f;
        [JsonProperty] internal float MenuSpeed = 3f;

        [JsonProperty] internal bool EnableSong = true;
        [JsonProperty] internal float SongSize = 0.4f;
        [JsonProperty] internal float SongSpeed = 3f;

        [JsonProperty] internal int EmoteDelay = 8;

        [JsonProperty] internal bool SubRain = true;
        [JsonProperty] internal int SubRainEmoteCount = 20;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<EmitterConfig> MenuEmitters = new List<EmitterConfig>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<EmitterConfig> SongEmitters = new List<EmitterConfig>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _ChatCommands ChatCommands = new _ChatCommands();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}/ChatEmoteRain/Config";

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
                MenuEmitters.Add(new EmitterConfig()
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
                SongEmitters.Add(new EmitterConfig()
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
                SongEmitters.Add(new EmitterConfig()
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
