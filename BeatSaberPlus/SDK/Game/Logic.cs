using IPA.Loader;
using System;
using System.Linq;
using System.Reflection;
using Zenject;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Game helper
    /// </summary>
    public class Logic
    {
        /// <summary>
        /// Last main scene was not menu ?
        /// </summary>
        private static bool m_LastMainSceneWasNotMenu = false;
        /// <summary>
        /// Was in replay ?
        /// </summary>
        private static bool m_WasInReplay = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Scenes
        /// </summary>
        public enum SceneType
        {
            None,
            Menu,
            Playing
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Active scene type
        /// </summary>
        public static SceneType ActiveScene { get; private set; } = SceneType.None;
        /// <summary>
        /// Current level data
        /// </summary>
        public static LevelData LevelData { get; private set; } = null;
        /// <summary>
        /// Level completion data
        /// </summary>
        public static LevelCompletionData LevelCompletionData { get; private set; } = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On scene change
        /// </summary>
        public static event Action<SceneType> OnSceneChange;
        /// <summary>
        /// On menu scene loaded
        /// </summary>
        public static event Action OnMenuSceneLoaded;
        /// <summary>
        /// On level started
        /// </summary>
        public static event Action<LevelData> OnLevelStarted;
        /// <summary>
        /// On level ended
        /// </summary>
        public static event Action<LevelCompletionData> OnLevelEnded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On Unity scene change
        /// </summary>
        /// <param name="p_Current">Current scene</param>
        /// <param name="p_Next">Next scene</param>
        private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene p_Current, UnityEngine.SceneManagement.Scene p_Next)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error($"====== [SDK.Game][Logic.SceneManager_activeSceneChanged] {p_Next.name} ======");
#endif

            try
            {
                if (p_Next.name == "GameCore")
                {
                    if (ActiveScene != SceneType.Playing)
                        OnGameSceneActive();
                }
                else if (p_Next.name == "MainMenu")
                {
                    if (ActiveScene != SceneType.Menu)
                        OnMenuSceneActive();

                    var l_GameScenesManager = UnityEngine.Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
                    if (l_GameScenesManager != null)
                    {
                        if (p_Current.name == "EmptyTransition" && !m_LastMainSceneWasNotMenu)
                        {
                            l_GameScenesManager.transitionDidFinishEvent -= OnMenuSceneLoadedFresh;
                            l_GameScenesManager.transitionDidFinishEvent += OnMenuSceneLoadedFresh;
                        }
                    }

                    m_LastMainSceneWasNotMenu = false;
                }

                if (p_Next.name == "GameCore" || p_Next.name == "Credits" || p_Next.name == "BeatmapEditor")
                    m_LastMainSceneWasNotMenu = true;
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][Logic.SceneManager_activeSceneChanged] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On menu scene active
        /// </summary>
        private static void OnMenuSceneActive()
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error("====== [SDK.Game][Logic.OnMenuSceneActive] ======");
#endif
            try
            {
                ActiveScene = SceneType.Menu;
                LevelData   = null;

                CP_SDK.ChatPlexSDK.Fire_OnGenericMenuScene();

                if (OnSceneChange != null)
                    OnSceneChange.Invoke(ActiveScene);

                if (LevelCompletionData != null && OnLevelEnded != null)
                    OnLevelEnded.Invoke(LevelCompletionData);
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][Logic.OnMenuSceneActive] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// On menu scene loaded
        /// </summary>
        /// <param name="p_Object">Transition object</param>
        private static void OnMenuSceneLoadedFresh(ScenesTransitionSetupDataSO p_Object, DiContainer p_DiContainer)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error("====== [SDK.Game][Logic.OnMenuSceneLoadedFresh] ======");
#endif
            try
            {
                var l_GameScenesManager = UnityEngine.Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
                if (l_GameScenesManager != null)
                    l_GameScenesManager.transitionDidFinishEvent -= OnMenuSceneLoadedFresh;

                UI.LevelDetail.Init();

                ActiveScene             = SceneType.Menu;
                LevelData               = null;
                LevelCompletionData     = null;
                m_WasInReplay           = false;

                CP_SDK.ChatPlexSDK.Fire_OnGenericMenuSceneLoaded();

                if (OnMenuSceneLoaded != null)
                    OnMenuSceneLoaded.Invoke();

                OnMenuSceneActive();
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][Logic.OnMenuSceneLoadedFresh] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// On game scene active
        /// </summary>
        private static void OnGameSceneActive()
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error("====== [SDK.Game][Logic.OnGameSceneActive] ======");
#endif
            try
            {
                ActiveScene             = SceneType.Playing;
                LevelCompletionData     = null;
                m_WasInReplay           = Scoring.IsInReplay;

                CP_SDK.ChatPlexSDK.Fire_OnGenericPlayingScene();

                if (OnSceneChange != null)
                    OnSceneChange.Invoke(ActiveScene);

                if (LevelData != null)
                {
                    LevelData.IsReplay = m_WasInReplay;

#if DEBUG
                    ChatPlexUnitySDK.Logger?.Error($"====== [SDK.Game][Logic.OnGameSceneActive] OnLevelStarted ======");
#endif

                    if (OnLevelStarted != null)
                        OnLevelStarted.Invoke(LevelData);
                }
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[SDK.Game][Logic.OnGameSceneActive] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// On level started
        /// </summary>
        /// <param name="p_LevelData">Level data</param>
        internal static void FireLevelStarted(LevelData p_LevelData)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error("====== [SDK.Game][Logic.FireLevelStarted] ======");
#endif
            LevelData = p_LevelData;
        }
        /// <summary>
        /// On level ended
        /// </summary>
        /// <param name="p_LevelCompletionData">Level completion data</param>
        internal static void FireLevelEnded(LevelCompletionData p_LevelCompletionData)
        {
#if DEBUG
            ChatPlexUnitySDK.Logger?.Error("====== [SDK.Game][Logic.FireLevelEnded] ======");
#endif
            LevelCompletionData             = p_LevelCompletionData;
            LevelCompletionData.IsReplay    = m_WasInReplay;
        }
    }
}
