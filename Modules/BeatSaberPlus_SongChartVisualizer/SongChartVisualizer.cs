using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Collections;
using UnityEngine;

namespace BeatSaberPlus_SongChartVisualizer
{
    /// <summary>
    /// SongChartVisualizer Module
    /// </summary>
    public class SongChartVisualizer : BeatSaberPlus.SDK.BSPModuleBase<SongChartVisualizer>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override CP_SDK.EIModuleBaseType Type => CP_SDK.EIModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Song Chart Visualizer";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Get spoiled about the map difficulty!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => SCVConfig.Instance.Enabled; set { SCVConfig.Instance.Enabled = value; SCVConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override CP_SDK.EIModuleBaseActivationType ActivationType => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// SongChartVisualizer view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// SongChartVisualizer left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// SongChartVisualizer right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Window GameObject
        /// </summary>
        private GameObject m_MasterGOB = null;
        /// <summary>
        /// Chart floating screen
        /// </summary>
        private FloatingScreen m_ChartFloatingScreen = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted += Game_LevelStarted;
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted -= Game_LevelStarted;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            /// Create view if needed
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            /// Create view if needed
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a level start
        /// </summary>
        /// <param name="p_Data">Level data</param>
        private void Game_LevelStarted(BeatSaberPlus.SDK.Game.LevelData p_Data)
        {
            /// Not enabled in multi-player
            if (p_Data.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer)
                return;

            /// Start the task
            CP_SDK.Unity.MTCoroutineStarter.Start(CreateChartVisualizer());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh the preview
        /// </summary>
        internal void RefreshPreview()
        {
            DestroyChart();
            CP_SDK.Unity.MTCoroutineStarter.Start(CreateChartVisualizer());
        }
        /// <summary>
        /// Destroy the preview
        /// </summary>
        internal void DestroyChart()
        {
            if (m_MasterGOB == null || !m_MasterGOB)
                return;

            GameObject.Destroy(m_MasterGOB);
            m_MasterGOB = null;
        }
        /// <summary>
        /// Toggle chat visibility
        /// </summary>
        public void ToggleVisibility()
        {
            if (m_MasterGOB && m_MasterGOB.transform.localScale.x > 0.5f)
                m_MasterGOB.transform.localScale = Vector3.zero;
            else if (m_MasterGOB)
                m_MasterGOB.transform.localScale = Vector3.one;
        }
        /// <summary>
        /// Set visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        public void SetVisible(bool p_Visible)
        {
            if (m_MasterGOB)
                m_MasterGOB.transform.localScale = p_Visible ? Vector3.one : Vector3.zero;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create the chart visualizer
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateChartVisualizer()
        {
            yield return new WaitForEndOfFrame();

            var l_HasRotation = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.transformedBeatmapData?.spawnRotationEventsCount > 0;
            var l_Position = l_HasRotation ? SCVConfig.Instance.Chart360_90Position
                                           : SCVConfig.Instance.ChartStandardPosition;
            var l_Rotation = l_HasRotation ? SCVConfig.Instance.Chart360_90Rotation
                                           : SCVConfig.Instance.ChartStandardRotation;

            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
            {
                l_Position = new Vector3(2.38f, 1.20f, 1.29f);
                l_Rotation = new Vector3(0f, 58f, 0f);
            }

            m_ChartFloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(105, 65), false, l_Position, Quaternion.identity, 0f, true);
            m_ChartFloatingScreen.gameObject.AddComponent<Components.SongChart>();

            /// Set rotation
            m_ChartFloatingScreen.ScreenRotation = Quaternion.Euler(l_Rotation);

            /// Update background color
            m_ChartFloatingScreen.GetComponentInChildren<ImageView>().color = SCVConfig.Instance.BackgroundColor;

            /// Bind event
            m_ChartFloatingScreen.HandleReleased += OnFloatingWindowMoved;

            /// Create UI Controller
            var l_ChatFloatingScreenController = BeatSaberUI.CreateViewController<UI.FloatingWindow>();
            m_ChartFloatingScreen.SetRootViewController(l_ChatFloatingScreenController, HMUI.ViewController.AnimationType.None);
            l_ChatFloatingScreenController.gameObject.SetActive(true);
            m_ChartFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = 4;

            /// Master GameObject
            m_MasterGOB = new GameObject("BeatSaberPlus_SongChartVisualizer");
            m_MasterGOB.transform.position = Vector3.zero;
            m_MasterGOB.transform.rotation = Quaternion.identity;

            /// Apply parent
            m_ChartFloatingScreen.transform.SetParent(m_MasterGOB.transform);
        }
        /// <summary>
        /// When the floating window is moved
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void OnFloatingWindowMoved(object p_Sender, FloatingScreenHandleEventArgs p_Event)
        {
            /// Always parallel to the floor
            if (SCVConfig.Instance.AlignWithFloor)
                m_ChartFloatingScreen.transform.localEulerAngles = new Vector3(m_ChartFloatingScreen.transform.localEulerAngles.x, m_ChartFloatingScreen.transform.localEulerAngles.y, 0);

            /// Don't update from preview
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                return;

            var l_HasRotation = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.transformedBeatmapData?.spawnRotationEventsCount > 0;
            if (!l_HasRotation)
            {
                SCVConfig.Instance.ChartStandardPosition = m_ChartFloatingScreen.transform.localPosition;
                SCVConfig.Instance.ChartStandardRotation = m_ChartFloatingScreen.transform.localEulerAngles;
            }
            else
            {
                SCVConfig.Instance.Chart360_90Position = m_ChartFloatingScreen.transform.localPosition;
                SCVConfig.Instance.Chart360_90Rotation = m_ChartFloatingScreen.transform.localEulerAngles;
            }
        }
    }
}
