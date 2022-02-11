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

                                    case "twitch_channel":
                                        string l_CurrentTwitchChannelName = l_Split[1].ToLower().Trim();

                                        if (!string.IsNullOrWhiteSpace(l_CurrentTwitchChannelName) && !l_ChannelList.Contains(l_CurrentTwitchChannelName, StringComparer.InvariantCultureIgnoreCase))
                                            l_ChannelList.Add(l_CurrentTwitchChannelName);

                                        Logger.Instance.Info($"[SDK.Chat][WebApp.OnContext] TwitchChannel: {l_CurrentTwitchChannelName}");
                                        l_NewTwitchChannels.Add(l_CurrentTwitchChannelName);
                                        break;
                                }
                            }
                            catch (Exception l_Exception)
                            {
                                Logger.Instance.Error("[SDK.Chat][WebApp.OnContext] An exception occurred in OnLoginDataUpdated callback");
                                Logger.Instance.Error(l_Exception);
                            }
                        }

                        foreach (var l_CurrentOldChannel in new List<string>(l_ChannelList))
                        {
                            /// Remove any channels that weren't present in the post data
                            if (!l_NewTwitchChannels.Contains(l_CurrentOldChannel))
                                l_ChannelList.Remove(l_CurrentOldChannel);
                        }

                        try
                        {
                            AuthConfig.Twitch.Channels = string.Join(",", l_ChannelList);

                            ParseWebAppSettings(l_PostValues);
                            ParseGlobalSettings(l_PostValues);
                            ParseTwitchSettings(l_PostValues);

                            var l_TwitchService = Service.Multiplexer.Services.FirstOrDefault(x => x is Services.Twitch.TwitchService);
                            if (l_TwitchService != null)
                                (l_TwitchService as Services.Twitch.TwitchService).OnCredentialsUpdated();
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

                l_PageBuilder.Replace("{WebAppSettingsHTML}",   GenerateWebAppSettings());
                l_PageBuilder.Replace("{GlobalSettingsHTML}",   GenerateGlobalSettings());
                l_PageBuilder.Replace("{TwitchSettingsHTML}",   GenerateTwitchSettings());
                l_PageBuilder.Replace("{TwitchChannelHtml}",    l_TwitchChannelHtmlString.ToString());
                l_PageBuilder.Replace("{TwitchHasChannels}",    l_ChannelList.Count > 0 ? "true" : "false");
                l_PageBuilder.Replace("{TwitchOAuthToken}",     AuthConfig.Twitch.OAuthToken);

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string GenerateWebAppSettings()
        {
            string l_Result = "<label class=\"form-label\">Web App</label>";
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.WebApp.LaunchWebAppOnStartup), SettingsConfig.WebApp.LaunchWebAppOnStartup);

            return l_Result;
        }
        private static void ParseWebAppSettings(Dictionary<string, string> p_Data)
        {
            if (p_Data.ContainsKey(nameof(SettingsConfig.WebApp.LaunchWebAppOnStartup)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.WebApp.LaunchWebAppOnStartup)].ToLower();
                SettingsConfig.WebApp.LaunchWebAppOnStartup = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string GenerateGlobalSettings()
        {
            string l_Result = "<label class=\"form-label\">Global</label>";
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Global.ParseEmojis),          SettingsConfig.Global.ParseEmojis);

            return l_Result;
        }
        private static void ParseGlobalSettings(Dictionary<string, string> p_Data)
        {
            if (p_Data.ContainsKey(nameof(SettingsConfig.Global.ParseEmojis)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Global.ParseEmojis)].ToLower();
                SettingsConfig.Global.ParseEmojis = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string GenerateTwitchSettings()
        {
            string l_Result = "<label class=\"form-label\">Twitch</label>";
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Twitch.ParseBTTVEmotes),      SettingsConfig.Twitch.ParseBTTVEmotes);
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Twitch.ParseFFZEmotes),       SettingsConfig.Twitch.ParseFFZEmotes);
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Twitch.Parse7TVEmotes),       SettingsConfig.Twitch.Parse7TVEmotes);
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Twitch.ParseTwitchEmotes),    SettingsConfig.Twitch.ParseTwitchEmotes);
            l_Result += BuildSwitchHTML(nameof(SettingsConfig.Twitch.ParseCheermotes),      SettingsConfig.Twitch.ParseCheermotes);

            return l_Result;
        }
        private static void ParseTwitchSettings(Dictionary<string, string> p_Data)
        {
            if (p_Data.ContainsKey(nameof(SettingsConfig.Twitch.ParseBTTVEmotes)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Twitch.ParseBTTVEmotes)].ToLower();
                SettingsConfig.Twitch.ParseBTTVEmotes = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
            if (p_Data.ContainsKey(nameof(SettingsConfig.Twitch.ParseFFZEmotes)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Twitch.ParseFFZEmotes)].ToLower();
                SettingsConfig.Twitch.ParseFFZEmotes = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
            if (p_Data.ContainsKey(nameof(SettingsConfig.Twitch.Parse7TVEmotes)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Twitch.Parse7TVEmotes)].ToLower();
                SettingsConfig.Twitch.Parse7TVEmotes = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
            if (p_Data.ContainsKey(nameof(SettingsConfig.Twitch.ParseTwitchEmotes)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Twitch.ParseTwitchEmotes)].ToLower();
                SettingsConfig.Twitch.ParseTwitchEmotes = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
            if (p_Data.ContainsKey(nameof(SettingsConfig.Twitch.ParseCheermotes)))
            {
                var l_PostValue = p_Data[nameof(SettingsConfig.Twitch.ParseCheermotes)].ToLower();
                SettingsConfig.Twitch.ParseCheermotes = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string BuildSwitchHTML(string p_Name, bool p_Value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<label class=\"form-switch\">\r\n");
            sb.Append($"\t<input type=\"hidden\" value=\"off\" name=\"{p_Name}\">\r\n");
            sb.Append($"\t<input name=\"{p_Name}\" type=\"checkbox\" {(p_Value ? "checked" : "")}>\r\n");
            sb.Append($"\t<i class=\"form-icon\"></i> {Uncamelcase(p_Name)}\r\n");
            sb.Append($"</label>");
            return sb.ToString();
        }
        private static string BuildNumberHTML(string p_Name, int p_Value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<label class=\"form-label\">\r\n");
            sb.Append($"\t<i class=\"form-icon\"></i> {Uncamelcase(p_Name)}\r\n");
            sb.Append($"\t<input name=\"{p_Name}\" class=\"form-input\" type=\"number\" placeholder=\"00\" value=\"{p_Value.ToString()}\">\r\n");
            sb.Append($"</label>");
            return sb.ToString();
        }
        private static string BuildStringHTML(string p_Name, string p_Value)
        {
            var l_Builder = new StringBuilder();
            l_Builder.Append($"<label class=\"form-label\">\r\n");
            l_Builder.Append($"\t<i class=\"form-icon\"></i> {Uncamelcase(p_Name)}\r\n");
            l_Builder.Append($"\t<input name=\"{p_Name}\" class=\"form-input\" type=\"text\" placeholder=\"00\" value=\"{p_Value}\">\r\n");
            l_Builder.Append($"</label>");

            return l_Builder.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static string Uncamelcase(string p_Value)
        {
            var l_Builder       = new StringBuilder();
            var l_UpperStreak   = 0;

            for (int l_I = 0; l_I < p_Value.Length; l_I++)
            {
                if (l_I < p_Value.Length - 2)
                {
                    bool l_IsLower = char.IsLower(p_Value[l_I]);
                    if (!l_IsLower)
                        l_UpperStreak++;

                    bool l_NextIsLower = char.IsLower(p_Value[l_I + 1]);
                    if (l_IsLower && !l_NextIsLower)
                    {
                        l_Builder.Append(p_Value[l_I]);
                        l_Builder.Append(" ");
                    }
                    else if (!l_IsLower && l_NextIsLower && l_UpperStreak > 1)
                    {
                        l_Builder.Append(" ");
                        l_Builder.Append(p_Value[l_I]);
                    }
                    else
                        l_Builder.Append(p_Value[l_I]);

                    if (l_IsLower)
                        l_UpperStreak = 0;
                }
                else
                    l_Builder.Append(p_Value[l_I]);
            }

            return l_Builder.ToString();
        }
    }
}
