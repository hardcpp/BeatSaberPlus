using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_SongChartVisualizer
{
    internal class SCVConfig : BeatSaberPlus.SDK.Config.JsonConfig<SCVConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        [JsonProperty] internal bool AlignWithFloor = true;
        [JsonProperty] internal bool ShowLockIcon = true;
        [JsonProperty] internal bool FollowEnvironementRotation = true;
        [JsonProperty] internal bool ShowNPSLegend = false;

        [JsonProperty] internal Color BackgroundColor   = new Color(0.00f, 0.00f, 0.00f, 0.50f);
        [JsonProperty] internal Color CursorColor       = new Color(1.00f, 0.03f, 0.00f, 1.00f);
        [JsonProperty] internal Color LineColor         = new Color(0.00f, 0.85f, 0.91f, 0.50f);
        [JsonProperty] internal Color LegendColor       = new Color(0.37f, 0.10f, 0.86f, 1.00f);
        [JsonProperty] internal Color DashLineColor     = new Color(0.37f, 0.10f, 0.86f, 1.00f);

        [JsonProperty] internal Vector3 ChartStandardPosition = new Vector3( 0f, -0.4f, 2.25f);
        [JsonProperty] internal Vector3 ChartStandardRotation = new Vector3(35f,  0.0f, 0.00f);

        [JsonProperty] internal Vector3 Chart360_90Position = new Vector3(  0f, 3.50f, 3.00f);
        [JsonProperty] internal Vector3 Chart360_90Rotation = new Vector3(-30f, 0.00f, 0.00f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/SongChartVisualizer/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (BeatSaberPlus.Config.SongChartVisualizer.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled                    = BeatSaberPlus.Config.SongChartVisualizer.Enabled;
            AlignWithFloor             = BeatSaberPlus.Config.SongChartVisualizer.AlignWithFloor;
            ShowLockIcon               = BeatSaberPlus.Config.SongChartVisualizer.ShowLockIcon;
            FollowEnvironementRotation = BeatSaberPlus.Config.SongChartVisualizer.FollowEnvironementRotation;
            ShowNPSLegend              = BeatSaberPlus.Config.SongChartVisualizer.ShowNPSLegend;

            BackgroundColor = BeatSaberPlus.Config.SongChartVisualizer.BackgroundColor;
            CursorColor     = BeatSaberPlus.Config.SongChartVisualizer.CursorColor;
            LineColor       = BeatSaberPlus.Config.SongChartVisualizer.LineColor;
            LegendColor     = BeatSaberPlus.Config.SongChartVisualizer.LegendColor;
            DashLineColor   = BeatSaberPlus.Config.SongChartVisualizer.DashLineColor;

            ChartStandardPosition = new Vector3(
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardPositionX,
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardPositionY,
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardPositionZ
                );
            ChartStandardRotation = new Vector3(
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardRotationX,
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardRotationY,
                BeatSaberPlus.Config.SongChartVisualizer.ChartStandardRotationZ
                );

            Chart360_90Position = new Vector3(
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90PositionX,
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90PositionY,
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90PositionZ
                );
            Chart360_90Rotation = new Vector3(
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90RotationX,
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90RotationY,
                BeatSaberPlus.Config.SongChartVisualizer.Chart360_90RotationZ
                );

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.SongChartVisualizer.OldConfigMigrated = true;
            Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset positions
        /// </summary>
        internal void ResetPosition()
        {
            ChartStandardPosition = new Vector3(0f, -0.4f, 2.25f);
            ChartStandardRotation = new Vector3(35f, 0.0f, 0.00f);

            Chart360_90Position = new Vector3(0f, 3.50f, 3.00f);
            Chart360_90Rotation = new Vector3(-30f, 0.00f, 0.00f);
        }
    }
}
