using System.Collections;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_SongChartVisualizer
{
    /// <summary>
    /// SongChartVisualizer Module
    /// </summary>
    public class SongChartVisualizer : CP_SDK.ModuleBase<SongChartVisualizer>
    {
        public const int MaxPoints = 100;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Song Chart Visualizer";
        public override string                              Description         => "Get spoiled about the map difficulty!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#song-chart-visualizer";
        public override bool                                UseChatFeatures     => false;
        public override bool                                IsEnabled           { get => SCVConfig.Instance.Enabled; set { SCVConfig.Instance.Enabled = value; SCVConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView     m_SettingsLeftView  = null;
        private UI.SettingsMainView     m_SettingsMainView  = null;
        private UI.SettingsRightView    m_SettingsRightView = null;

        private Transform                           m_RootTransform             = null;
        private CP_SDK.UI.Components.CFloatingPanel m_ChartFloatingPanel        = null;
        private UI.ChartFloatingPanelView           m_ChartFloatingPanelView    = null;

#if BEATSABER
        private AudioTimeSyncController m_AudioTimeSyncController = null;
#else
#error Missing game implementation
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            CP_SDK.ChatPlexSDK.OnGenericSceneChange += ChatPlexSDK_OnGenericSceneChange;
#if BEATSABER
            CP_SDK_BS.Game.Logic.OnLevelStarted += Game_LevelStarted;
#else
#error Missing game implementation
#endif

            try
            {
                /// Master GameObject
                m_RootTransform = new GameObject("ChatPlexMod_SongChartVisualizer").transform;
                m_RootTransform.transform.position = Vector3.zero;
                m_RootTransform.transform.rotation = Quaternion.identity;

                GameObject.DontDestroyOnLoad(m_RootTransform);

                m_ChartFloatingPanel = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("ChartFloatingPanel", m_RootTransform);
                m_ChartFloatingPanel.SetSize(new Vector2(105.0f, 65.0f));
                m_ChartFloatingPanel.SetBackground(true);
                m_ChartFloatingPanel.SetBackgroundColor(SCVConfig.Instance.BackgroundColor);
                m_ChartFloatingPanel.OnSceneRelease(CP_SDK.EGenericScene.Playing, ChartFloatingPanel_OnRelease);
                m_ChartFloatingPanel.SetRadius(0f);

                m_ChartFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.ChartFloatingPanelView>();
                m_ChartFloatingPanel.SetViewController(m_ChartFloatingPanelView);

                m_RootTransform.gameObject.SetActive(false);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_SongChartVisualizer][SongChartVisualizer.OnEnable] Failed to destroy floating panel");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            try
            {
                if (m_RootTransform == null)
                    return;

                CP_SDK.UI.UISystem.DestroyUI(ref m_ChartFloatingPanel, ref m_ChartFloatingPanelView);

                GameObject.Destroy(m_RootTransform.gameObject);

                /// Reset variables
                m_RootTransform             = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_SongChartVisualizer][SongChartVisualizer.OnDisable] Failed to destroy floating panel");
                Logger.Instance.Error(l_Exception);
            }

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsRightView);

            /// Unbind event
            CP_SDK.ChatPlexSDK.OnGenericSceneChange -= ChatPlexSDK_OnGenericSceneChange;
#if BEATSABER
            CP_SDK_BS.Game.Logic.OnLevelStarted -= Game_LevelStarted;
#else
#error Missing game implementation
#endif
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsLeftView == null)     m_SettingsLeftView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();
            if (m_SettingsMainView == null)     m_SettingsMainView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsRightView == null)    m_SettingsRightView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsRightView>();

            /// Change main view
            return (m_SettingsMainView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On generic scene change
        /// </summary>
        /// <param name="p_GenericScene"></param>
        private void ChatPlexSDK_OnGenericSceneChange(CP_SDK.EGenericScene p_GenericScene)
        {
            if (p_GenericScene != CP_SDK.EGenericScene.Menu)
                return;

            m_RootTransform.gameObject.SetActive(false);
        }
#if BEATSABER
        /// <summary>
        /// When a level start
        /// </summary>
        /// <param name="p_Data">Level data</param>
        private void Game_LevelStarted(CP_SDK_BS.Game.LevelData p_Data)
        {
            /// Not enabled in multi-player
            if (p_Data.Type == CP_SDK_BS.Game.LevelType.Multiplayer)
                return;

            /// Start the task
            CP_SDK.Unity.MTCoroutineStarter.Start(PrepareFloatingPanel());
        }
#else
#error Missing game implementation
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preview enabled
        /// </summary>
        /// <param name="p_Enabled">Is enabled?</param>
        internal void SetPreview(bool p_Enabled)
        {
            m_RootTransform.gameObject.SetActive(p_Enabled);

            if (p_Enabled)
            {
                m_ChartFloatingPanel.SetTransformDirect(new Vector3(3.38f, 1.20f, 2.29f), new Vector3(0.00f, 58.00f, 0.00f));
                m_ChartFloatingPanel.SetLockIcon(CP_SDK.UI.Components.CFloatingPanel.ECorner.None);
                m_ChartFloatingPanelView.SetGraph(Data.GraphBuilder.GetSampleNPSGraph());
            }
        }
        /// <summary>
        /// Update style
        /// </summary>
        internal void UpdateStyle()
        {
            m_ChartFloatingPanel?.SetBackgroundColor(SCVConfig.Instance.BackgroundColor);
            m_ChartFloatingPanelView?.UpdateStyle();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggle chat visibility
        /// </summary>
        public void ToggleVisibility()
        {
            if (m_RootTransform && m_RootTransform.localScale.x > 0.5f)
                m_RootTransform.localScale = Vector3.zero;
            else if (m_RootTransform)
                m_RootTransform.localScale = Vector3.one;
        }
        /// <summary>
        /// Set visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        public void SetVisible(bool p_Visible)
        {
            if (!m_RootTransform)
                return;

            m_RootTransform.localScale = p_Visible ? Vector3.one : Vector3.zero;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create the chart visualizer
        /// </summary>
        /// <returns></returns>
        private IEnumerator PrepareFloatingPanel()
        {
            yield return new WaitForEndOfFrame();

#if BEATSABER
            if (CP_SDK_BS.Game.Logic.LevelData?.Data?.playerSpecificSettings?.noTextsAndHuds ?? false)
            {
                m_RootTransform.gameObject.SetActive(false);
                yield break;
            }

            var l_TransformedBeatmapData    = CP_SDK_BS.Game.Logic.LevelData?.Data?.transformedBeatmapData;
            var l_AudioClip                 = CP_SDK_BS.Game.Logic.LevelData?.Data?.songAudioClip;
            var l_SongDuration              = l_AudioClip?.length ?? -1f;

            if (l_TransformedBeatmapData    == null
                || l_AudioClip              == null
                || l_SongDuration           == -1f)
            {
                yield break;
            }

            var l_HasRotation = CP_SDK_BS.Game.Logic.LevelData?.HasRotations ?? false;
#else
#error Missing game implementation
#endif
            var l_Position = l_HasRotation ? SCVConfig.Instance.ChartRotatingPosition : SCVConfig.Instance.ChartStandardPosition;
            var l_Rotation = l_HasRotation ? SCVConfig.Instance.ChartRotatingRotation : SCVConfig.Instance.ChartStandardRotation;

            m_ChartFloatingPanel.SetTransformDirect(l_Position, l_Rotation);
            m_RootTransform.gameObject.SetActive(true);

            m_ChartFloatingPanelView.SetGraph(Data.GraphBuilder.BuildNPSGraph(l_TransformedBeatmapData, l_SongDuration));
            m_ChartFloatingPanel.SetLockIcon(SCVConfig.Instance.ShowLockIcon ? CP_SDK.UI.Components.CFloatingPanel.ECorner.TopRight : CP_SDK.UI.Components.CFloatingPanel.ECorner.None);

#if BEATSABER
            var l_Waiter = new WaitForSeconds(0.25f);

            m_AudioTimeSyncController = null;
            while (m_AudioTimeSyncController == null)
            {
                m_AudioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
                if (m_AudioTimeSyncController != null)
                    break;

                if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Playing)
                    yield break;

                yield return l_Waiter;
            }

            m_ChartFloatingPanelView.SetGetSongTimeFunction(() => m_AudioTimeSyncController?.songTime ?? 0f);

            var l_RotationFollow = null as Transform;
            if (l_HasRotation)
            {
                var l_Atempt = 0;
                while (l_RotationFollow == null)
                {
                    var l_FlyingGameHUDRotation = Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault();
                    if (l_FlyingGameHUDRotation != null)
                    {
                        l_RotationFollow = l_FlyingGameHUDRotation.transform;
                        break;
                    }

                    if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Playing)
                        yield break;

                    l_Atempt++;
                    if (l_Atempt > 3)
                        break;

                    yield return l_Waiter;
                }
            }
#else
#error Missing game implementation
#endif

            m_ChartFloatingPanelView.SetRotationFollow(m_RootTransform, l_RotationFollow);

            if (!l_RotationFollow && m_RootTransform)
                m_RootTransform.localRotation = Quaternion.identity;
        }
        /// <summary>
        /// When the floating panel is moved
        /// </summary>
        /// <param name="p_LocalPosition">New local position</param>
        /// <param name="p_LocalEulerAngles">New local euler angles</param>
        private void ChartFloatingPanel_OnRelease(Vector3 p_LocalPosition, Vector3 p_LocalEulerAngles)
        {
#if BEATSABER
            var l_HasRotation = CP_SDK_BS.Game.Logic.LevelData?.HasRotations ?? false;
#else
#error Missing game implementation
#endif

            if (!l_HasRotation)
            {
                SCVConfig.Instance.ChartStandardPosition = p_LocalPosition;
                SCVConfig.Instance.ChartStandardRotation = p_LocalEulerAngles;
            }
            else
            {
                SCVConfig.Instance.ChartRotatingPosition = p_LocalPosition;
                SCVConfig.Instance.ChartRotatingRotation = p_LocalEulerAngles;
            }
        }
    }
}
