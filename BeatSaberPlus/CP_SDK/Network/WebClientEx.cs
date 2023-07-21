using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK.Network
{
    /// <summary>
    /// WebClient client class
    /// </summary>
    public sealed class WebClientEx : IWebClient
    {
        /// <summary>
        /// Global client instance
        /// </summary>
        public static readonly WebClientEx GlobalClient = new WebClientEx("", TimeSpan.FromSeconds(10), true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// API client
        /// </summary>
        private HttpClient m_Client = null;
        /// <summary>
        /// Cookie container
        /// </summary>
        private CookieContainer m_CookieContainer = new CookieContainer();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maximum retry attempt
        /// </summary>
        public int MaxRetry = 5;
        /// <summary>
        /// Delay between each retry
        /// </summary>
        public int RetryInterval = 5;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Client public accessor
        /// </summary>
        public HttpClient InternalClient => m_Client;
        /// <summary>
        /// Cookies container
        /// </summary>
        public CookieContainer Cookies => m_CookieContainer;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_BaseAddress">Base address</param>
        /// <param name="p_TimeOut">Maximum timeout</param>
        /// <param name="p_KeepAlive">Should keep alive the connection</param>
        /// <param name="p_ForceCacheDiscard">Should force cache discard</param>
        public WebClientEx(string p_BaseAddress, TimeSpan p_TimeOut, bool p_KeepAlive = true, bool p_ForceCacheDiscard = false)
        {
            HttpClientHandler l_Handler = new HttpClientHandler()
            {
                AutomaticDecompression  = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer         = m_CookieContainer
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
            m_Client.DefaultRequestHeaders.Add("User-Agent", "ChatPlexAPISDK_ApiClient/6.0.3");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do Async GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        public async void GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null)
            => await DoRequest("GetAsync", "GET", p_URL, null, null, p_Token, p_Callback, p_DontRetry, p_Progress).ConfigureAwait(false);
        public async void DownloadAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null)
            => await DoRequest("DownloadAsync", "GET", p_URL, null, null, p_Token, p_Callback, p_DontRetry, p_Progress).ConfigureAwait(false);
        /// <summary>
        /// Do Async POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Optional content to post</param>
        /// <param name="p_ContentType">Content type</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void PostAsync(string p_URL, HttpContent p_Content, string p_ContentType, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("PostAsync", "POST", p_URL, p_Content, p_ContentType, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);
        /// <summary>
        /// Do Async PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Optional content to post</param>
        /// <param name="p_ContentType">Content type</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void PatchAsync(string p_URL, HttpContent p_Content, string p_ContentType, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("PatchAsync", "PATCH", p_URL, p_Content, p_ContentType, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);
        /// <summary>
        /// Do Async DELETE query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void DeleteAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("DeleteAsync", "DELETE", p_URL, null, null, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);

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

            if (!p_URL.Contains("://"))
                l_Result = m_Client.BaseAddress + l_Result;

            if (l_Result.Contains("?"))
                l_Result = l_Result.Substring(0, l_Result.IndexOf("?"));

            return l_Result;
        }
        /// <summary>
        /// Do request
        /// </summary>
        /// <param name="p_DebugName">Method name for logs</param>
        /// <param name="p_HttpMethod">Http method</param>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Optional content to post</param>
        /// <param name="p_ContentType">Content type</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        private async Task<bool> DoRequest( string              p_DebugName,
                                            string              p_HttpMethod,
                                            string              p_URL,
                                            HttpContent         p_Content,
                                            string              p_ContentType,
                                            CancellationToken   p_Token,
                                            Action<WebResponse> p_Callback,
                                            bool                p_DontRetry,
                                            IProgress<float>    p_Progress)
        {
#if DEBUG
            ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClientEx.{p_DebugName}] {p_HttpMethod} " + p_URL);
#endif

            var l_Reply = null as WebResponse;
            for (int l_Retry = 0; l_Retry < MaxRetry; l_Retry++)
            {
                if (p_Token.IsCancellationRequested)
                    break;

                var l_Response = null as HttpResponseMessage;
                try
                {
                    l_Reply = null;
                    switch (p_HttpMethod)
                    {
                        case "GET":
                            l_Response = await m_Client.GetAsync(p_URL, p_Token).ConfigureAwait(false);
                            break;

                        case "POST":
                            p_Content.Headers.ContentType.MediaType = p_ContentType;
                            l_Response = await m_Client.PostAsync(p_URL, p_Content, p_Token).ConfigureAwait(false);
                            break;

                        case "PATCH":
                            p_Content.Headers.ContentType.MediaType = p_ContentType;
                            l_Response = await m_Client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), p_URL) { Content = p_Content, }, p_Token).ConfigureAwait(false);
                            break;

                        case "DELETE":
                            l_Response = await m_Client.DeleteAsync(p_URL, p_Token).ConfigureAwait(false);
                            break;
                    }

                    if (p_Token.IsCancellationRequested)
                        break;

                    l_Reply = new WebResponse(l_Response);

                    if (!l_Reply.IsSuccessStatusCode && l_Reply.StatusCode == (HttpStatusCode)429)
                    {
                        var l_Limits = RateLimitInfo.Get(l_Response);
                        if (l_Limits != null)
                        {
                            int l_TotalMilliseconds = (int)(l_Limits.Reset - DateTime.Now).TotalMilliseconds;
                            if (l_TotalMilliseconds > 0)
                            {
                                ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClientEx.{p_DebugName}] Request {SafeURL(p_URL)} was rate limited, retrying in {l_TotalMilliseconds}ms...");

                                await Task.Delay(l_TotalMilliseconds).ConfigureAwait(false);
                                continue;
                            }
                        }
                    }

                    if ((l_Reply.IsSuccessStatusCode || (!l_Reply.ShouldRetry || p_DontRetry)))
                        //&& (l_Response.Content.Headers.ContentLength > 0 || (l_Response.Content.Headers.ContentType != null && l_Response.Content.Headers.ContentLength == null)))
                    {
                        l_Reply.Populate(await l_Response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
                    }

                    if (!l_Reply.IsSuccessStatusCode)
                    {
                        if (!l_Reply.ShouldRetry || p_DontRetry)
                        {
                            ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClientEx.{p_DebugName}] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", not retrying");
                            break;
                        }

                        ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClientEx.{p_DebugName}] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in {RetryInterval} seconds...");

                        await Task.Delay(RetryInterval * 1000).ConfigureAwait(false);

                        continue;
                    }

                    break;
                }
                catch (System.Exception)
                {
                    /// Do nothing here
                }
                finally
                {
                    l_Response?.Dispose();
                }
            }

            if (!p_Token.IsCancellationRequested)
                p_Callback?.Invoke(l_Reply);

            return true;
        }
    }
}
