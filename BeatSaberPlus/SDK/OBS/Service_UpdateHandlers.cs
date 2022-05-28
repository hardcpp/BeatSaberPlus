using Newtonsoft.Json.Linq;

namespace BeatSaberPlus.SDK.OBS
{
    /// <summary>
    /// Server update handlers
    /// </summary>
    public partial class Service
    {
        /// <summary>
        /// SwitchScenes update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_SwitchScenes(JObject l_JObject)
        {
            var l_NewActiveScene = l_JObject["scene-name"]?.Value<string>() ?? null;

            DeserializeScene(l_NewActiveScene, l_JObject);

            if ((ActiveScene == null || ActiveScene.name != l_NewActiveScene) && m_Scenes.TryGetValue(l_NewActiveScene, out var l_Scene))
            {
                var l_OldScene = ActiveScene;
                ActiveScene = l_Scene;

                OnActiveSceneChanged?.Invoke(l_OldScene, l_Scene);
            }
        }
        /// <summary>
        /// SceneItemVisibilityChanged update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_SceneItemVisibilityChanged(JObject l_JObject)
        {
            var l_SceneName = l_JObject["scene-name"]?.Value<string>() ?? null;
            if (m_Scenes.TryGetValue(l_SceneName, out var l_Scene))
            {
                var l_SourceName    = l_JObject["item-name"]?.Value<string>() ?? null;
                var l_Source        = l_Scene.GetSourceByName(l_SourceName);

                if (l_Source != null)
                {
                    l_Source.render = l_JObject["item-visible"]?.Value<bool>() ?? true;
                    OnSourceVisibilityChanged?.Invoke(l_Scene, l_Source, l_Source.render);
                }
                else
                {
                    Logger.Instance.Error("[SDK.OBS][Service.Update_SceneItemVisibilityChanged] Source \"" + (l_SourceName != null ? l_SourceName : "<unk>") + "\" is missing, refreshing all scenes");
                    RefreshSceneList();
                }
            }
            else
            {
                Logger.Instance.Error("[SDK.OBS][Service.Update_SceneItemVisibilityChanged] Scene \"" + (l_SceneName != null ? l_SceneName : "<unk>") + "\" is missing, refreshing all scenes");
                RefreshSceneList();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// StudioModeSwitched update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_StudioModeSwitched(JObject l_JObject)
        {
            var l_OldValue = IsInStudioMode;
            var l_NewState = l_JObject["new-state"]?.Value<bool>() ?? false;

            ActivePreviewScene  = ActiveScene;
            IsInStudioMode      = l_NewState;
            OnStudioModeChanged?.Invoke(l_NewState, ActivePreviewScene);
        }
        /// <summary>
        /// PreviewSceneChanged update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_PreviewSceneChanged(JObject l_JObject)
        {
            var l_NewActivePreviewScene = l_JObject["scene-name"]?.Value<string>() ?? null;

            DeserializeScene(l_NewActivePreviewScene, l_JObject);

            if ((ActivePreviewScene == null || ActivePreviewScene.name != l_NewActivePreviewScene) && m_Scenes.TryGetValue(l_NewActivePreviewScene, out var l_Scene))
            {
                var l_OldScene = ActivePreviewScene;
                ActivePreviewScene = l_Scene;

                OnActiveSceneChanged?.Invoke(l_OldScene, l_Scene);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// StreamStarted update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_StreamStarted(JObject l_JObject)
        {
            if (IsStreaming)
                return;

            IsStreaming = true;
            OnStreamingStatusChanged?.Invoke(IsStreaming);
        }
        /// <summary>
        /// StreamStopped update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_StreamStopped(JObject l_JObject)
        {
            if (!IsStreaming)
                return;

            IsStreaming = false;
            OnStreamingStatusChanged?.Invoke(IsStreaming);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// RecordingStarted update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_RecordingStarted(JObject l_JObject)
        {
            if (IsRecording)
                return;

            LastRecordedFileName    = l_JObject["recordingFilename"]?.Value<string>() ?? string.Empty;
            IsRecording             = true;
            OnRecordingStatusChanged?.Invoke(IsRecording);
        }
        /// <summary>
        /// RecordingStopped update callback
        /// </summary>
        /// <param name="l_JObject">Reply</param>
        private static void Update_RecordingStopped(JObject l_JObject)
        {
            if (!IsRecording)
                return;

            LastRecordedFileName    = l_JObject["recordingFilename"]?.Value<string>() ?? string.Empty;
            IsRecording             = false;

            OnRecordingStatusChanged?.Invoke(IsRecording);
        }
    }
}
