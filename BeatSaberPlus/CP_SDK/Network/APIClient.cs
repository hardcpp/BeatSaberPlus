using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK.Network
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
            m_Client.DefaultRequestHeaders.ConnectionClose = !p_KeepAlive;
            m_Client.DefaultRequestHeaders.Add("User-Agent", ChatPlexSDK.NetworkUserAgent);
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
            ChatPlexUnitySDK.Logger.Debug("[CP_SDK.Network][APIClient.GetAsync] GET " + p_URL);
#endif
            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.GetAsync(p_URL, p_Token).ConfigureAwait(false);

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
                        var l_Buffer            = await l_Reply.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.GetAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.GetAsync] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
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
                    l_Reply = await m_Client.PostAsync(p_URL, p_Content, p_Token).ConfigureAwait(false);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer            = await l_Reply.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.PostAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.PostAsync] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
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
                    l_Reply = await m_Client.SendAsync(l_Request, p_Token).ConfigureAwait(false);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer = await l_Reply.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.PatchAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.PatchAsync] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
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
                    l_Reply = await m_Client.DeleteAsync(p_URL, p_Token).ConfigureAwait(false);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        /// Read reply
                        var l_Buffer = await l_Reply.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.DeleteAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.DeleteAsync] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Download async
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_ShouldRetry">Should retry in case of failure?</param>
        /// <returns></returns>
        public async Task<byte[]> DownloadAsync(string p_URL, CancellationToken p_Token, IProgress<double> p_Progress, bool p_DontRetry = false)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger.Debug("[CP_SDK.Network][APIClient.DownloadAsync] GET " + p_URL);
#endif
            p_Token.ThrowIfCancellationRequested();

            HttpResponseMessage l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.GetAsync(p_URL, HttpCompletionOption.ResponseHeadersRead, p_Token);

                    if (p_DontRetry || l_Reply.IsSuccessStatusCode || l_Reply.StatusCode == HttpStatusCode.NotFound || l_Reply.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var l_MemoryStream = new MemoryStream();
                        var l_Stream = await l_Reply.Content.ReadAsStreamAsync().ConfigureAwait(false);

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
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.DownloadAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in 5 seconds...");
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.DownloadAsync] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
            }

            return null;
        }
        /// <summary>
        /// Download async one shot
        /// </summary>
        /// <param name="p_Content">Request content</param>
        /// <returns>HTTPResponse</returns>
        public async Task<byte[]> DownloadAsyncOneShot(string p_URL, CancellationToken p_Token, bool p_DontRetry = false)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger.Debug("[CP_SDK.Network][APIClient.DownloadAsyncOneShot] GET " + p_URL);
#endif
            p_Token.ThrowIfCancellationRequested();

            byte[] l_Reply = null;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                try
                {
                    l_Reply = await m_Client.GetByteArrayAsync(p_URL).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    /// Do nothing here
                }

                if (p_Token.IsCancellationRequested)
                    p_Token.ThrowIfCancellationRequested();

                if (l_Reply != null)
                    return l_Reply;

                ChatPlexSDK.Logger.Error($"[CP_SDK.Network][APIClient.DownloadAsyncOneShot] Request {SafeURL(p_URL)} failed, next try in 5 seconds...");

                /// Short exit
                if (p_DontRetry)
                    return null;

                /// Wait 5 seconds
                await Task.Delay(RetryInterval).ConfigureAwait(false);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Safe URL parsing
        /// </summary>
        /// <param name="p_URL"></param>
        /// <returns></returns>
        private string SafeURL(string p_URL)
        {
            var l_Result = p_URL;

            if (!p_URL.ToLower().StartsWith("http"))
                l_Result = m_Client.BaseAddress + l_Result;

            if (l_Result.Contains("?"))
                l_Result = l_Result.Substring(0, l_Result.IndexOf("?"));

            return l_Result;
        }
    }
}
