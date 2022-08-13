using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public enum EMapVersionStates
    {
        UNK,
        Uploaded,
        Testplay,
        Published,
        Feedback
    }

    public class MapVersion
    {
        [JsonProperty] public string hash = "";
        [JsonProperty] public string key = "";
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty] public EMapVersionStates state =  EMapVersionStates.UNK;
        [JsonProperty] public string createdAt = "";
        [JsonProperty] public int sageScore = 0;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapDifficulty[] diffs = null;
        [JsonProperty] public string downloadURL = "";
        [JsonProperty] public string coverURL = "";
        [JsonProperty] public string previewURL = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get distinct list of characteristics in order
        /// </summary>
        /// <returns></returns>
        public List<string> GetCharacteristicsInOrder()
        {
            List<string> l_Result = new List<string>();

            if (diffs != null)
            {
                l_Result = diffs.Select(x => x.characteristic)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => Game.Levels.GetCharacteristicOrdering(x))
                    .ToList();
            }

            return l_Result;
        }
        /// <summary>
        /// Get all difficulties for a specific Characteristics
        /// </summary>
        /// <param name="p_Characteristic">Target characteristic</param>
        /// <returns></returns>
        public List<MapDifficulty> GetDifficultiesPerCharacteristic(string p_Characteristic)
        {
            List<MapDifficulty> l_Result = new List<MapDifficulty>();

            if (diffs != null)
            {
                foreach (var l_Diff in diffs)
                {
                    if (Levels.SanitizeCharacteristic(l_Diff.characteristic).ToLower() != p_Characteristic.ToLower())
                        continue;

                    l_Result.Add(l_Diff);
                }
            }

            return l_Result;
        }
        /// <summary>
        /// Get all difficulties for a specific Characteristics
        /// </summary>
        /// <param name="p_Characteristic">Target characteristic</param>
        /// <returns></returns>
        public List<MapDifficulty> GetDifficultiesPerCharacteristicSerializedName(string p_CharacteristicSerializedName)
        {
            List<MapDifficulty> l_Result = new List<MapDifficulty>();

            if (diffs != null)
            {
                foreach (var l_Diff in diffs)
                {
                    var l_SerializedName    = Levels.SanitizeCharacteristic(l_Diff.characteristic);
                    var l_CharacteristicSO  = Levels.GetCharacteristicSOBySerializedName(l_SerializedName);

                    if (l_CharacteristicSO.serializedName != p_CharacteristicSerializedName)
                        continue;

                    l_Result.Add(l_Diff);
                }
            }

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get cover image bytes
        /// </summary>
        /// <param name="p_Callback">Callback(p_Valid, p_Bytes)</param>
        public void CoverImageBytes(Action<bool, byte[]> p_Callback)
        {
            BeatMapsClient.APIClient.GetAsync(coverURL, CancellationToken.None).ContinueWith((p_APIResult) =>
            {
                try
                {
                    if (p_APIResult == null || p_APIResult.IsCanceled || p_APIResult.Status != TaskStatus.RanToCompletion || p_APIResult.Result == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    var l_Response = p_APIResult.Result;
                    if (!l_Response.IsSuccessStatusCode || l_Response.BodyBytes == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    p_Callback?.Invoke(true, l_Response.BodyBytes);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game.BeatMaps][Version.CoverImageBytes] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Zip archive bytes
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_ShouldRetry">Should retry in case of failure?</param>
        /// <returns></returns>
        public Task<byte[]> ZipBytes(CancellationToken p_Token, IProgress<double> p_Progress, bool p_ShouldRetry = false)
            => BeatMapsClient.APIClient.DownloadAsync(downloadURL, p_Token, p_Progress, p_ShouldRetry);
    }
}
