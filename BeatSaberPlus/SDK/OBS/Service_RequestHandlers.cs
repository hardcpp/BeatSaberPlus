using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BeatSaberPlus.SDK.OBS
{
    /// <summary>
    /// Service request handlers
    /// </summary>
    public partial class Service
    {
        private static JObject s_Authenticate_Reply = new JObject()
        {
            ["request-type"]    = "Authenticate",
            ["auth"]            = "",
            ["message-id"]      = "Authenticate"
        };
        /// <summary>
        /// GetAuthRequired request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetAuthRequired(JObject p_JObject)
        {
            var l_IsAuthRequired = p_JObject["authRequired"]?.Value<bool>() ?? false;

            if (l_IsAuthRequired)
            {
                var l_Challenge     = p_JObject["challenge"]?.Value<string>() ?? "";
                var l_Salt          = p_JObject["salt"]?.Value<string>() ?? "";
                var l_Secret        = System.Convert.ToBase64String(Cryptography.SHA256.GetHash(BSPConfig.Instance.OBS.Pssword + l_Salt));
                var l_AuthResponse  = System.Convert.ToBase64String(Cryptography.SHA256.GetHash(l_Secret + l_Challenge));

                s_Authenticate_Reply["auth"] = l_AuthResponse;
                m_Client.SendMessage(s_Authenticate_Reply.ToString(Newtonsoft.Json.Formatting.None));
            }
            else
            {
                Status = EStatus.Connected;
                DoInitialQueries();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// GetSceneList request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetSceneList(JObject p_JObject)
        {
            var l_CurrentSceneName  = p_JObject["current-scene"]?.Value<string>() ?? null;
            var l_Scenes            = p_JObject["scenes"] as JArray;

            var l_ExistingScenes = Pool.MTListPool<string>.Get();
            try
            {
                l_ExistingScenes.Clear();

                for (int l_I = 0; l_I < l_Scenes.Count; ++l_I)
                {
                    var l_SceneName = l_Scenes[l_I]["name"].Value<string>();
                    DeserializeScene(l_SceneName, l_Scenes[l_I] as JObject);
                    l_ExistingScenes.Add(l_SceneName);
                }

                var l_CurrentSceneList = m_Scenes.Keys.ToArray();
                for (int l_I = 0; l_I < l_CurrentSceneList.Length; ++l_I)
                {
                    if (l_ExistingScenes.Contains(l_CurrentSceneList[l_I]))
                        continue;

                    if (m_Scenes.TryRemove(l_CurrentSceneList[l_I], out var l_SceneToRemove))
                    {
                        Logger.Instance.Debug("[SDK.OBS][Service.Handle_GetSceneList] Scene \"" + l_SceneToRemove.name + "\" was destroyed!");
                        Models.Scene.Release(l_SceneToRemove);
                    }
                }
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.OBS][Service.Handle_GetSceneList] Error:");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                l_ExistingScenes.Clear();
                Pool.MTListPool<string>.Release(l_ExistingScenes);
            }

            OnSceneListRefreshed?.Invoke();

            if ((ActiveScene == null || ActiveScene.name != l_CurrentSceneName) && m_Scenes.TryGetValue(l_CurrentSceneName, out var l_Scene))
            {
                var l_OldScene = ActiveScene;
                ActiveScene = l_Scene;

                OnActiveSceneChanged?.Invoke(l_OldScene, l_Scene);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// GetPreviewScene request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetPreviewScene(JObject p_JObject)
        {
            var l_OldPreviewScene = ActivePreviewScene;

            if (p_JObject.ContainsKey("error"))
            {
                ActivePreviewScene  = ActiveScene;
                IsInStudioMode      = false;
                OnStudioModeChanged?.Invoke(false, ActiveScene);
            }
            else
            {
                var l_NewActivePreviewScene = p_JObject["name"]?.Value<string>() ?? null;
                DeserializeScene(l_NewActivePreviewScene, p_JObject);

                IsInStudioMode = true;

                if ((ActivePreviewScene == null || ActivePreviewScene.name != l_NewActivePreviewScene) && m_Scenes.TryGetValue(l_NewActivePreviewScene, out var l_Scene))
                    OnStudioModeChanged?.Invoke(true, ActivePreviewScene);
                else
                    OnStudioModeChanged?.Invoke(true, ActiveScene);
            }
        }
        /// <summary>
        /// GetTransitionList request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetTransitionList(JObject p_JObject)
        {
            var l_Transitions = p_JObject.ContainsKey("transitions") && p_JObject["transitions"].Type == JTokenType.Array ? p_JObject["transitions"] as JArray : null;
            if (l_Transitions != null)
            {
                var l_Names = new List<string>();
                for (int l_I = 0; l_I < l_Transitions.Count; ++l_I)
                {
                    var l_Name = l_Transitions[l_I]["name"]?.Value<string>() ?? null;
                    if (!string.IsNullOrEmpty(l_Name))
                        l_Names.Add(l_Name);
                }

                m_Transitions = l_Names.ToArray();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// GetStreamingStatus request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetStreamingStatus(JObject p_JObject)
        {
            var l_IsStreaming = p_JObject["streaming"]?.Value<bool>() ?? false;

            if (l_IsStreaming != IsStreaming)
            {
                IsStreaming = l_IsStreaming;
                OnStreamingStatusChanged?.Invoke(IsStreaming);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// GetRecordingStatus request callback
        /// </summary>
        /// <param name="p_JObject">Reply</param>
        private static void Handle_GetRecordingStatus(JObject p_JObject)
        {
            var l_IsRecording = p_JObject["isRecording"]?.Value<bool>() ?? false;

            if (l_IsRecording != IsRecording)
            {
                IsRecording = l_IsRecording;
                OnRecordingStatusChanged?.Invoke(IsRecording);
            }
        }
    }
}
