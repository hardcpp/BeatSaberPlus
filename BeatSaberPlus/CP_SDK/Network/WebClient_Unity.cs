#if CP_SDK_UNITY
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace CP_SDK.Network
{
    /// <summary>
    /// WebClient using unity web requests
    /// </summary>
    public class WebClient
    {
        /// <summary>
        /// Global client instance
        /// </summary>
        public static readonly WebClient GlobalClient = new WebClient();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Timeout seconds
        /// </summary>
        public int Timeout = 10;
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
        public float RetryInterval = 5f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get async
        /// </summary>
        /// <param name="p_URL">Request URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">On result callbacks</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        public void GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false)
        {
#if DEBUG
            ChatPlexSDK.Logger.Debug("[CP_SDK.Network][WebClient.GetAsync] GET " + p_URL);
#endif

            Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_GetAsync(p_URL, p_Token, p_Callback, p_DontRetry));
        }
        /// <summary>
        /// Download async
        /// </summary>
        /// <param name="p_URL">Request URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">On result callbacks</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        public void DownloadAsync(string p_URL, CancellationToken p_Token, Action<byte[]> p_Callback, IProgress<float> p_Progress = null, bool p_DontRetry = false)
        {
#if DEBUG
            ChatPlexSDK.Logger.Debug("[CP_SDK.Network][WebClient.DownloadAsync] GET " + p_URL);
#endif

            Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_DownloadAsync(p_URL, p_Token, p_Callback, p_Progress, p_DontRetry));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get async
        /// </summary>
        /// <param name="p_URL">Request URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">On result callbacks</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        private IEnumerator Coroutine_GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry)
        {
            var l_Reply = null as WebResponse;
            for (int l_RetryI = 1; l_RetryI <= MaxRetry; l_RetryI++)
            {
                if (p_Token.IsCancellationRequested)
                    break;

                var l_Request = new UnityWebRequest(p_URL)
                {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout         = Timeout
                };

                yield return l_Request.SendWebRequest();
                l_Reply = new WebResponse(l_Request);

                if (p_Token.IsCancellationRequested)
                    break;

                if (!l_Reply.IsSuccessStatusCode)
                {
                    if (!l_Reply.ShouldRetry || p_DontRetry)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.DownloadAsync] Request {SafeURL(p_URL)} failed with code {l_Request.responseCode}:\"{l_Request.error}\", not retrying");
                        break;
                    }

                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.GetAsync] Request {SafeURL(p_URL)} failed with code {l_Reply.StatusCode}:\"{l_Reply.ReasonPhrase}\", next try in {RetryInterval} seconds...");

                    yield return new WaitForSecondsRealtime(RetryInterval);

                    continue;
                }
                else
                    break;
            }

            Unity.MTThreadInvoker.EnqueueOnThread(() => p_Callback(l_Reply));
        }
        /// <summary>
        /// Download async
        /// </summary>
        /// <param name="p_URL">Request URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">On result callbacks</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        private IEnumerator Coroutine_DownloadAsync(string p_URL, CancellationToken p_Token, Action<byte[]> p_Callback, IProgress<float> p_Progress, bool p_DontRetry)
        {
            var l_Waiter = new WaitForSecondsRealtime(0.05f);

            for (int l_RetryI = 1; l_RetryI <= MaxRetry; l_RetryI++)
            {
                if (p_Token.IsCancellationRequested)
                    break;

                var l_Request = new UnityWebRequest(p_URL)
                {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout         = DownloadTimeout
                };

                p_Progress?.Report(0f);

                l_Request.SendWebRequest();

                do
                {
                    yield return l_Waiter;
                    p_Progress?.Report(l_Request.downloadProgress);

                    if (p_Token.IsCancellationRequested || l_Request.isDone || l_Request.isHttpError || l_Request.isNetworkError)
                        break;
                } while (true);

                if (p_Token.IsCancellationRequested)
                    break;

                if (l_Request.isHttpError || l_Request.isNetworkError)
                {
                    if (!(l_Request.responseCode < 400 || l_Request.responseCode >= 500) || p_DontRetry)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.DownloadAsync] Request {SafeURL(p_URL)} failed with code {l_Request.responseCode}:\"{l_Request.error}\", not retrying");
                        break;
                    }

                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebClient.DownloadAsync] Request {SafeURL(p_URL)} failed with code {l_Request.responseCode}:\"{l_Request.error}\", next try in {RetryInterval} seconds...");

                    yield return new WaitForSecondsRealtime(RetryInterval);

                    continue;
                }
                else
                {
                    p_Progress?.Report(1f);
                    Unity.MTThreadInvoker.EnqueueOnThread(() => p_Callback(l_Request.downloadHandler.data));
                    yield break;
                }
            }

            Unity.MTThreadInvoker.EnqueueOnThread(() => p_Callback(null));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
    }
}
#endif