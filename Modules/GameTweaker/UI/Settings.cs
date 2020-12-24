using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus.Modules.GameTweaker.UI
{
    /// <summary>
    /// Saber Tweaker view controller
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("removedebris-toggle")]
        private ToggleSetting m_RemoveDebris;
        [UIComponent("removeallcutparticles-toggle")]
        private ToggleSetting m_RemoveAllCutParticles;
        [UIComponent("removeobstacleparticles-toggle")]
        private ToggleSetting m_RemoveAllObstacleParticles;
        [UIComponent("removefloorburnmarkparticles-toggle")]
        private ToggleSetting m_RemoveFloorBurnMarkParticles;
        [UIComponent("removefloorburnmarkeffects-toggle")]
        private ToggleSetting m_RemoveFloorBurnMarkEffects;
        [UIComponent("removesaberclasheffects-toggle")]
        private ToggleSetting m_RemoveSaberClashEffects;
        [UIComponent("removeworldparticles-toggle")]
        private ToggleSetting m_RemoveWorldParticles;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("removemusicbandlogo-toggle")]
        private ToggleSetting m_RemoveMusicBandLogo;
        [UIComponent("removefullcombolossanimation-toggle")]
        private ToggleSetting m_RemoveFullComboLossAnimation;
        [UIComponent("nofake360hud-toggle")]

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private ToggleSetting m_NoFake360HUD;
        [UIComponent("removetrail-toggle")]
        private ToggleSetting m_RemoveTrail;
        [UIComponent("intensity-increment")]
        private IncrementSetting m_Intensity;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay
            ////////////////////////////////////////////////////////////////////////////

            /// Particles / Effects
            SDK.UI.ToggleSetting.Setup(m_RemoveDebris,                  l_Event,                                           Config.GameTweaker.RemoveDebris,                 true);
            SDK.UI.ToggleSetting.Setup(m_RemoveAllCutParticles,         l_Event,                                           Config.GameTweaker.RemoveAllCutParticles,        true);
            SDK.UI.ToggleSetting.Setup(m_RemoveAllObstacleParticles,    l_Event,                                           Config.GameTweaker.RemoveObstacleParticles,      true);
            SDK.UI.ToggleSetting.Setup(m_RemoveFloorBurnMarkParticles,  l_Event,                                           Config.GameTweaker.RemoveSaberBurnMarkSparkles,  true);
            SDK.UI.ToggleSetting.Setup(m_RemoveFloorBurnMarkEffects,    l_Event,                                           Config.GameTweaker.RemoveSaberBurnMarks,         true);
            SDK.UI.ToggleSetting.Setup(m_RemoveSaberClashEffects,       l_Event,                                           Config.GameTweaker.RemoveSaberClashEffects,      true);
            SDK.UI.ToggleSetting.Setup(m_RemoveWorldParticles,          l_Event,                                           Config.GameTweaker.RemoveWorldParticles,         true);

            /// Environment
            SDK.UI.ToggleSetting.Setup(m_RemoveMusicBandLogo,           l_Event,                                           Config.GameTweaker.RemoveMusicBandLogo,          true);
            SDK.UI.ToggleSetting.Setup(m_RemoveFullComboLossAnimation,  l_Event,                                           Config.GameTweaker.RemoveFullComboLossAnimation, true);
            SDK.UI.ToggleSetting.Setup(m_NoFake360HUD,                  l_Event,                                           Config.GameTweaker.NoFake360HUD,                 true);

            /// Sabers
            SDK.UI.ToggleSetting.Setup(m_RemoveTrail,                   l_Event,                                           Config.GameTweaker.RemoveSaberSmoothingTrail,    true);
            SDK.UI.IncrementSetting.Setup(m_Intensity,                  l_Event, SDK.UI.BSMLSettingFormarter.Percentage,   Config.GameTweaker.SaberSmoothingTrailIntensity, true);
            m_Intensity.gameObject.SetActive(false);
            m_Intensity.interactable = !Config.GameTweaker.RemoveSaberSmoothingTrail;
            m_Intensity.gameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Particles / Effects
            Config.GameTweaker.RemoveDebris                     = m_RemoveDebris.Value;
            Config.GameTweaker.RemoveAllCutParticles            = m_RemoveAllCutParticles.Value;
            Config.GameTweaker.RemoveObstacleParticles          = m_RemoveAllObstacleParticles.Value;
            Config.GameTweaker.RemoveSaberBurnMarkSparkles      = m_RemoveFloorBurnMarkParticles.Value;
            Config.GameTweaker.RemoveSaberBurnMarks             = m_RemoveFloorBurnMarkEffects.Value;
            Config.GameTweaker.RemoveSaberClashEffects          = m_RemoveSaberClashEffects.Value;
            Config.GameTweaker.RemoveWorldParticles             = m_RemoveWorldParticles.Value;

            /// Environment
            Config.GameTweaker.RemoveMusicBandLogo              = m_RemoveMusicBandLogo.Value;
            Config.GameTweaker.RemoveFullComboLossAnimation     = m_RemoveFullComboLossAnimation.Value;
            Config.GameTweaker.NoFake360HUD                     = m_NoFake360HUD.Value;

            /// Sabers
            Config.GameTweaker.RemoveSaberSmoothingTrail        = m_RemoveTrail.Value;
            Config.GameTweaker.SaberSmoothingTrailIntensity     = m_Intensity.Value;
            m_Intensity.interactable = !Config.GameTweaker.RemoveSaberSmoothingTrail;

            /// Update patches
            GameTweaker.Instance.UpdatePatches(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Particles / Effects
            m_RemoveDebris.Value                    = Config.GameTweaker.RemoveDebris;
            m_RemoveAllCutParticles.Value           = Config.GameTweaker.RemoveAllCutParticles;
            m_RemoveAllObstacleParticles.Value      = Config.GameTweaker.RemoveObstacleParticles;
            m_RemoveFloorBurnMarkParticles.Value    = Config.GameTweaker.RemoveSaberBurnMarkSparkles;
            m_RemoveFloorBurnMarkEffects.Value      = Config.GameTweaker.RemoveSaberBurnMarks;
            m_RemoveSaberClashEffects.Value         = Config.GameTweaker.RemoveSaberClashEffects;
            m_RemoveWorldParticles.Value            = Config.GameTweaker.RemoveWorldParticles;

            /// Environment
            m_RemoveMusicBandLogo.Value             = Config.GameTweaker.RemoveMusicBandLogo;
            m_RemoveFullComboLossAnimation.Value    = Config.GameTweaker.RemoveFullComboLossAnimation;
            m_NoFake360HUD.Value                    = Config.GameTweaker.NoFake360HUD;

            /// Sabers
            m_RemoveTrail.Value                     = Config.GameTweaker.RemoveSaberSmoothingTrail;
            m_Intensity.Value                       = Config.GameTweaker.SaberSmoothingTrailIntensity;

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged(null);
        }
    }
}
