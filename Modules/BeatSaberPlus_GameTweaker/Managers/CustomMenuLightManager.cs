using IPA.Utilities;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.Managers
{
    /// <summary>
    /// Custom menu light manager replacement
    /// </summary>
    public static class CustomMenuLightManager
    {
        /// <summary>
        /// Custom SerializedObject color for BeatGame serialized object
        /// </summary>
        private class CustomColorSO : ColorSO
        {
            public Color _color;
            public override Color color => _color;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static MenuLightsManager m_MenuLightsManager;
        private static MenuLightsPresetSO m_DefaultPreset;
        private static MenuLightsPresetSO m_LevelClearedPreset;
        private static MenuLightsPresetSO m_LevelFailedPreset;
        private static (float, ColorSO)[] m_DefaultPresetBackup;
        private static (float, ColorSO)[] m_LevelClearedPresetBackup;
        private static (float, ColorSO)[] m_LevelFailedPresetBackup;
        private static CustomColorSO m_Custom_Default;
        private static CustomColorSO m_Custom_Cleared;
        private static CustomColorSO m_Custom_Failed;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init and start reflection fetching
        /// </summary>
        public static void Init()
        {
            m_Custom_Default = CustomColorSO.CreateInstance<CustomColorSO>();
            m_Custom_Cleared = CustomColorSO.CreateInstance<CustomColorSO>();
            m_Custom_Failed  = CustomColorSO.CreateInstance<CustomColorSO>();
            SharedCoroutineStarter.instance.StartCoroutine(Coroutine_InitLate());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Apply current config
        /// </summary>
        public static void UpdateFromConfig()
        {
            if (!m_DefaultPreset)
                return;

            var l_Enabled = GTConfig.Instance.MainMenu.OverrideMenuEnvColors;

            if (!l_Enabled)
            {
                for (int l_I = 0; l_I < m_DefaultPresetBackup.Length; ++l_I)
                {
                    m_DefaultPreset.lightIdColorPairs[l_I].intensity = m_DefaultPresetBackup[l_I].Item1;
                    m_DefaultPreset.lightIdColorPairs[l_I].baseColor = m_DefaultPresetBackup[l_I].Item2;
                }

                if (m_LevelClearedPreset && m_LevelFailedPreset)
                {
                    for (int l_I = 0; l_I < m_LevelClearedPresetBackup.Length; ++l_I)
                    {
                        m_LevelClearedPreset.lightIdColorPairs[l_I].intensity = m_LevelClearedPresetBackup[l_I].Item1;
                        m_LevelClearedPreset.lightIdColorPairs[l_I].baseColor = m_LevelClearedPresetBackup[l_I].Item2;
                    }
                    for (int l_I = 0; l_I < m_LevelFailedPresetBackup.Length; ++l_I)
                    {
                        m_LevelFailedPreset.lightIdColorPairs[l_I].intensity = m_LevelFailedPresetBackup[l_I].Item1;
                        m_LevelFailedPreset.lightIdColorPairs[l_I].baseColor = m_LevelFailedPresetBackup[l_I].Item2;
                    }
                }
            }
            else
            {
                m_Custom_Default._color = GTConfig.Instance.MainMenu.BaseColor;
                m_Custom_Cleared._color = GTConfig.Instance.MainMenu.LevelClearedColor;
                m_Custom_Failed._color  = GTConfig.Instance.MainMenu.LevelFailedColor;

                for (int l_I = 0; l_I < m_DefaultPreset.lightIdColorPairs.Length; ++l_I)
                    m_DefaultPreset.lightIdColorPairs[l_I].baseColor = m_Custom_Default;

                if (m_LevelClearedPreset && m_LevelFailedPreset)
                {
                    for (int l_I = 0; l_I < m_LevelClearedPreset.lightIdColorPairs.Length; ++l_I)
                        m_LevelClearedPreset.lightIdColorPairs[l_I].baseColor = m_Custom_Cleared;

                    for (int l_I = 0; l_I < m_LevelFailedPreset.lightIdColorPairs.Length; ++l_I)
                        m_LevelFailedPreset.lightIdColorPairs[l_I].baseColor = m_Custom_Failed;
                }
            }

            try
            {
                if (m_MenuLightsManager && m_DefaultPreset)
                {
                    m_MenuLightsManager.SetField("_preset", m_DefaultPreset);
                    m_MenuLightsManager.enabled = true;
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Switch to base preset
        /// </summary>
        public static void SwitchToBase()
        {
            if (!GTConfig.Instance.MainMenu.OverrideMenuEnvColors)
                return;

            try
            {
	            if (m_MenuLightsManager && m_DefaultPreset)
                {
                    m_MenuLightsManager.SetField("_preset", m_DefaultPreset);
                    m_MenuLightsManager.enabled = true;
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Switch to level cleared preset
        /// </summary>
        public static void SwitchToLevelCleared()
        {
            if (!GTConfig.Instance.MainMenu.OverrideMenuEnvColors)
                return;

            try
            {
                if (m_MenuLightsManager && m_LevelClearedPreset)
                {
                    m_MenuLightsManager.SetField("_preset", m_LevelClearedPreset);
                    m_MenuLightsManager.enabled = true;
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Switch to level failed preset
        /// </summary>
        public static void SwitchToLevelFailed()
        {
            if (!GTConfig.Instance.MainMenu.OverrideMenuEnvColors)
                return;

            try
            {
                if (m_MenuLightsManager && m_LevelFailedPreset)
                {
                    m_MenuLightsManager.SetField("_preset", m_LevelFailedPreset);
                    m_MenuLightsManager.enabled = true;
                }
            }
            catch
            {

            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init coroutine that fetch object by reflection
        /// </summary>
        /// <returns></returns>
        private static IEnumerator Coroutine_InitLate()
        {
            yield return new WaitUntil(() => GameObject.FindObjectOfType<MenuLightsManager>());
            m_MenuLightsManager = GameObject.FindObjectOfType<MenuLightsManager>();

            m_DefaultPreset         = m_MenuLightsManager.GetField<MenuLightsPresetSO, MenuLightsManager>("_defaultPreset");
            m_DefaultPresetBackup   = m_DefaultPreset.lightIdColorPairs.Select(x => (x.intensity, x.baseColor)).ToArray();

            UpdateFromConfig();

            yield return new WaitUntil(() => GameObject.FindObjectOfType<SoloFreePlayFlowCoordinator>());
            var l_SoloFreePlayFlowCoordinator = GameObject.FindObjectOfType<SoloFreePlayFlowCoordinator>();

            m_LevelClearedPreset    = l_SoloFreePlayFlowCoordinator.GetField<MenuLightsPresetSO, SoloFreePlayFlowCoordinator>("_resultsClearedLightsPreset");
            m_LevelFailedPreset     = l_SoloFreePlayFlowCoordinator.GetField<MenuLightsPresetSO, SoloFreePlayFlowCoordinator>("_resultsFailedLightsPreset");

            m_LevelClearedPresetBackup  = m_LevelClearedPreset.lightIdColorPairs.Select(x => (x.intensity, x.baseColor)).ToArray();
            m_LevelFailedPresetBackup   = m_LevelFailedPreset.lightIdColorPairs.Select(x => (x.intensity, x.baseColor)).ToArray();

            UpdateFromConfig();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Preset replacer for PMenuLightsManager harmony patch
        /// </summary>
        /// <param name="preset">Input preset</param>
        /// <returns></returns>
        internal static MenuLightsPresetSO GetPresetForPatch(MenuLightsPresetSO preset)
        {
            if (!GTConfig.Instance.MainMenu.OverrideMenuEnvColors)
                return preset;

            if (m_LevelClearedPreset && m_LevelFailedPreset && (preset.name == m_LevelClearedPreset.name || preset.name == m_LevelFailedPreset.name))
                return preset;

            return m_DefaultPreset;
        }
    }
}
