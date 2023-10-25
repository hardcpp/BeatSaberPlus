using Newtonsoft.Json;
using UnityEngine;

namespace ChatPlexMod_SongChartVisualizer
{
    /// <summary>
    /// Config class
    /// </summary>
    internal class SCVConfig : CP_SDK.Config.JsonConfig<SCVConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        [JsonProperty] internal bool AlignWithFloor             = true;
        [JsonProperty] internal bool ShowLockIcon               = true;
        [JsonProperty] internal bool FollowEnvironementRotation = true;
        [JsonProperty] internal bool ShowNPSLegend              = false;

        [JsonProperty] internal Color BackgroundColor   = new Color(0.00f, 0.00f, 0.00f, 0.50f);
        [JsonProperty] internal Color CursorColor       = new Color(1.00f, 0.03f, 0.00f, 1.00f);
        [JsonProperty] internal Color LineColor         = new Color(0.00f, 0.85f, 0.91f, 0.50f);
        [JsonProperty] internal Color LegendColor       = new Color(0.37f, 0.10f, 0.86f, 1.00f);
        [JsonProperty] internal Color DashLineColor     = new Color(0.37f, 0.10f, 0.86f, 0.20f);

        [JsonProperty] internal Vector3 ChartStandardPosition = new Vector3(  0.0f, -0.4f, 2.25f);
        [JsonProperty] internal Vector3 ChartStandardRotation = new Vector3( 35.0f,  0.0f, 0.00f);

        [JsonProperty] internal Vector3 ChartRotatingPosition = new Vector3(  0.0f, 3.50f, 3.00f);
        [JsonProperty] internal Vector3 ChartRotatingRotation = new Vector3(-30.0f, 0.00f, 0.00f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/SongChartVisualizer/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {

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

            ChartRotatingPosition = new Vector3(0f, 3.50f, 3.00f);
            ChartRotatingRotation = new Vector3(-30f, 0.00f, 0.00f);
        }
    }
}
