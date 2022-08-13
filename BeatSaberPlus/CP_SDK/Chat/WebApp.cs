using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CP_SDK.Chat
{
    /// <summary>
    /// Web app
    /// </summary>
    internal class WebApp
    {
        /// <summary>
        /// Listener
        /// </summary>
        private static HttpListener m_Listener;
        /// <summary>
        /// Cancellation token
        /// </summary>
        private static CancellationTokenSource m_CancellationToken;
        /// <summary>
        /// Page data
        /// </summary>
        private static string m_PageData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the webapp
        /// </summary>
        internal static void Start()
        {
            if (m_PageData == null)
                m_PageData = Misc.Resources.FromRelPathStr(Assembly.GetExecutingAssembly(), "CP_SDK.Chat.Resources.index.html");

            if (m_Listener != null)
                return;

            m_CancellationToken = new CancellationTokenSource();
            m_Listener          = new HttpListener();
            m_Listener.Prefixes.Add($"http://localhost:{ChatModSettings.Instance.WebAppPort}/");

            try
            {
                m_Listener.Start();

                Task.Run(() =>
                {
                    while (!m_CancellationToken.IsCancellationRequested)
                        OnContext(m_Listener.GetContext());
                }).ConfigureAwait(false);
            }
            catch (HttpListenerException l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Chat][WebApp.Start] Can't start the agent to listen transaction" + l_Exception);
                return;
            }
        }
        /// <summary>
        /// Stop the webapp
        /// </summary>
        internal static void Stop()
        {
            if (m_CancellationToken != null)
            {
                m_CancellationToken.Cancel();
                ChatPlexSDK.Logger.Info("[CP_SDK.Chat][WebApp.Stop] Stopped");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On HTTP request
        /// </summary>
        /// <param name="p_Context"></param>
        internal static void OnContext(HttpListenerContext p_Context)
        {
            try
            {
                var l_Request       = p_Context.Request;
                var l_Response      = p_Context.Response;

                if (l_Request.HttpMethod == "POST" && l_Request.Url.AbsolutePath == "/submit")
                {
                    using (var l_Reader = new StreamReader(l_Request.InputStream, l_Request.ContentEncoding))
                    {
                        var l_PostData = l_Reader.ReadToEnd();
                        var l_PostDict = new Dictionary<string, string>();

                        ChatModSettings.Instance.LaunchWebAppOnStartup = false;

                        foreach (var l_CurrentPostData in l_PostData.Split('&'))
                        {
                            try
                            {
                                var l_Split = l_CurrentPostData.Split('=');

                                switch (l_Split[0])
                                {
                                    case "start_webapp":
                                        var l_PostValue = l_Split[1].ToLower().Trim();
                                        ChatModSettings.Instance.LaunchWebAppOnStartup = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
                                        break;

                                    default:
                                        l_PostDict.Add(l_Split[0], l_Split[1]);
                                        break;
                                }
                            }
                            catch (Exception l_Exception)
                            {
                                ChatPlexSDK.Logger.Error("[CP_SDK.Chat][WebApp.OnContext] An exception occurred in OnLoginDataUpdated callback");
                                ChatPlexSDK.Logger.Error(l_Exception);
                            }
                        }

                        ChatModSettings.Instance.Save();

                        foreach (var l_Service in Service.Multiplexer.Services)
                            l_Service.WebPageOnPost(l_PostDict);
                    }

                    l_Response.Redirect("/#saved");
                    l_Response.Close();

                    return;
                }

                var l_PageBuilder = new StringBuilder(m_PageData);

                l_PageBuilder.Replace("{APPLICATION_NAME}", ChatPlexSDK.ProductName);
                l_PageBuilder.Replace("{START_WEBAPP}",     ChatModSettings.Instance.LaunchWebAppOnStartup ? "checked" : "");

                var l_HTMLForm       = "";
                var l_HTML           = "";
                var l_JS             = "";
                var l_JS_VALIDATE    = "";
                foreach (var l_Service in Service.Multiplexer.Services)
                {
                    var l_HTMLFormToAdd     = l_Service.WebPageHTMLForm();
                    var l_HTMLToAdd         = l_Service.WebPageHTML();
                    var l_JSToAdd           = l_Service.WebPageJS();
                    var l_JSValidateToAdd   = l_Service.WebPageJSValidate();

                    if (!string.IsNullOrEmpty(l_HTMLFormToAdd))     l_HTMLForm      += l_HTMLFormToAdd      + "<br/>";
                    if (!string.IsNullOrEmpty(l_HTMLToAdd))         l_HTML          += l_HTMLToAdd          + "<br/>";
                    if (!string.IsNullOrEmpty(l_JSToAdd))           l_JS            += l_JSToAdd            + "\n";
                    if (!string.IsNullOrEmpty(l_JSValidateToAdd))   l_JS_VALIDATE   += l_JSValidateToAdd    + "\n";
                }

                l_PageBuilder.Replace("{_HTML_FORM_}",      l_HTMLForm);
                l_PageBuilder.Replace("{_HTML_}",           l_HTML);
                l_PageBuilder.Replace("{_JS_}",             l_JS);
                l_PageBuilder.Replace("{_JS_VALIDATE_}",    l_JS_VALIDATE);

                byte[] l_Data = Encoding.UTF8.GetBytes(l_PageBuilder.ToString());
                l_Response.ContentType      = "text/html";
                l_Response.ContentEncoding  = Encoding.UTF8;
                l_Response.ContentLength64  = l_Data.LongLength;

                l_Response.OutputStream.Write(l_Data, 0, l_Data.Length);
                l_Response.OutputStream.Close();
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Chat][WebApp.OnContext] Exception occurred during webapp request.");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
