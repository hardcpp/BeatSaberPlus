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
                    Logger.Instance.Error("[SDK.Game.BeatMaps][Version.CoverImageBytes] Error :");
                    Logger.Instance.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            });
        }
        /// <summary>
        /// Get Zip archive bytes
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_ShouldRetry">Should retry in case of failure?</param>
        /// <returns></returns>
        public async Task<byte[]> ZipBytes(CancellationToken p_Token, IProgress<double> p_Progress, bool p_ShouldRetry = false)
        {
            var l_APIClient     = BeatMapsClient.APIClient;
            var l_HttpClient    = l_APIClient.InternalClient;

            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < l_APIClient.MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await l_HttpClient.GetAsync(downloadURL, HttpCompletionOption.ResponseHeadersRead, p_Token);

                    if (!p_ShouldRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound)
                    {
                        var l_MemoryStream  = new MemoryStream();
                        var l_Stream        = await l_Reply.Content.ReadAsStreamAsync().ConfigureAwait(false);

                        byte[] l_Buffer = new byte[8192];
                        long? l_ContentLength = l_Reply.Content.Headers.ContentLength;
                        long l_TotalRead = 0;
                        p_Progress?.Report(0.0);

                        while (true)
                        {
                            int l_ReadBytes;
                            if ((l_ReadBytes = await l_Stream.ReadAsync(l_Buffer, 0, l_Buffer.Length, p_Token).ConfigureAwait(false)) > 0)
                            {
                                if (!p_Token.IsCancellationRequested)
                                {
                                    if (l_ContentLength.HasValue)
                                        p_Progress?.Report((double)l_TotalRead / (double)l_ContentLength.Value);

                                    await l_MemoryStream.WriteAsync(l_Buffer, 0, l_ReadBytes, p_Token).ConfigureAwait(false);
                                    l_TotalRead += (long)l_ReadBytes;
                                }
                                else
                                    break;
                            }
                            else
                            {
                                p_Progress?.Report(1.0);
                                return l_MemoryStream.ToArray();
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    /// Do nothing here
                }

                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                if (l_Reply != null)
                    Logger.Instance.Error($"[SDK.Network][APIClient.GetAsync] Request failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    Logger.Instance.Error($"[SDK.Network][APIClient.GetAsync] Request failed, next try in 5 seconds...");

                /// Short exit
                if (p_ShouldRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(l_APIClient.RetryInterval);
            }

            return null;
        }
    }
}
