using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker
{
    /// <summary>
    /// Note tweaker config
    /// </summary>
    internal class NTConfig : BeatSaberPlus.SDK.Config.JsonConfig<NTConfig>
    {
        private const string DEFAULT_PROFILE_NAME = "<i>============ Default ============</i>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [JsonProperty] internal bool Enabled = false;

        internal class _Profile
        {
            [JsonProperty] internal string Name = "New profile";

            [JsonProperty] internal float NotesScale = 0.9f;
            [JsonProperty] internal bool  NotesShowPrecisonDots = true;
            [JsonProperty] internal float NotesPrecisonDotsScale = 0.40f;

            [JsonProperty] internal float ArrowsScale = 1.0f;
            [JsonProperty] internal float ArrowsIntensity = 1.0f;
            [JsonProperty] internal bool  ArrowsOverrideColors = false;
            [JsonProperty, JsonConverter(typeof(BeatSaberPlus.SDK.Config.JsonConverters.ColorConverter))]
            internal Color ArrowsLColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);
            [JsonProperty, JsonConverter(typeof(BeatSaberPlus.SDK.Config.JsonConverters.ColorConverter))]
            internal Color ArrowsRColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);

            [JsonProperty] internal float DotsScale = 0.85f;
            [JsonProperty] internal float DotsIntensity = 1.0f;
            [JsonProperty] internal bool  DotsOverrideColors = false;
            [JsonProperty, JsonConverter(typeof(BeatSaberPlus.SDK.Config.JsonConverters.ColorConverter))]
            internal Color DotsLColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);
            [JsonProperty, JsonConverter(typeof(BeatSaberPlus.SDK.Config.JsonConverters.ColorConverter))]
            internal Color DotsRColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);

            [JsonProperty] internal float BombsScale = 1f;
            [JsonProperty] internal bool  BombsOverrideColor = false;
            [JsonProperty, JsonConverter(typeof(BeatSaberPlus.SDK.Config.JsonConverters.ColorConverter))]
            internal Color BombsColor = new Color(1.0000f, 0.0000f, 0.6469f, 1f);

            [JsonProperty] internal float ArcsIntensity = 1.00f;
            [JsonProperty] internal bool  ArcsHaptics = true;

            [JsonProperty] internal float BurstNotesDotsScale = 1.5f;

            internal static _Profile AsDefault()
            {
                return new _Profile()
                {
                    Name                    = DEFAULT_PROFILE_NAME,
                    NotesScale              = 1f,
                    NotesShowPrecisonDots   = false,
                    DotsScale               = 1f,
                    BombsColor              = new Color(0.251f, 0.251f, 0.251f, 1.000f)
                };
            }

            internal bool IsDefault()
                => Name == DEFAULT_PROFILE_NAME;

        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal List<_Profile> Profiles = new List<_Profile>();

        [JsonProperty] internal int ActiveProfile = 1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get active profile
        /// </summary>
        /// <returns></returns>
        internal _Profile GetActiveProfile()
        {
            if (Profiles == null || Profiles.Count == 0)
            {
                Profiles = new List<_Profile>();
                Profiles.Add(_Profile.AsDefault());
                Profiles.Add(new _Profile());
                Save();
            }

            if (ActiveProfile < 0 || ActiveProfile >= Profiles.Count)
            {
                ActiveProfile %= Profiles.Count;
                Save();
            }

            return Profiles[ActiveProfile];
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/NoteTweaker/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (!p_OnCreation && m_RawLoaded != null && (Profiles == null || Profiles.Count == 0))
            {
                var l_Import = new _Profile();
                l_Import.Name               = "New config";
                l_Import.NotesScale         = m_RawLoaded["Scale"]?.Value<float>()                  ?? l_Import.NotesScale;
                l_Import.NotesShowPrecisonDots   = m_RawLoaded["ShowDotsWithArrow"]?.Value<bool>()  ?? l_Import.NotesShowPrecisonDots;
                l_Import.NotesPrecisonDotsScale  = m_RawLoaded["PrecisionDotScale"]?.Value<float>() ?? l_Import.NotesPrecisonDotsScale;

                l_Import.ArrowsScale            = m_RawLoaded["ArrowScale"]?.Value<float>()         ?? l_Import.ArrowsScale;
                l_Import.ArrowsOverrideColors   = m_RawLoaded["OverrideArrowColors"]?.Value<bool>() ?? l_Import.ArrowsOverrideColors;
                l_Import.ArrowsIntensity        = m_RawLoaded["ArrowAlpha"]?.Value<float>()         ?? l_Import.ArrowsIntensity;
                l_Import.ArrowsLColor = new Color(
                    m_RawLoaded["ArrowLColor"]?["r"].Value<float>() ?? l_Import.ArrowsLColor.r,
                    m_RawLoaded["ArrowLColor"]?["g"].Value<float>() ?? l_Import.ArrowsLColor.g,
                    m_RawLoaded["ArrowLColor"]?["b"].Value<float>() ?? l_Import.ArrowsLColor.b,
                    1f
                );
                l_Import.ArrowsRColor = new Color(
                    m_RawLoaded["ArrowRColor"]?["r"].Value<float>() ?? l_Import.ArrowsRColor.r,
                    m_RawLoaded["ArrowRColor"]?["g"].Value<float>() ?? l_Import.ArrowsRColor.g,
                    m_RawLoaded["ArrowRColor"]?["b"].Value<float>() ?? l_Import.ArrowsRColor.b,
                    1f
                );

                l_Import.DotsScale            = m_RawLoaded["DotScale"]?.Value<float>()         ?? l_Import.DotsScale;
                l_Import.DotsOverrideColors   = m_RawLoaded["OverrideDotColors"]?.Value<bool>() ?? l_Import.DotsOverrideColors;
                l_Import.DotsIntensity        = m_RawLoaded["DotAlpha"]?.Value<float>()         ?? l_Import.DotsIntensity;
                l_Import.DotsLColor = new Color(
                    m_RawLoaded["DotLColor"]?["r"].Value<float>() ?? l_Import.DotsLColor.r,
                    m_RawLoaded["DotLColor"]?["g"].Value<float>() ?? l_Import.DotsLColor.g,
                    m_RawLoaded["DotLColor"]?["b"].Value<float>() ?? l_Import.DotsLColor.b,
                    1f
                );
                l_Import.DotsRColor = new Color(
                    m_RawLoaded["DotRColor"]?["r"].Value<float>() ?? l_Import.DotsRColor.r,
                    m_RawLoaded["DotRColor"]?["g"].Value<float>() ?? l_Import.DotsRColor.g,
                    m_RawLoaded["DotRColor"]?["b"].Value<float>() ?? l_Import.DotsRColor.b,
                    1f
                );

                l_Import.BombsScale         = m_RawLoaded["BombScale"]?.Value<float>()          ?? l_Import.BombsScale;
                l_Import.BombsOverrideColor = m_RawLoaded["OverrideBombColor"]?.Value<bool>()   ?? l_Import.BombsOverrideColor;
                l_Import.BombsColor = new Color(
                    m_RawLoaded["BombColor"]?["r"].Value<float>() ?? l_Import.BombsColor.r,
                    m_RawLoaded["BombColor"]?["g"].Value<float>() ?? l_Import.BombsColor.g,
                    m_RawLoaded["BombColor"]?["b"].Value<float>() ?? l_Import.BombsColor.b,
                    1f
                );

                Profiles.Add(l_Import);
            }

            if (Profiles == null || Profiles.Count == 0)
            {
                Profiles = new List<_Profile>();
                Profiles.Add(_Profile.AsDefault());
                Profiles.Add(new _Profile());
            }

            Profiles.RemoveAll(x => x.IsDefault());
            Profiles.Insert(0, _Profile.AsDefault());

            Save();
        }
    }
}
