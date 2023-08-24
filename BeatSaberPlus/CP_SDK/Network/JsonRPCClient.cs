using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace CP_SDK.Network
{
    /// <summary>
    /// JsonRPCClient
    /// </summary>
    public sealed class JsonRPCClient
    {
        private WebClientCore m_WebClient = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_WebClient">Web client instance</param>
        public JsonRPCClient(WebClientCore p_WebClient)
            => m_WebClient = p_WebClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Parameters">Request parameters</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        public JsonRPCResult Request(string p_Method, object[] p_Parameters, bool p_DontRetry = false)
        {
            var l_Content = new JObject()
            {
                ["id"]      = 1,
                ["jsonrpc"] = "2.0",
                ["method"]  = p_Method,
                ["params"]  = new JArray(p_Parameters)
            };

            return DoRequest(p_Method, l_Content, p_DontRetry);
        }
        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Parameters">Request parameters</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        public JsonRPCResult Request(string p_Method, JObject p_Parameters, bool p_DontRetry = false)
        {
            var l_Content = new JObject()
            {
                ["id"] = 1,
                ["jsonrpc"] = "2.0",
                ["method"]  = p_Method,
                ["params"]  = p_Parameters
            };

            return DoRequest(p_Method, l_Content, p_DontRetry);
        }

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
            };

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
            };

            DoRequestAsync(p_Method, l_Content, p_Token, p_Callback, p_DontRetry);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Content">Query content</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        private JsonRPCResult DoRequest(string p_Method, JObject p_Content, bool p_DontRetry = false)
        {
            return HandleWebResponse(p_Method, m_WebClient.Post(
                "",
                WebContent.FromJson(p_Content),
                p_DontRetry
            ));
        }
        /// <summary>
        /// Do a RPC request
        /// </summary>
        /// <param name="p_Method">Target method</param>
        /// <param name="p_Content">Query content</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_DontRetry">Should not retry?</param>
        /// <returns></returns>
        private void DoRequestAsync(string p_Method, JObject p_Content, CancellationToken p_Token, Action<JsonRPCResult> p_Callback, bool p_DontRetry = false)
        {
            m_WebClient.PostAsync(
                "",
                WebContent.FromJson(p_Content),
                p_Token,
                (p_WebResponse) => { p_Callback?.Invoke(HandleWebResponse(p_Method, p_WebResponse)); },
                p_DontRetry
            );
        }
        /// <summary>
        /// Handle a web response
        /// </summary>
        /// <param name="p_Method">Called method</param>
        /// <param name="p_WebResponse">Web response</param>
        /// <returns></returns>
        private static JsonRPCResult HandleWebResponse(string p_Method, WebResponse p_WebResponse)
        {
            if (p_WebResponse == null)
                return new JsonRPCResult() { RawResponse = p_WebResponse, Result = null };

            try
            {
                var l_JsonResult = JObject.Parse(p_WebResponse.BodyString);

                return new JsonRPCResult()
                {
                    RawResponse = p_WebResponse,
                    Result      = (l_JsonResult.GetValue("result") ?? null) as JObject,
                    Error       = (l_JsonResult.GetValue("error") ?? null) as JObject
                };
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_API_SDK.Network][JsonRPCClient.HandleResponse] Request {p_Method} failed parsing response:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return null;
        }
    }
}
