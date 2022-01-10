﻿using Newtonsoft.Json;
using System;
using System.Linq;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public class MapDetail
    {
        [JsonProperty] public string id = "";
        [JsonProperty] public string name = "";
        [JsonProperty] public string description = "";
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public UserDetail uploader = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapDetailMetadata metadata = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapStats stats = null;
        [JsonProperty] public string uploaded = "";
        [JsonProperty] public bool automapper = false;
        [JsonProperty] public bool ranked = false;
        [JsonProperty] public bool qualified = false;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapVersion[] versions = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [JsonIgnore]
        public bool Partial = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Partial BeatMap from ID
        /// </summary>
        /// <param name="p_Key">ID of the BeatMap</param>
        /// <returns></returns>
        public static MapDetail PartialFromKey(string p_Key)
        {
            return new MapDetail() { id = p_Key };
        }
        /// <summary>
        /// Populate partial BeatMap
        /// </summary>
        /// <param name="p_Callback">Completion/failure callback</param>
        public void Populate(Action<bool> p_Callback)
        {
            BeatMapsClient.PopulateOnlineByKey(this, p_Callback);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is the BeatMap valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (Partial || string.IsNullOrEmpty(id))
                return false;

            var l_Version = SelectMapVersion();
            if (l_Version == null)
                return false;

            return l_Version.diffs != null && l_Version.diffs.Length >= 1;
        }
        /// <summary>
        /// Select default valid version
        /// </summary>
        /// <returns></returns>
        public MapVersion SelectMapVersion()
        {
            if (versions == null || versions.Length == 0)
                return null;

            return versions.Last();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get upload time
        /// </summary>
        /// <returns></returns>
        public DateTime GetUploadTime()
        {
            if (!string.IsNullOrEmpty(uploaded) && Misc.Time.TryParseInternational(uploaded, out var l_Date))
                return l_Date;
            else if (string.IsNullOrEmpty(uploaded) && versions != null & versions.Length >= 1
                && Misc.Time.TryParseInternational(SelectMapVersion().createdAt, out l_Date))
                return l_Date;

            return Misc.Time.FromUnixTime(0);
        }
    }
}
