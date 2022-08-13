using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;

namespace CP_SDK.OBS
{
    /// <summary>
    /// OBS service holder
    /// </summary>
    public partial class Service
    {
        /// <summary>
        /// Client instance
        /// </summary>
        private static Network.WebSocketClient m_Client = null;
        /// <summary>
        /// Reference count
        /// </summary>
        private static int m_ReferenceCount = 0;
        /// <summary>
        /// Lock object
        /// </summary>
        private static object m_Object = new object();
        /// <summary>
        /// Scenes collection
        /// </summary>
        private static ConcurrentDictionary<string, Models.Scene> m_Scenes = new ConcurrentDictionary<string, Models.Scene>();
        /// <summary>
        /// Transitions
        /// </summary>
        private static string[] m_Transitions = new string[] { };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Status of the service
        /// </summary>
        public enum EStatus
        {
            Disconnected,
            Connecting,
            Connected,
            Authing,
            AuthRejected
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On scene list refreshed
        /// </summary>
        public static event Action OnSceneListRefreshed;

        /// <summary>
        /// On active scene changed(Old, New)
        /// </summary>
        public static event Action<Models.Scene, Models.Scene> OnActiveSceneChanged;
        /// <summary>
        /// On source visibility changed (Scene, Source, IsVisible)
        /// </summary>
        public static event Action<Models.Scene, Models.Source, bool> OnSourceVisibilityChanged;
        /// <summary>
        /// On studio mode change(Active, Scene)
        /// </summary>
        public static event Action<bool, Models.Scene> OnStudioModeChanged;
        /// <summary>
        /// On streaming status changed (IsStreaming)
        /// </summary>
        public static event Action<bool> OnStreamingStatusChanged;
        /// <summary>
        /// On recording status changed (IsRecording)
        /// </summary>
        public static event Action<bool> OnRecordingStatusChanged;

        /// <summary>
        /// Status of the service
        /// </summary>
        public static EStatus Status { get; private set; } = EStatus.Disconnected;
        /// <summary>
        /// Is in studio mode?
        /// </summary>
        public static bool IsInStudioMode { get; private set; } = false;
        /// <summary>
        /// Is in streaming mode?
        /// </summary>
        public static bool IsStreaming { get; private set; } = false;
        /// <summary>
        /// Is recording
        /// </summary>
        public static bool IsRecording { get; private set; } = false;
        /// <summary>
        /// Last recorded file name
        /// </summary>
        public static string LastRecordedFileName { get; private set; } = string.Empty;
        /// <summary>
        /// Active scene
        /// </summary>
        public static Models.Scene ActiveScene { get; private set; } = null;
        /// <summary>
        /// Active preview scene
        /// </summary>
        public static Models.Scene ActivePreviewScene { get; private set; } = null;

