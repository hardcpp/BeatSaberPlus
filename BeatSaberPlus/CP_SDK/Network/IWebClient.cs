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
        /// Get header
        /// </summary>
        /// <param name="p_Name">Headername</param>
        /// <returns></returns>
        string GetHeader(string p_Name);
        /// <summary>
        /// Set header
        /// </summary>
        /// <param name="p_Name">Header name</param>
        /// <param name="p_Value">Header value</param>
        void SetHeader(string p_Name, string p_Value);
        /// <summary>
        /// Remove header
        /// </summary>
        /// <param name="p_Name">Header name</param>
        void RemoveHeader(string p_Name);

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
        void GetAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null);
        /// <summary>
        /// Do Async GET query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        /// <param name="p_Progress">Progress reporter</param>
        void DownloadAsync(string p_URL, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false, IProgress<float> p_Progress = null);
        /// <summary>
        /// Do Async POST query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        void PostAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false);
        /// <summary>
        /// Do Async PATCH query
        /// </summary>
        /// <param name="p_URL">Target URL</param>
        /// <param name="p_Content">Content to post</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry</param>
        void PatchAsync(string p_URL, WebContent p_Content, CancellationToken p_Token, Action<WebResponse> p_Callback, bool p_DontRetry = false);
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
