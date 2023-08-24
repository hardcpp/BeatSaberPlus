#if CP_SDK_UNITY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace CP_SDK.Network
{
    /// <summary>
    /// WebClient using unity web requests
    /// </summary>
    public sealed class WebClientUnity : IWebClient
    {
        /// <summary>
        /// Global client instance
        /// </summary>
        public static readonly WebClientUnity GlobalClient = new WebClientUnity("", TimeSpan.FromSeconds(10));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private string                      m_BaseAddress   = string.Empty;
        private int                         m_Timeout       = 10;
        private Dictionary<string, string>  m_Headers       = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Timeout seconds
        /// </summary>
        public int DownloadTimeout = 2 * 60;
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
        /// <param name="p_TimeOut">Requests timeout</param>
        /// <param name="p_ForceCacheDiscard">Should force server cache discard</param>
        public WebClientUnity(string p_BaseAddress, TimeSpan p_TimeOut, bool p_ForceCacheDiscard = false)
        {
            m_BaseAddress = p_BaseAddress;

            m_Timeout = (int)p_TimeOut.TotalSeconds;

            if (p_ForceCacheDiscard)
                m_Headers.Add("Cache-Control", "no-cache, must-revalidate, proxy-revalidate, max-age=0, s-maxage=0, max-stale=0");
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
            lock (m_Headers)
            {
                if (m_Headers.TryGetValue(p_Name, out var l_Value))
                    return l_Value;
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
            lock (m_Headers)
            {
                if (m_Headers.ContainsKey(p_Name))
                    m_Headers.Remove(p_Name);

                m_Headers.Add(p_Name, p_Value);
            }
        }
        /// <summary>
        /// Remove header
        /// </summary>
        /// <param name="p_Name">Header name</param>
        public void RemoveHeader(string p_Name)
        {
            lock (m_Headers)
            {
                if (m_Headers.ContainsKey(p_Name))
                    m_Headers.Remove(p_Name);
            }
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
        public void GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null)
            => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DoRequest("GetAsync", "GET", GetURL(p_URL), null, p_Token, p_Callback, p_DontRetry, p_Progress));
        /// <summary>
        /// Do Async GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        public void DownloadAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null)
            => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DoRequest("DownloadAsync", "DOWNLOAD", GetURL(p_URL), null, p_Token, p_Callback, p_DontRetry, p_Progress));
        /// <summary>
        /// Do Async POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public void PostAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DoRequest("PostAsync", "POST", GetURL(p_URL), p_Content, p_Token, p_Callback, p_DontRetry, null));
        /// <summary>
        /// Do Async PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public void PatchAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DoRequest("PatchAsync", "PATCH", GetURL(p_URL), p_Content, p_Token, p_Callback, p_DontRetry, null));
        /// <summary>
        /// Do Async DELETE query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        public void DeleteAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
            => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DoRequest("DeleteAsync", "DELETE", GetURL(p_URL), null, p_Token, p_Callback, p_DontRetry, null));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get URL
        /// </summary>
        /// <param name="p_URL">Request URL</param>
        /// <returns></returns>
        private string GetURL(string p_URL)
        {
            if (string.IsNullOrEmpty(m_BaseAddress))    return p_URL;
            if (p_URL.Contains("://"))                  return p_URL;
            if (m_BaseAddress.EndsWith("/"))            return m_BaseAddress + p_URL;

            return m_BaseAddress + "/" + p_URL;
        }
        /// <summary>
        /// Safe URL parsing
        /// </summary>
        /// <param name="p_URL">Source URL</param>
        /// <returns></returns>
        private string SafeURL(string p_URL)
        {
            var l_Result = p_URL;
            if (l_Result.Contains("?"))
                l_Result = l_Result.Substring(0, l_Result.IndexOf("?"));

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prepare request
        /// </summary>
        /// <param name="p_Request">Request to prepare</param>
        /// <param name="p_IsDownload">Is a download request?</param>
        private void PrepareRequest(UnityWebRequest p_Request, bool p_IsDownload)
        {
            if (p_Request.downloadHandler == null)
                p_Request.downloadHandler = new DownloadHandlerBuffer();

            p_Request.timeout = p_IsDownload ? DownloadTimeout : m_Timeout;

            foreach (var l_KVP in m_Headers)
                p_Request.SetRequestHeader(l_KVP.Key, l_KVP.Value);
        }
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
        private IEnumerator Coroutine_DoRequest(string              p_DebugName,
                                                string              p_HttpMethod,
                                                string              p_URL,
                                                WebContent          p_Content,
                                                CancellationToken   p_Token,
                                                Action<WebResponse> p_Callback,
                                                bool                p_DontRetry,
                                                IProgress<float>    p_Progress)
        {
#if DEBUG
            ChatPlexSDK.Logger.Debug($"[CP_SDK.Network][WebClient.{p_DebugName}] {p_HttpMethod} " + p_URL);
#endif

            var l_Reply = null as WebResponse;
            for (int l_RetryI = 1; l_RetryI <= MaxRetry; l_RetryI++)
            {
                if (p_Token.IsCancellationRequested)
                    break;

                try { p_Progress?.Report(0f); } catch { }

                var l_Request = null as UnityWebRequest;
                switch (p_HttpMethod)
                {
                    case "GET":
                    case "DOWNLOAD":
                        l_Request = UnityWebRequest.Get(p_URL);
                        break;

                    case "POST":
                    case "PATCH":
                        l_Request = new UnityWebRequest(p_URL, p_HttpMethod)
                        {
                            uploadHandler = new UploadHandlerRaw(p_Content.Bytes)
                            {
                                contentType = p_Content.Type
                            },
                            downloadHandler = new DownloadHandlerBuffer()
                        };
                        break;

                    case "DELETE":
                        l_Request = UnityWebRequest.Delete(p_URL);
                        break;
                }

                PrepareRequest(l_Request, p_HttpMethod == "DOWNLOAD");

                if (p_Progress == null)
                    yield return l_Request.SendWebRequest();
                else
                {
                    l_Request.SendWebRequest();

                    var l_Waiter = new WaitForSecondsRealtime(0.05f);
                    do
                    {
                        yield return l_Waiter;
                        try { p_Progress?.Report(l_Request.downloadProgress); } catch { }

                        if (p_Token.IsCancellationRequested || l_Request.isDone || l_Request.isHttpError || l_Request.isNetworkError)
                            break;
                    } while (true);
                }

                if (p_Token.IsCancellationRequested)
                    break;

                l_Reply = new WebResponse(l_Request);

                if (!l_Reply.IsSuccessStatusCode && l_Reply.StatusCode == (HttpStatusCode)429)
                {
                    var l_Limits = RateLimitInfo.Get(l_Request);
                    if (l_Limits != null)
                    {
                        int l_TotalMilliseconds = (int)(l_Limits.Reset - DateTime.Now).TotalMilliseconds;
                        if (l_TotalMilliseconds > 0)
                        {
                            ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.{p_DebugName}] Request {SafeURL(p_URL)} was rate limited, retrying in {l_TotalMilliseconds}ms...");

                            yield return new WaitForSecondsRealtime(RetryInterval);
                            continue;
                        }
                    }
                }

                if (!l_Reply.IsSuccessStatusCode)
                {
                    if (!l_Reply.ShouldRetry || p_DontRetry)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.{p_DebugName}] Request {SafeURL(p_URL)} failed with code {l_Request.responseCode}:\"{l_Request.error}\", not retrying");
                        break;
                    }

                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.{p_DebugName}] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in {RetryInterval} seconds...");

                    yield return new WaitForSecondsRealtime(RetryInterval);
                    continue;
                }

                try { p_Progress?.Report(1f); } catch { }

                break;
            }

            if (!p_Token.IsCancellationRequested)
                Unity.MTThreadInvoker.EnqueueOnThread(() => p_Callback?.Invoke(l_Reply));
        }
    }
}
#endif