        /// <summary>
        /// Scenes collection
        /// </summary>
        public static ConcurrentDictionary<string, Models.Scene> Scenes => m_Scenes;
        /// <summary>
        /// Transitions
        /// </summary>
        public static string[] Transitions => m_Transitions;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            OBSModSettings.Instance.Warmup();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Acquire
        /// </summary>
        public static void Acquire()
        {
            lock (m_Object)
            {
                if (m_ReferenceCount == 0)
                    Create();

                m_ReferenceCount++;
            }
        }
        /// <summary>
        /// Release
        /// </summary>
        /// <param name="p_OnExit">Should release all instances</param>
        public static void Release(bool p_OnExit = false)
        {
            lock (m_Object)
            {
                if (p_OnExit)
                {
                    Destroy();
                    m_ReferenceCount = 0;
                }
                else
                    m_ReferenceCount--;

                if (m_ReferenceCount < 0) m_ReferenceCount = 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Apply config
        /// </summary>
        public static void ApplyConf()
        {
            OBSModSettings.Instance.Save();

            if (m_ReferenceCount > 0)
            {
                if (OBSModSettings.Instance.Enabled)
                {
                    if (m_Client.IsConnected)
                        m_Client.Disconnect();

                    m_Client.Connect("ws://" + OBSModSettings.Instance.Server);
                }
                else if (!OBSModSettings.Instance.Enabled && Status != EStatus.Disconnected && Status != EStatus.AuthRejected)
                    m_Client.Disconnect();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create
        /// </summary>
        private static void Create()
        {
            if (m_Client != null)
                return;

            m_Client = new Network.WebSocketClient();
            m_Client.ReconnectDelay      = 15 * 1000;
            m_Client.OnOpen             += WebSocket_OnOpen;
            m_Client.OnClose            += WebSocket_OnClose;
            m_Client.OnError            += WebSocket_OnError;
            m_Client.OnMessageReceived  += WebSocket_OnMessageReceived;

            if (OBSModSettings.Instance.Enabled)
            {
                Status = EStatus.Connecting;
                m_Client.Connect("ws://" + OBSModSettings.Instance.Server);
            }
            else
                Status = EStatus.Disconnected;
        }
        /// <summary>
        /// Destroy
        /// </summary>
        private static void Destroy()
        {
            if (m_Client == null)
                return;

            try
            {
                m_Client.Disconnect();
                m_Client.Dispose();
            }
            catch
            {

            }

            m_Client = null;

            foreach (var l_KVP in m_Scenes)
                Models.Scene.Release(l_KVP.Value);

            IsInStudioMode      = false;
            IsStreaming         = false;
            IsRecording         = false;
            ActiveScene         = null;
            ActivePreviewScene  = null;

            m_Scenes.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On web socket open
        /// </summary>
        private static void WebSocket_OnOpen()
        {
            ChatPlexSDK.Logger.Info("[CP_SDK.OBS][Service.WebSocket_OnOpen]");

            Status = EStatus.Authing;
            SendRequest("GetAuthRequired");
        }
        /// <summary>
        /// On web socket close
        /// </summary>
        private static void WebSocket_OnClose()
        {
            ChatPlexSDK.Logger.Info("[CP_SDK.OBS][Service.WebSocket_OnClose]");

            if (Status != EStatus.AuthRejected)
                Status = EStatus.Disconnected;
        }
        /// <summary>
        /// On web socket message
        /// </summary>
        /// <param name="p_Message">Received message</param>
        private static void WebSocket_OnMessageReceived(string p_Message)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger.Info("[CP_SDK.OBS][Service.WebSocket_OnMessageReceived]");
#endif

            try
            {
                var l_JObject = JObject.Parse(p_Message);

                if (Status == EStatus.Authing)
                {
                    if (l_JObject.ContainsKey("error") &&
                        (  l_JObject["error"].Value<string>() == "Not Authenticated"
                        || l_JObject["error"].Value<string>() == "Authentication Failed."))
                    {
                        Status = EStatus.AuthRejected;
                        m_Client.Disconnect();
                        return;
                    }

                    if (   l_JObject.ContainsKey("message-id") && l_JObject["message-id"]?.Value<string>() == "Authenticate"
                        && l_JObject.ContainsKey("status"))
                    {
                        if (l_JObject["status"]?.Value<string>() == "ok")
                        {
                            Status = EStatus.Connected;
                            DoInitialQueries();
                            return;
                        }
                        else
                        {
                            Status = EStatus.AuthRejected;
                            m_Client.Disconnect();
                            return;
                        }
                    }
                }

                if (l_JObject.ContainsKey("message-id"))
                {
                    var l_MessageID = l_JObject["message-id"]?.Value<string>() ?? "None";

                    switch (l_MessageID)
                    {
                        case "GetAuthRequired":     Handle_GetAuthRequired(l_JObject);      break;
                        case "GetSceneList":        Handle_GetSceneList(l_JObject);         break;
                        case "GetPreviewScene":     Handle_GetPreviewScene(l_JObject);      break;
                        case "GetTransitionList":   Handle_GetTransitionList(l_JObject);    break;
                        case "GetStreamingStatus":  Handle_GetStreamingStatus(l_JObject);   break;
                        case "GetRecordingStatus":  Handle_GetRecordingStatus(l_JObject);   break;
                    }
                }
                else if (l_JObject.ContainsKey("update-type"))
                {
                    var l_UpdateType = l_JObject["update-type"]?.Value<string>() ?? "None";

#if DEBUG
                    ChatPlexUnitySDK.Logger.Info("[CP_SDK.OBS][Service.WebSocket_OnMessageReceived] UpdateType: " + l_UpdateType);
#endif
                    switch (l_UpdateType)
                    {
                        case "SwitchScenes":              Update_SwitchScenes(l_JObject);                 break;
                        case "SceneItemVisibilityChanged":Update_SceneItemVisibilityChanged(l_JObject);   break;

                        case "StudioModeSwitched":        Update_StudioModeSwitched(l_JObject);           break;
                        case "PreviewSceneChanged":       Update_PreviewSceneChanged(l_JObject);          break;

                        case "StreamStarted":             Update_StreamStarted(l_JObject);                break;
                        case "StreamStopped":             Update_StreamStopped(l_JObject);                break;

                        case "RecordingStarted":          Update_RecordingStarted(l_JObject);             break;
                        case "RecordingStopped":          Update_RecordingStopped(l_JObject);             break;
#if DEBUG
                        default:
                            ChatPlexUnitySDK.Logger.Error("[CP_SDK.OBS][Service.WebSocket_OnMessageReceived] Unhandled:");
                            ChatPlexUnitySDK.Logger.Error(p_Message);
                            break;
#endif
                    }

                }
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.OBS][Service.WebSocket_OnMessageReceived] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On web socket error
        /// </summary>
        private static void WebSocket_OnError()
        {
            ChatPlexSDK.Logger.Info("[CP_SDK.OBS][Service.WebSocket_OnError]");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do initial queries
        /// </summary>
        private static void DoInitialQueries()
        {
            SendRequest("GetSceneList");
            SendRequest("GetPreviewScene");
            SendRequest("GetTransitionList");
            SendRequest("GetStreamingStatus");
            SendRequest("GetRecordingStatus");
        }
        /// <summary>
        /// Send basic request to OBS
        /// </summary>
        /// <param name="p_Type">Request type</param>
        private static void SendRequest(string p_Type)
        {
            if (!m_Client.IsConnected)
                return;

            m_Client.SendMessage("{\"request-type\": \"" + p_Type + "\", \"message-id\": \"" + p_Type + "\" }");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh all the scenes
        /// </summary>
        public static void RefreshSceneList()
            => SendRequest("GetSceneList");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static JObject s_SetCurrentScene_Request = new JObject()
        {
            ["request-type"]    = "SetCurrentScene",
            ["scene-name"]      = "",
            ["message-id"]      = ""
        };
        /// <summary>
        /// Change active scene
        /// </summary>
        /// <param name="p_Scene">Scene to switch to</param>
        internal static void SwitchToScene(Models.Scene p_Scene)
        {
            if (p_Scene == null)
                return;

            s_SetCurrentScene_Request["scene-name"] = p_Scene.name;

            m_Client.SendMessage(s_SetCurrentScene_Request.ToString(Newtonsoft.Json.Formatting.None));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable studio mode
        /// </summary>
        public static void EnableStudioMode() => SendRequest("EnableStudioMode");
        /// <summary>
        /// Disable studio mode
        /// </summary>
        public static void DisableStudioMode() => SendRequest("DisableStudioMode");
        private static JObject s_SetPreviewScene_Request = new JObject()
        {
            ["request-type"]    = "SetPreviewScene",
            ["scene-name"]      = "",
            ["message-id"]      = ""
        };
        /// <summary>
        /// Change active preview scene
        /// </summary>
        /// <param name="p_Scene">Scene to switch to</param>
        internal static void SwitchPreviewToScene(Models.Scene p_Scene)
        {
            if (p_Scene == null)
                return;

            s_SetPreviewScene_Request["scene-name"] = p_Scene.name;

            m_Client.SendMessage(s_SetPreviewScene_Request.ToString(Newtonsoft.Json.Formatting.None));
        }
        private static JObject s_TransitionToProgram_Request = new JObject()
        {
            ["request-type"]    = "TransitionToProgram",
            ["message-id"]      = ""
        };
        private static JObject s_TransitionToProgram_WidthTransition = new JObject()
        {

        };
        /// <summary>
        /// Preview transition to scene
        /// </summary>
        /// <param name="p_Duration">Transition duration</param>
        /// <param name="p_TransitionName">Transition name</param>
        public static void PreviewTransitionToScene(int p_Duration = -1, string p_TransitionName = null)
        {
            if (p_Duration > -1 || p_TransitionName != null)
            {
                if (p_Duration > -1)
                    s_TransitionToProgram_WidthTransition["duration"] = p_Duration;
                else
                    s_TransitionToProgram_WidthTransition.Remove("duration");

                if (p_TransitionName != null)
                    s_TransitionToProgram_WidthTransition["name"] = p_TransitionName;
                else
                    s_TransitionToProgram_WidthTransition.Remove("name");

                s_TransitionToProgram_Request["with-transition"] = s_TransitionToProgram_WidthTransition;
            }

            m_Client.SendMessage(s_TransitionToProgram_Request.ToString(Newtonsoft.Json.Formatting.None));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start streaming
        /// </summary>
        public static void StartStreaming() => SendRequest("StartStreaming");
        /// <summary>
        /// Stop streaming
        /// </summary>
        public static void StopStreaming() => SendRequest("StopStreaming");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start recording
        /// </summary>
        public static void StartRecording() => SendRequest("StartRecording");
        /// <summary>
        /// Stop recording
        /// </summary>
        public static void StopRecording() => SendRequest("StopRecording");
        private static JObject s_SetFilenameFormatting_Request = new JObject()
        {
            ["request-type"]        = "SetFilenameFormatting",
            ["filename-formatting"] = "",
            ["message-id"]          = ""
        };
        /// <summary>
        /// Set record filename format
        /// </summary>
        /// <param name="p_Format">New format</param>
        public static void SetRecordFilenameFormat(string p_Format)
        {
            s_SetFilenameFormatting_Request["filename-formatting"] = p_Format;
            m_Client.SendMessage(s_SetFilenameFormatting_Request.ToString(Newtonsoft.Json.Formatting.None));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static JObject s_SetSourceVisible_Request = new JObject()
        {
            ["request-type"]    = "SetSceneItemProperties",
            ["scene-name"]      = "",
            ["item"]            = "",
            ["visible"]         = true,
            ["message-id"]      = ""
        };
        /// <summary>
        /// Set source visibility
        /// </summary>
        /// <param name="p_Scene">Scene that contain the source</param>
        /// <param name="p_Source">Source instance</param>
        /// <param name="p_Visibility">New visibility</param>
        internal static void SetSourceVisible(Models.Scene p_Scene, Models.Source p_Source, bool p_Visibility)
        {
            if (p_Scene == null || p_Source == null)
                return;

            s_SetSourceVisible_Request["scene-name"]    = p_Scene.name;
            s_SetSourceVisible_Request["item"]          = p_Source.name;
            s_SetSourceVisible_Request["visible"]       = p_Visibility;

            m_Client.SendMessage(s_SetSourceVisible_Request.ToString(Newtonsoft.Json.Formatting.None));
        }
        private static JObject s_SetSourceMuted_Request = new JObject()
        {
            ["request-type"]    = "SetMute",
            ["source"]          = "",
            ["mute"]            = true,
            ["message-id"]      = ""
        };
        /// <summary>
        /// Set source muted
        /// </summary>
        /// <param name="p_Scene">Scene that contain the source</param>
        /// <param name="p_Source">Source instance</param>
        /// <param name="p_Muted">New state</param>
        internal static void SetSourceMuted(Models.Scene p_Scene, Models.Source p_Source, bool p_Muted)
        {
            if (p_Scene == null || p_Source == null)
                return;

            s_SetSourceMuted_Request["source"]  = p_Source.name;
            s_SetSourceMuted_Request["mute"]    = p_Muted;

            m_Client.SendMessage(s_SetSourceMuted_Request.ToString(Newtonsoft.Json.Formatting.None));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deserialize scene
        /// </summary>
        /// <param name="p_Name">Scene name</param>
        /// <param name="p_Scene">Scene content</param>
        private static void DeserializeScene(string p_Name, JObject p_Scene)
        {
            if (m_Scenes.TryGetValue(p_Name, out var l_Existing))
                l_Existing.Deserialize(p_Scene, true);
            else
            {
                var l_NewScene = Models.Scene.FromJObject(p_Scene);
                m_Scenes.TryAdd(p_Name, l_NewScene);
            }
        }
    }
}
