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

namespace BeatSaberPlus.SDK.Chat
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
            {
                using (var l_Reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("BeatSaberPlus.SDK.Chat.Resources.index.html")))
                    m_PageData = l_Reader.ReadToEnd();
            }

            if (m_Listener != null)
                return;

            m_CancellationToken = new CancellationTokenSource();
            m_Listener          = new HttpListener();
            m_Listener.Prefixes.Add($"http://localhost:{SettingsConfig.WebApp.WebAppPort}/");

            try
            {
                m_Listener.Start();
            }
            catch (HttpListenerException l_Exception)
            {
                Logger.Instance.Error("[SDK.Chat][WebApp.Start] Can't start the agent to listen transaction" + l_Exception);
                return;
            }

            Task.Run(() =>
            {
                while (!m_CancellationToken.IsCancellationRequested)
                    OnContext(m_Listener.GetContext());
            });
        }
        /// <summary>
        /// Stop the webapp
        /// </summary>
        internal static void Stop()
        {
            if (m_CancellationToken != null)
            {
                m_CancellationToken.Cancel();
                Logger.Instance.Info("[SDK.Chat][WebApp.Stop] Stopped");
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
                var l_ChannelList   = AuthConfig.Twitch.Channels.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (l_Request.HttpMethod == "POST" && l_Request.Url.AbsolutePath == "/submit")
                {
                    using (var l_Reader = new StreamReader(l_Request.InputStream, l_Request.ContentEncoding))
                    {
                        var l_PostData          = l_Reader.ReadToEnd();
                        var l_NewTwitchChannels = new List<string>();
                        var l_PostValues        = new Dictionary<string, string>();

                        SettingsConfig.WebApp.LaunchWebAppOnStartup = false;

                        foreach (var l_CurrentPostData in l_PostData.Split('&'))
                        {
                            try
                            {
                                var l_Split = l_CurrentPostData.Split('=');
                                l_PostValues[l_Split[0]] = l_Split[1];

                                switch (l_Split[0])
                                {
                                    case "twitch_oauthtoken":
                                        var l_NewTwitchOauthToken = HttpUtility.UrlDecode(l_Split[1]);
                                        AuthConfig.Twitch.OAuthToken =  l_NewTwitchOauthToken.StartsWith("oauth:")
                                                                        ?
                                                                            l_NewTwitchOauthToken
                                                                        :
                                                                            !string.IsNullOrEmpty(l_NewTwitchOauthToken)
                                                                            ?
                                                                                $"oauth:{l_NewTwitchOauthToken}"
                                                                            :
                                                                                ""
                                                                        ;
                                        break;

                                    case "twitch_channel1":
                                    case "twitch_channel2":
                                    case "twitch_channel3":
                                    case "twitch_channel4":
                                    case "twitch_channel5":
                                        var l_Value = l_Split[1].ToLower().Trim();
                                        if (!string.IsNullOrEmpty(l_Value))
                                            l_NewTwitchChannels.Add(l_Split[1].ToLower().Trim());
                                        break;

                                    case "start_webapp":
                                        var l_PostValue = l_Split[1].ToLower().Trim();
                                        SettingsConfig.WebApp.LaunchWebAppOnStartup = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
                                        break;
                                }
                            }
                            catch (Exception l_Exception)
                            {
                                Logger.Instance.Error("[SDK.Chat][WebApp.OnContext] An exception occurred in OnLoginDataUpdated callback");
                                Logger.Instance.Error(l_Exception);
                            }
                        }

                        try
                        {
                            AuthConfig.Twitch.Channels = string.Join(",", l_NewTwitchChannels.Distinct());

                            var l_TwitchService = Service.Multiplexer.Services.FirstOrDefault(x => x is Services.Twitch.TwitchService);
                            if (l_TwitchService != null)
                                (l_TwitchService as Services.Twitch.TwitchService).OnCredentialsUpdated(false);
                            else
                                Logger.Instance.Error("[SDK.Chat][WebApp.OnContext] Twitch service not found!");
                        }
                        catch (Exception l_Exception)
                        {
                            Logger.Instance.Error("[SDK.Chat][WebApp.OnContext] An exception occurred while updating config");
                            Logger.Instance.Error(l_Exception);
                        }
                    }

                    l_Response.Redirect(l_Request.UrlReferrer.OriginalString);
                    l_Response.Close();

                    return;
                }

                var l_PageBuilder               = new StringBuilder(m_PageData);
                var l_TwitchChannelHtmlString   = new StringBuilder();

                for (int l_I = 0; l_I < l_ChannelList.Count; l_I++)
                {
                    var l_Channel = l_ChannelList[l_I];
                    l_TwitchChannelHtmlString.Append($"<span id=\"twitch_channel_{l_I}\" class=\"chip \"><div style=\"overflow: hidden;text-overflow: ellipsis;\">{l_Channel}</div><input type=\"text\" class=\"form-input\" name=\"twitch_channel\" style=\"display: none; \" value=\"{l_Channel}\" /><button type=\"button\" onclick=\"removeTwitchChannel('twitch_channel_{l_I}')\" class=\"btn btn-clear\" aria-label=\"Close\" role=\"button\"></button></span>");
                }

                l_PageBuilder.Replace("{APPLICATION_NAME}", "BeatSaberPlus");
                l_PageBuilder.Replace("{TWITCH_OAUTH}",     AuthConfig.Twitch.OAuthToken);
                l_PageBuilder.Replace("{TWITCH_CHANNEL1}",  l_ChannelList.Count >= 1 ? l_ChannelList[0] : "");
                l_PageBuilder.Replace("{TWITCH_CHANNEL2}",  l_ChannelList.Count >= 2 ? l_ChannelList[1] : "");
                l_PageBuilder.Replace("{TWITCH_CHANNEL3}",  l_ChannelList.Count >= 3 ? l_ChannelList[2] : "");
                l_PageBuilder.Replace("{TWITCH_CHANNEL4}",  l_ChannelList.Count >= 4 ? l_ChannelList[3] : "");
                l_PageBuilder.Replace("{TWITCH_CHANNEL5}",  l_ChannelList.Count >= 5 ? l_ChannelList[4] : "");
                l_PageBuilder.Replace("{START_WEBAPP}",     SettingsConfig.WebApp.LaunchWebAppOnStartup ? "checked" : "");


                byte[] l_Data = Encoding.UTF8.GetBytes(l_PageBuilder.ToString());
                l_Response.ContentType      = "text/html";
                l_Response.ContentEncoding  = Encoding.UTF8;
                l_Response.ContentLength64  = l_Data.LongLength;

                l_Response.OutputStream.Write(l_Data, 0, l_Data.Length);
                l_Response.OutputStream.Close();
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Chat][WebApp.OnContext] Exception occurred during webapp request.");
                Logger.Instance.Error(l_Exception);
            }
        }
    }
}
