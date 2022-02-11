using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker
{
    internal class NTConfig : BeatSaberPlus.SDK.Config.JsonConfig<NTConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        [JsonProperty] internal bool ShowDotsWithArrow = true;
        [JsonProperty] internal bool OverrideArrowColors = false;
        [JsonProperty] internal bool OverrideDotColors = false;
        [JsonProperty] internal float Scale = 0.9f;

        [JsonProperty] internal float ArrowScale = 1.0f;
        [JsonProperty] internal float ArrowAlpha = 1.0f;
        [JsonProperty] internal Color ArrowLColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);
        [JsonProperty] internal Color ArrowRColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);

        [JsonProperty] internal float DotScale = 0.85f;
        [JsonProperty] internal float DotAlpha = 1.00f;
        [JsonProperty] internal Color DotLColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);
        [JsonProperty] internal Color DotRColor = new Color(0.12f, 0.75f, 1.00f, 1.00f);

        [JsonProperty] internal bool OverrideBombColor = false;
        [JsonProperty] internal Color BombColor = new Color(1.0000f, 0.0000f, 0.6469f, 1f);

        [JsonProperty] internal float PrecisionDotScale = 0.40f;

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
            if (BeatSaberPlus.Config.NoteTweaker.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled = BeatSaberPlus.Config.NoteTweaker.Enabled;

            ShowDotsWithArrow   = BeatSaberPlus.Config.NoteTweaker.ShowDotsWithArrow;
            OverrideArrowColors = BeatSaberPlus.Config.NoteTweaker.OverrideArrowColors;
            OverrideDotColors   = BeatSaberPlus.Config.NoteTweaker.OverrideDotColors;
            Scale               = BeatSaberPlus.Config.NoteTweaker.Scale;

            ArrowScale  = BeatSaberPlus.Config.NoteTweaker.ArrowScale;
            ArrowLColor = BeatSaberPlus.Config.NoteTweaker.ArrowLColor.ColorWithAlpha(BeatSaberPlus.Config.NoteTweaker.ArrowA);
            ArrowRColor = BeatSaberPlus.Config.NoteTweaker.ArrowRColor.ColorWithAlpha(BeatSaberPlus.Config.NoteTweaker.ArrowA);

            DotScale    = BeatSaberPlus.Config.NoteTweaker.DotScale;
            DotLColor   = BeatSaberPlus.Config.NoteTweaker.DotLColor.ColorWithAlpha(BeatSaberPlus.Config.NoteTweaker.DotA);
            DotRColor   = BeatSaberPlus.Config.NoteTweaker.DotRColor.ColorWithAlpha(BeatSaberPlus.Config.NoteTweaker.DotA);

            OverrideBombColor = BeatSaberPlus.Config.NoteTweaker.OverrideBombColor;
            BombColor         = BeatSaberPlus.Config.NoteTweaker.BombColor.ColorWithAlpha(1.00f);
            PrecisionDotScale = BeatSaberPlus.Config.NoteTweaker.PrecisionDotScale;

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.NoteTweaker.OldConfigMigrated = true;
            Save();
        }
    }
}
