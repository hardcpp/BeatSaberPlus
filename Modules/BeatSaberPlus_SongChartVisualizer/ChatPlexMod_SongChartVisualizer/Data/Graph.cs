using UnityEngine;

namespace ChatPlexMod_SongChartVisualizer.Data
{
    /// <summary>
    /// Graph instance
    /// </summary>
    internal class Graph
    {
        internal GraphPoint[] Points;
        internal float SongDuration;
        internal float MinY;
        internal float MaxY;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_SongDuration">Song duration</param>
        internal Graph(float p_SongDuration)
        {
            SongDuration    = p_SongDuration;
            MinY            = 0.0f;
            MaxY            = 0.0f;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal float SongTimeToProgress(float p_SongTime)
            => (p_SongTime / (SongDuration + 1));
        internal int ProgressToIndexLow(float p_Progress)
        {
            var l_Index = (int)(p_Progress * (float)SongChartVisualizer.MaxPoints);
            return Mathf.Clamp(l_Index, 0, SongChartVisualizer.MaxPoints - 1);
        }
        internal float ProgressToIndexRelativeVirtual(float p_Progress)
        {
            var l_RelativeVirtual = p_Progress * (float)SongChartVisualizer.MaxPoints;
            return (float)l_RelativeVirtual - (int)l_RelativeVirtual;
        }
        internal int ProgressToIndexHi(float p_Progress)
        {
            var l_Index = (int)(p_Progress * (float)SongChartVisualizer.MaxPoints) + 1;
            return Mathf.Clamp(l_Index, 0, SongChartVisualizer.MaxPoints - 1);
        }
    }
}
