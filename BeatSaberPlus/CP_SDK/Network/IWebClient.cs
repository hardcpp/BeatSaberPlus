using System;
using System.Net.Http;
using System.Threading;

namespace CP_SDK.Network
{
    /// <summary>
    /// Web Client interface
    /// </summary>
    public interface IWebClient
    {
        /// <summary>
        /// Do Async GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        void GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null);
        void DownloadAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null);
        /// <summary>
        /// Do Async POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Optional content to post</param>
        /// <param name="p_ContentType">Content type</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        void PostAsync(string p_URL, HttpContent p_Content, string p_ContentType, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false);
        /// <summary>
        /// Do Async PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Optional content to post</param>
        /// <param name="p_ContentType">Content type</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        void PatchAsync(string p_URL, HttpContent p_Content, string p_ContentType, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false);
        /// <summary>
        /// Do Async DELETE query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        void DeleteAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false);
    }
}
