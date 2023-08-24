using System;
using System.IO;
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
    public sealed class WebClientCore : IWebClient
    {
        /// <summary>
        /// Global client instance
        /// </summary>
        public static readonly WebClientCore GlobalClient = new WebClientCore("", TimeSpan.FromSeconds(10), true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private HttpClient      m_Client            = null;
        private CookieContainer m_CookieContainer   = new CookieContainer();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maximum retry attempt
        /// </summary>
        public int MaxRetry = 2;
        /// <summary>
        /// Delay between each retry
        /// </summary>
        public int RetryInterval = 5;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_BaseAddress">Base address</param>
        /// <param name="p_TimeOut">Maximum timeout</param>
        /// <param name="p_KeepAlive">Should keep alive the connection</param>
        /// <param name="p_ForceCacheDiscard">Should force cache discard</param>
        public WebClientCore(string p_BaseAddress, TimeSpan p_TimeOut, bool p_KeepAlive = true, bool p_ForceCacheDiscard = false)
        {
            HttpClientHandler l_Handler = new HttpClientHandler()
            {
                AutomaticDecompression  = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer         = m_CookieContainer
            };

            m_Client = new HttpClient(l_Handler)
            {
                Timeout                 = p_TimeOut,
#if NET6_0_OR_GREATER
                DefaultRequestVersion   = HttpVersion.Version20,
                DefaultVersionPolicy    = HttpVersionPolicy.RequestVersionOrLower
#endif
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
            m_Client.DefaultRequestHeaders.Add("User-Agent", $"ChatPlexAPISDK_ApiClient/2.0.0");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get header
        /// </summary>
        /// <param name="p_Name">Headername</param>
        /// <returns></returns>
        public string GetHeader(string p_Name)
        {
            lock (m_Client.DefaultRequestHeaders)
            {
                if (m_Client.DefaultRequestHeaders.Contains(p_Name))
                    return string.Join(", ", m_Client.DefaultRequestHeaders.GetValues(p_Name));
            }

            return "";
        }
        /// <summary>
        /// Set header
        /// </summary>
        /// <param name="p_Name">Header name</param>
        /// <param name="p_Value">Header value</param>
        public void SetHeader(string p_Name, string p_Value)
        {
            lock (m_Client.DefaultRequestHeaders)
            {
                if (m_Client.DefaultRequestHeaders.Contains(p_Name))
                    m_Client.DefaultRequestHeaders.Remove(p_Name);

                m_Client.DefaultRequestHeaders.Add(p_Name, p_Value);
            }
        }
        /// <summary>
        /// Remove header
        /// </summary>
        /// <param name="p_Name">Header name</param>
        public void RemoveHeader(string p_Name)
        {
            lock (m_Client.DefaultRequestHeaders)
            {
                if (m_Client.DefaultRequestHeaders.Contains(p_Name))
                    m_Client.DefaultRequestHeaders.Remove(p_Name);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        public WebResponse Get(string p_URL, bool p_DontRetry = false, IProgress<float> p_Progress = null)
        {
            var l_QueryDone = false;
            var l_Reply = null as WebResponse;

            DoRequest("Get", "GET", p_URL, null, CancellationToken.None, (p_Result) => { l_Reply = p_Result; l_QueryDone = true; }, p_DontRetry, p_Progress).ConfigureAwait(false);

            while (!l_QueryDone)
                Thread.Sleep(5);

            return l_Reply;
        }
        /// <summary>
        /// Do GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        public WebResponse Donwload(string p_URL, bool p_DontRetry = false, IProgress<float> p_Progress = null)
        {
            var l_QueryDone = false;
            var l_Reply = null as WebResponse;

            DoRequest("Donwload", "GET", p_URL, null, CancellationToken.None, (p_Result) => { l_Reply = p_Result; l_QueryDone = true; }, p_DontRetry, p_Progress).ConfigureAwait(false);

            while (!l_QueryDone)
                Thread.Sleep(5);

            return l_Reply;
        }
        /// <summary>
        /// Do POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public WebResponse Post(string p_URL, WebContent p_Content, bool p_DontRetry = false)
        {
            var l_QueryDone = false;
            var l_Reply = null as WebResponse;

            DoRequest("Post", "POST", p_URL, p_Content, CancellationToken.None, (p_Result) => { l_Reply = p_Result; l_QueryDone = true; }, p_DontRetry, null).ConfigureAwait(false);

            while (!l_QueryDone)
                Thread.Sleep(5);

            return l_Reply;
        }
        /// <summary>
        /// Do PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public WebResponse Patch(string p_URL, WebContent p_Content, bool p_DontRetry = false)
        {
            var l_QueryDone = false;
            var l_Reply = null as WebResponse;

            DoRequest("Patch", "PATCH", p_URL, p_Content, CancellationToken.None, (p_Result) => { l_Reply = p_Result; l_QueryDone = true; }, p_DontRetry, null).ConfigureAwait(false);

            while (!l_QueryDone)
                Thread.Sleep(5);

            return l_Reply;
        }
        /// <summary>
        /// Do DELETE query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public WebResponse Delete(string p_URL, bool p_DontRetry = false)
        {
            var l_QueryDone = false;
            var l_Reply = null as WebResponse;

            DoRequest("Delete", "DELETE", p_URL, null, CancellationToken.None, (p_Result) => { l_Reply = p_Result; l_QueryDone = true; }, p_DontRetry, null).ConfigureAwait(false);

            while (!l_QueryDone)
                Thread.Sleep(5);

            return l_Reply;
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
            => await DoRequest("GetAsync", "GET", p_URL, null, p_Token, p_Callback, p_DontRetry, p_Progress).ConfigureAwait(false);
        /// <summary>
        /// Do Async GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        public async void DownloadAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null)
            => await DoRequest("DownloadAsync", "GET", p_URL, null, p_Token, p_Callback, p_DontRetry, p_Progress).ConfigureAwait(false);
        /// <summary>
        /// Do Async POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void PostAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("PostAsync", "POST", p_URL, p_Content, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);
        /// <summary>
        /// Do Async PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void PatchAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("PatchAsync", "PATCH", p_URL, p_Content, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);
        /// <summary>
        /// Do Async DELETE query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public async void DeleteAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => await DoRequest("DeleteAsync", "DELETE", p_URL, null, p_Token, p_Callback, p_DontRetry, null).ConfigureAwait(false);

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do request
        /// </summary>
        /// <param name="p_DebugName">Method name for logs</param>
        /// <param name="p_HttpMethod">Http method</param>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        private async Task<bool> DoRequest( string              p_DebugName,
                                            string              p_HttpMethod,
                                            string              p_URL,
                                            WebContent          p_Content,
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
                    p_Progress?.Report(0.0f);

                    l_Reply = null;
                    switch (p_HttpMethod)
                    {
                        case "GET":
                            l_Response = await m_Client.GetAsync(p_URL, p_Token).ConfigureAwait(false);
                            break;

                        case "POST":
                            var l_PostContent = new ByteArrayContent(p_Content.Bytes);
                            l_PostContent.Headers.ContentType = null;
                            l_PostContent.Headers.Remove("Content-Type");
                            l_PostContent.Headers.TryAddWithoutValidation("Content-Type", p_Content.Type);

                            l_Response = await m_Client.PostAsync(p_URL, l_PostContent, p_Token).ConfigureAwait(false);
                            break;

                        case "PATCH":
                            var l_PatchContent = new ByteArrayContent(p_Content.Bytes);
                            l_PatchContent.Headers.ContentType = null;
                            l_PatchContent.Headers.Remove("Content-Type");
                            l_PatchContent.Headers.TryAddWithoutValidation("Content-Type", p_Content.Type);

                            l_Response = await m_Client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), p_URL) { Content = l_PatchContent, }, p_Token).ConfigureAwait(false);
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
                    {
                        if (p_Progress != null)
                        {
                            var l_MemoryStream = new MemoryStream();
                            var l_Stream = await l_Response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                            var l_Buffer        = new byte[8192];
                            var l_ContentLength = l_Response.Content.Headers.ContentLength;
                            var l_TotalRead     = 0L;

                            while (true)
                            {
                                int l_ReadBytes;
                                if ((l_ReadBytes = await l_Stream.ReadAsync(l_Buffer, 0, l_Buffer.Length, p_Token).ConfigureAwait(false)) > 0)
                                {
                                    if (!p_Token.IsCancellationRequested)
                                    {
                                        if (l_ContentLength.HasValue)
                                            p_Progress?.Report((float)l_TotalRead / (float)l_ContentLength.Value);

                                        await l_MemoryStream.WriteAsync(l_Buffer, 0, l_ReadBytes, p_Token).ConfigureAwait(false);
                                        l_TotalRead += (long)l_ReadBytes;
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    p_Progress?.Report(1.0f);
                                    l_Reply.Populate(l_MemoryStream.ToArray());
                                    break;
                                }
                            }
                        }
                        else
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

                    p_Progress?.Report(1.0f);
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
