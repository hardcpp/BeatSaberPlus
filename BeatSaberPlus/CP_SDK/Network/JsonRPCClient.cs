using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;

namespace CP_SDK.Network
{
    /// <summary>
    /// JsonRPCClient
    /// </summary>
    public sealed class JsonRPCClient
    {
        /// <summary>
        /// WebClient
        /// </summary>
        private IWebClient m_WebClient = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_WebClient">Web client instance</param>
        public JsonRPCClient(IWebClient p_WebClient)
            => m_WebClient = p_WebClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Parameters">Request parameters</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        public void RequestAsync(string p_Method, object[] p_Parameters, CancellationToken p_Token, Action<JsonRPCResult> p_Callback, bool p_DontRetry = false)
        {
            var l_Content = new JObject()
            {
                ["id"]      = 1,
                ["jsonrpc"] = "2.0",
                ["method"]  = p_Method,
                ["params"]  = new JArray(p_Parameters)

            }.ToString();

            DoRequestAsync(p_Method, l_Content, p_Token, p_Callback, p_DontRetry);
        }
        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Parameters">Request parameters</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        public void RequestAsync(string p_Method, JObject p_Parameters, CancellationToken p_Token, Action<JsonRPCResult> p_Callback, bool p_DontRetry = false)
        {
            var l_Content = new JObject()
            {
                ["id"]      = 1,
                ["jsonrpc"] = "2.0",
                ["method"]  = p_Method,
                ["params"]  = p_Parameters

            }.ToString();

            DoRequestAsync(p_Method, l_Content, p_Token, p_Callback, p_DontRetry);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Content">Query content</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        private void DoRequestAsync(string p_Method, string p_Content, CancellationToken p_Token, Action<JsonRPCResult> p_Callback, bool p_DontRetry = false)
        {
            m_WebClient.PostAsync("", new StringContent(p_Content), "application/json", p_Token, (p_WebResponse) =>
            {
                if (p_WebResponse == null)
                {
                    p_Callback?.Invoke(new JsonRPCResult() { RawResponse = p_WebResponse, Result = null });
                    return;
                }

                try
                {
                    var l_JsonResult = JObject.Parse(p_WebResponse.BodyString);

                    p_Callback?.Invoke(new JsonRPCResult()
                    {
                        RawResponse = p_WebResponse,
                        Result      = (l_JsonResult.GetValue("result") ?? null) as JObject,
                        Error       = (l_JsonResult.GetValue("error")  ?? null) as JObject
                    });
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Network][JsonRPCClient.DoRequestAsync] Request {p_Method} failed parsing response:");
                    ChatPlexSDK.Logger.Error(l_Exception.ToString());

                    p_Callback?.Invoke(null);
                }

            }, p_DontRetry);
        }
    }
}
