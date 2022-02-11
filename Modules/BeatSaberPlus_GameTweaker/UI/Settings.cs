using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus_GameTweaker.UI
{
    /// <summary>
    /// Saber Tweaker view controller
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
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
        private ToggleSetting m_NoFake360HUD;
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
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveDebris,                  l_Event,                                           GTConfig.Instance.RemoveDebris,                 true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveAllCutParticles,         l_Event,                                           GTConfig.Instance.RemoveAllCutParticles,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveAllObstacleParticles,    l_Event,                                           GTConfig.Instance.RemoveObstacleParticles,      true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveFloorBurnMarkParticles,  l_Event,                                           GTConfig.Instance.RemoveSaberBurnMarkSparkles,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveFloorBurnMarkEffects,    l_Event,                                           GTConfig.Instance.RemoveSaberBurnMarks,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveSaberClashEffects,       l_Event,                                           GTConfig.Instance.RemoveSaberClashEffects,      true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveWorldParticles,          l_Event,                                           GTConfig.Instance.RemoveWorldParticles,         true);

            /// Environment
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveMusicBandLogo,           l_Event,                                           GTConfig.Instance.RemoveMusicBandLogo,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveFullComboLossAnimation,  l_Event,                                           GTConfig.Instance.RemoveFullComboLossAnimation, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NoFake360HUD,                  l_Event,                                           GTConfig.Instance.NoFake360HUD,                 true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            GTConfig.Instance.Save();
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
            GTConfig.Instance.RemoveDebris                     = m_RemoveDebris.Value;
            GTConfig.Instance.RemoveAllCutParticles            = m_RemoveAllCutParticles.Value;
            GTConfig.Instance.RemoveObstacleParticles          = m_RemoveAllObstacleParticles.Value;
            GTConfig.Instance.RemoveSaberBurnMarkSparkles      = m_RemoveFloorBurnMarkParticles.Value;
            GTConfig.Instance.RemoveSaberBurnMarks             = m_RemoveFloorBurnMarkEffects.Value;
            GTConfig.Instance.RemoveSaberClashEffects          = m_RemoveSaberClashEffects.Value;
            GTConfig.Instance.RemoveWorldParticles             = m_RemoveWorldParticles.Value;

            /// Environment
            GTConfig.Instance.RemoveMusicBandLogo              = m_RemoveMusicBandLogo.Value;
            GTConfig.Instance.RemoveFullComboLossAnimation     = m_RemoveFullComboLossAnimation.Value;
            GTConfig.Instance.NoFake360HUD                     = m_NoFake360HUD.Value;

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
            m_RemoveDebris.Value                    = GTConfig.Instance.RemoveDebris;
            m_RemoveAllCutParticles.Value           = GTConfig.Instance.RemoveAllCutParticles;
            m_RemoveAllObstacleParticles.Value      = GTConfig.Instance.RemoveObstacleParticles;
            m_RemoveFloorBurnMarkParticles.Value    = GTConfig.Instance.RemoveSaberBurnMarkSparkles;
            m_RemoveFloorBurnMarkEffects.Value      = GTConfig.Instance.RemoveSaberBurnMarks;
            m_RemoveSaberClashEffects.Value         = GTConfig.Instance.RemoveSaberClashEffects;
            m_RemoveWorldParticles.Value            = GTConfig.Instance.RemoveWorldParticles;

            /// Environment
            m_RemoveMusicBandLogo.Value             = GTConfig.Instance.RemoveMusicBandLogo;
            m_RemoveFullComboLossAnimation.Value    = GTConfig.Instance.RemoveFullComboLossAnimation;
            m_NoFake360HUD.Value                    = GTConfig.Instance.NoFake360HUD;

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged(null);
        }
    }
}
