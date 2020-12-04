using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Plugins.SongChartVisualizer
{
    /// <summary>
    /// SongChartVisualizer plugin
    /// </summary>
    class SongChartVisualizer : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Song Chart Visualizer";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.SongChartVisualizer.Enabled; set => Config.SongChartVisualizer.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static SongChartVisualizer Instance = null;

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
        /// Settings view accessor
        /// </summary>
        internal UI.Settings settingsView => m_SettingsView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Set singleton
            Instance = this;

            /// Bind event
            Utils.Game.OnSceneChange += Game_OnSceneChange;

            /// Temp
            Config.SongChartVisualizer.ResetPosition();
        }
        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            Utils.Game.OnSceneChange -= Game_OnSceneChange;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected override void ShowUIImplementation()
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
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">Scene type</param>
        private void Game_OnSceneChange(Utils.Game.SceneType p_Scene)
        {
            m_MasterGOB = null;

            if (p_Scene != Utils.Game.SceneType.Playing)
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

            if (Utils.Game.ActiveScene == Utils.Game.SceneType.Menu)
            {
                l_Position = new Vector3(2.38f, 1.20f, 1.29f);
                l_Rotation = new Vector3(0f, 58f, 0f);
            }

            var l_FloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(105, 65), false, l_Position, Quaternion.identity, 0f, true);
            l_FloatingScreen.gameObject.AddComponent<Components.SongChart>();

            /// Set rotation
            l_FloatingScreen.ScreenRotation = Quaternion.Euler(l_Rotation);

            /// Update background color
            var l_Color = l_FloatingScreen.GetComponentInChildren<ImageView>().color;
            l_Color.r = Config.SongChartVisualizer.BackgroundR;
            l_Color.g = Config.SongChartVisualizer.BackgroundG;
            l_Color.b = Config.SongChartVisualizer.BackgroundB;
            l_Color.a = Config.SongChartVisualizer.BackgroundA;

            l_FloatingScreen.GetComponentInChildren<ImageView>().color = l_Color;

            /// Master GameObject
            m_MasterGOB = new GameObject("BeatSaberPlus_SongChartVisualizer");
            m_MasterGOB.transform.position = Vector3.zero;
            m_MasterGOB.transform.rotation = Quaternion.identity;

            /// Apply parent
            l_FloatingScreen.transform.SetParent(m_MasterGOB.transform);
        }
    }
}
