using System;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Game helper
    /// </summary>
    public class Logic
    {
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            BS_Utils.Utilities.BSEvents.menuSceneActive             += BSEvents_menuSceneActive;
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh    += BSEvents_lateMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.gameSceneActive             += BSEvents_gameSceneActive;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On menu scene active
        /// </summary>
        private static void BSEvents_menuSceneActive()
        {
            ActiveScene = SceneType.Menu;

            if (OnSceneChange != null)
                OnSceneChange.Invoke(ActiveScene);
        }
        /// <summary>
        /// On menu scene loaded
        /// </summary>
        /// <param name="p_Object">Transition object</param>
        private static void BSEvents_lateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO p_Object)
        {
            UI.LevelDetail.Init();

            if (OnMenuSceneLoaded != null)
                OnMenuSceneLoaded.Invoke();
        }
        /// <summary>
        /// On game scene active
        /// </summary>
        private static void BSEvents_gameSceneActive()
        {
            ActiveScene = SceneType.Playing;

            if (OnSceneChange != null)
                OnSceneChange.Invoke(ActiveScene);
        }
    }
}
