using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Collections;
using UnityEngine;

namespace BeatSaberPlus.Modules.SongChartVisualizer
{
    /// <summary>
    /// SongChartVisualizer Module
    /// </summary>
    internal class SongChartVisualizer : SDK.ModuleBase<SongChartVisualizer>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
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
        public override bool IsEnabled { get => Config.SongChartVisualizer.Enabled; set => Config.SongChartVisualizer.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            SDK.Game.Logic.OnSceneChange += Game_OnSceneChange;

            /// Temp
            Config.SongChartVisualizer.ResetPosition();
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            SDK.Game.Logic.OnSceneChange -= Game_OnSceneChange;
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
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">Scene type</param>
        private void Game_OnSceneChange(SDK.Game.Logic.SceneType p_Scene)
        {
            m_MasterGOB = null;

            if (p_Scene != SDK.Game.Logic.SceneType.Playing)
                return;

            /// Start the task
            SharedCoroutineStarter.instance.StartCoroutine(CreateChartVisualizer());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh the preview
        /// </summary>
        internal void RefreshPreview()
        {
            DestroyPreview();
            SharedCoroutineStarter.instance.StartCoroutine(CreateChartVisualizer());
        }
        /// <summary>
        /// Destroy the preview
        /// </summary>
        internal void DestroyPreview()
        {
            if (m_MasterGOB == null || !m_MasterGOB)
                return;

            GameObject.Destroy(m_MasterGOB);
            m_MasterGOB = null;
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

            var l_HasRotation = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.difficultyBeatmap?.beatmapData?.spawnRotationEventsCount > 0;
            var l_Position = l_HasRotation ? new Vector3(Config.SongChartVisualizer.Chart360_90PositionX,   Config.SongChartVisualizer.Chart360_90PositionY,    Config.SongChartVisualizer.Chart360_90PositionZ)
                                           : new Vector3(Config.SongChartVisualizer.ChartStandardPositionX, Config.SongChartVisualizer.ChartStandardPositionY,  Config.SongChartVisualizer.ChartStandardPositionZ);
            var l_Rotation = l_HasRotation ? new Vector3(Config.SongChartVisualizer.Chart360_90RotationX,   Config.SongChartVisualizer.Chart360_90RotationY,    Config.SongChartVisualizer.Chart360_90RotationZ)
                                           : new Vector3(Config.SongChartVisualizer.ChartStandardRotationX, Config.SongChartVisualizer.ChartStandardRotationY,  Config.SongChartVisualizer.ChartStandardRotationZ);

            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Menu)
            {
                l_Position = new Vector3(2.38f, 1.20f, 1.29f);
                l_Rotation = new Vector3(0f, 58f, 0f);
            }

            var l_FloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(105, 65), false, l_Position, Quaternion.identity, 0f, true);
            l_FloatingScreen.gameObject.AddComponent<Components.SongChart>();

            /// Set rotation
            l_FloatingScreen.ScreenRotation = Quaternion.Euler(l_Rotation);

            /// Update background color
            l_FloatingScreen.GetComponentInChildren<ImageView>().color = Config.SongChartVisualizer.BackgroundColor;

            /// Master GameObject
            m_MasterGOB = new GameObject("BeatSaberPlus_SongChartVisualizer");
            m_MasterGOB.transform.position = Vector3.zero;
            m_MasterGOB.transform.rotation = Quaternion.identity;

            /// Apply parent
            l_FloatingScreen.transform.SetParent(m_MasterGOB.transform);
        }
    }
}
