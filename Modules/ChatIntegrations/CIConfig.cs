﻿using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations
{
    internal class CIConfig : BeatSaberPlus.SDK.Config.JsonConfig<CIConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/ChatIntegrations/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (BeatSaberPlus.Config.ChatIntegrations.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled = BeatSaberPlus.Config.ChatIntegrations.Enabled;

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.ChatIntegrations.OldConfigMigrated = true;
            Save();
        }
    }
}