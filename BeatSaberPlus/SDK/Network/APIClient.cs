using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Network
{
    /// <summary>
    /// API client class
    /// </summary>
    public sealed class APIClient
    {
        /// <summary>
        /// API client
        /// </summary>
        private HttpClient m_Client = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maximum retry attempt
        /// </summary>
        public int MaxRetry = 5;
        /// <summary>
        /// Delay between each retry
        /// </summary>
        public int RetryInterval = 5 * 1000;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Client public accesor
        /// </summary>
        public HttpClient InternalClient => m_Client;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_BaseAddress">Base address</param>
        /// <param name="p_TimeOut">Maximum timeout</param>
        /// <param name="p_KeepAlive">Should keep alive the connection</param>
        /// <param name="p_ForceCacheDiscard">Should force cache discard</param>
        public APIClient(string p_BaseAddress, TimeSpan p_TimeOut, bool p_KeepAlive = true, bool p_ForceCacheDiscard = true)
        {
            HttpClientHandler l_Handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            m_Client = new HttpClient(l_Handler)
            {
                Timeout = p_TimeOut,
            };

            if (!string.IsNullOrEmpty(p_BaseAddress))
                m_Client.BaseAddress = new Uri(p_BaseAddress);

            if (p_ForceCacheDiscard)
            {
                m_Client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache         = true,
                    NoStore         = false,
                    MustRevalidate  = true,
                    ProxyRevalidate = true,
                    MaxAge          = TimeSpan.FromSeconds(0),
                    SharedMaxAge    = TimeSpan.FromMilliseconds(0),
                    MaxStaleLimit   = TimeSpan.FromMilliseconds(0)
                };
            }
            m_Client.DefaultRequestHeaders.ConnectionClose = p_KeepAlive;
            m_Client.DefaultRequestHeaders.Add("User-Agent", $"BeatSaberPlus/{Plugin.Version.Major}.{Plugin.Version.Minor}.{Plugin.Version.Patch}");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do Async get query
        /// </summary>
        /// <param name="p_Content">Request content</param>
        /// <returns>HTTPResponse</returns>
        public async Task<APIResponse> GetAsync(string p_URL, CancellationToken p_Token, bool p_DontRetry = false)
        {
#if DEBUG
            Logger.Instance.Debug("[SDK.Network][APIClient.GetAsync] GET " + p_URL);
#endif
            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.GetAsync(p_URL, p_Token);

                    if (l_Reply != null && l_Reply.StatusCode == (HttpStatusCode)429)
                    {
                        var l_Limits = RateLimitInfo.FromHttp(l_Reply);
                        if (l_Limits != null)
                        {
                            int l_TotalMilliseconds = (int)(l_Limits.Reset - DateTime.Now).TotalMilliseconds;
                            if (l_TotalMilliseconds > 0)
                            {
                                await Task.Delay(l_TotalMilliseconds).ConfigureAwait(false);
                                continue;
                            }
                        }
                    }

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer            = await l_Reply.Content.ReadAsByteArrayAsync();
                        var l_ResponseBuffer    = Encoding.UTF8.GetString(l_Buffer, 0, l_Buffer.Length);

                        return new APIResponse(l_Reply, l_Buffer, l_ResponseBuffer);
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
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval);
            }

            return null;
        }
        /// <summary>
        /// Do Async post query
        /// </summary>
        /// <param name="p_Content">Request content</param>
        /// <returns>HTTPResponse</returns>
        public async Task<APIResponse> PostAsync(string p_URL, HttpContent p_Content, CancellationToken p_Token, bool p_DontRetry = false)
        {
            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.PostAsync(p_URL, p_Content, p_Token);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer            = await l_Reply.Content.ReadAsByteArrayAsync();
                        var l_ResponseBuffer    = Encoding.UTF8.GetString(l_Buffer, 0, l_Buffer.Length);

                        return new APIResponse(l_Reply, l_Buffer, l_ResponseBuffer);
                    }
                }
                catch (System.Exception)
                {
                    /// Do nothing here
                }

                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                if (l_Reply != null)
                    Logger.Instance.Error($"[SDK.Network][APIClient.PostAsync] Request failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    Logger.Instance.Error($"[SDK.Network][APIClient.PostAsync] Request failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval);
            }

            return null;
        }
        /// <summary>
        /// Do Async patch query
        /// </summary>
        /// <param name="p_Content">Request content</param>
        /// <returns>HTTPResponse</returns>
        public async Task<APIResponse> PatchAsync(string p_URL, HttpContent p_Content, CancellationToken p_Token, bool p_DontRetry = false)
        {
            p_Token.ThrowIfCancellationRequested();

            var l_Request = new HttpRequestMessage(new HttpMethod("PATCH"), p_URL)
            {
                Content = p_Content
            };

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.SendAsync(l_Request, p_Token);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer = await l_Reply.Content.ReadAsByteArrayAsync();
                        var l_ResponseBuffer = Encoding.UTF8.GetString(l_Buffer, 0, l_Buffer.Length);

                        return new APIResponse(l_Reply, l_Buffer, l_ResponseBuffer);
                    }
                }
                catch (System.Exception)
                {
                    /// Do nothing here
                }

                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                if (l_Reply != null)
                    Logger.Instance.Error($"[SDK.Network][APIClient.PatchAsync] Request failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    Logger.Instance.Error($"[SDK.Network][APIClient.PatchAsync] Request failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval);
            }

            return null;
        }
        /// <summary>
        /// Do Async delete query
        /// </summary>
        /// <param name="p_Content">Request content</param>
        /// <returns>HTTPResponse</returns>
        public async Task<APIResponse> DeleteAsync(string p_URL, CancellationToken p_Token, bool p_DontRetry = false)
        {
            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.DeleteAsync(p_URL, p_Token);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer = await l_Reply.Content.ReadAsByteArrayAsync();
                        var l_ResponseBuffer = Encoding.UTF8.GetString(l_Buffer, 0, l_Buffer.Length);

                        return new APIResponse(l_Reply, l_Buffer, l_ResponseBuffer);
                    }
                }
                catch (System.Exception)
                {
                    /// Do nothing here
                }

                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                if (l_Reply != null)
                    Logger.Instance.Error($"[SDK.Network][APIClient.DeleteAsync] Request failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    Logger.Instance.Error($"[SDK.Network][APIClient.DeleteAsync] Request failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval);
            }

            return null;
        }
    }
}
