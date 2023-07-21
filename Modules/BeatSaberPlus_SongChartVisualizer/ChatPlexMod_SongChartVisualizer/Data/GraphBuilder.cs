namespace ChatPlexMod_SongChartVisualizer.Data
{
    /// <summary>
    /// Graph builder utils
    /// </summary>
    internal static class GraphBuilder
    {
        private static Graph m_SampleGraph = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER
        internal static Graph BuildNPSGraph(IReadonlyBeatmapData p_TransformedBeatmapData, float p_SongDuration)
#else
#error Missing game implementation
#endif
        {
            var l_Graph         = new Graph(p_SongDuration);
            var l_NPSSRaw       = CP_SDK.Pool.ListPool<float>.Get();
            var l_DataPoints    = CP_SDK.Pool.ListPool<GraphPoint>.Get();

            try
            {
                l_NPSSRaw.Clear();
                l_DataPoints.Clear();

                if (l_NPSSRaw.Capacity < (int)(p_SongDuration + 1))         l_NPSSRaw.Capacity          = (int)(p_SongDuration + 1);
                if (l_DataPoints.Capacity < SongChartVisualizer.MaxPoints)  l_DataPoints.Capacity       = SongChartVisualizer.MaxPoints;

                for (var l_I = 0; l_I < (int)(p_SongDuration + 1f); ++l_I)
                    l_NPSSRaw.Add(0.0f);

#if BEATSABER
                var l_Iterator = p_TransformedBeatmapData.allBeatmapDataItems.GetEnumerator();
                while (l_Iterator.MoveNext())
                {
                    var l_Object = l_Iterator.Current;
                    if (l_Object.type != BeatmapDataItem.BeatmapDataItemType.BeatmapObject
                        || !(l_Object is NoteData l_NoteData)
                        || l_NoteData.colorType == ColorType.None)
                        continue;

                    l_NPSSRaw[(int)l_NoteData.time]++;
                }
                l_Iterator.Dispose();
#else
#error Missing game implementation
#endif

                var l_ResamplePoint     = ((double)l_NPSSRaw.Count / (double)SongChartVisualizer.MaxPoints);
                var l_ResampleMaxIndex  = l_NPSSRaw.Count - 1;
                for (var l_I = 0; l_I < SongChartVisualizer.MaxPoints; ++l_I)
                {
                    var l_VPoint        = (double)l_I * l_ResamplePoint;
                    var l_FirstIndex    = (int)l_VPoint;
                    var l_SecondIndex   = l_FirstIndex >= l_ResampleMaxIndex ? l_FirstIndex : (l_FirstIndex + 1);
                    var l_DeltaT        = l_VPoint - l_FirstIndex;
                    var l_Value         = (float)(l_NPSSRaw[l_FirstIndex] * (1 - l_DeltaT) + l_NPSSRaw[l_SecondIndex] * l_DeltaT);

                    l_Graph.MinY = System.Math.Min(l_Graph.MinY, l_Value);
                    l_Graph.MaxY = System.Math.Max(l_Graph.MaxY, l_Value);

                    l_DataPoints.Add(new GraphPoint()
                    {
                        X = l_I,
                        Y = l_Value
                    });
                }

                l_Graph.Points = l_DataPoints.ToArray();
            }
            finally
            {
                l_NPSSRaw.Clear();
                l_DataPoints.Clear();

                CP_SDK.Pool.ListPool<float>.Release(l_NPSSRaw);
                CP_SDK.Pool.ListPool<GraphPoint>.Release(l_DataPoints);
            }
#if FALSE
            var l_Builder = new System.Text.StringBuilder();
            l_Builder.AppendLine("var l_Graph = new Graph(" + p_SongDuration.ToString("0.00").Replace(',', '.') + "f)");
            l_Builder.AppendLine("{");
            l_Builder.AppendLine("    MinY   = " + l_Graph.MinY.ToString("0.00").Replace(',', '.') + "f,");
            l_Builder.AppendLine("    MaxY   = " + l_Graph.MaxY.ToString("0.00").Replace(',', '.') + "f,");
            l_Builder.AppendLine("    Points = new GraphPoint[]");
            l_Builder.AppendLine("    {");

            for (var l_I = 0; l_I < l_Graph.Points.Length; ++l_I)
                l_Builder.AppendLine("        new GraphPoint { X = " + l_Graph.Points[l_I].X.ToString("0.00").Replace(',', '.') + "f, Y = " + l_Graph.Points[l_I].Y.ToString("0.00").Replace(',', '.') + "f }" + ((l_I < (l_Graph.Points.Length - 1)) ? "," : ""));

            l_Builder.AppendLine("    }");
            l_Builder.AppendLine("};");

            System.IO.File.WriteAllText("graph.cs", l_Builder.ToString());
#endif

            return l_Graph;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get sample NPS graph
        /// </summary>
        /// <returns></returns>
        internal static Graph GetSampleNPSGraph()
        {
            if (m_SampleGraph != null)
                return m_SampleGraph;

            m_SampleGraph = new Graph(225.50f)
            {
                MinY   = 0.00f,
                MaxY   = 13.60f,
                Points = new GraphPoint[]
                {
                    new GraphPoint { X =  0.00f, Y =  0.00f },
                    new GraphPoint { X =  1.00f, Y =  3.48f },
                    new GraphPoint { X =  2.00f, Y =  4.44f },
                    new GraphPoint { X =  3.00f, Y =  4.00f },
                    new GraphPoint { X =  4.00f, Y =  4.92f },
                    new GraphPoint { X =  5.00f, Y =  2.60f },
                    new GraphPoint { X =  6.00f, Y =  4.12f },
                    new GraphPoint { X =  7.00f, Y =  3.82f },
                    new GraphPoint { X =  8.00f, Y =  5.76f },
                    new GraphPoint { X =  9.00f, Y =  3.34f },
                    new GraphPoint { X = 10.00f, Y =  3.80f },
                    new GraphPoint { X = 11.00f, Y =  3.28f },
                    new GraphPoint { X = 12.00f, Y =  5.00f },
                    new GraphPoint { X = 13.00f, Y =  3.76f },
                    new GraphPoint { X = 14.00f, Y =  4.28f },
                    new GraphPoint { X = 15.00f, Y =  5.80f },
                    new GraphPoint { X = 16.00f, Y =  6.04f },
                    new GraphPoint { X = 17.00f, Y =  6.42f },
                    new GraphPoint { X = 18.00f, Y =  5.28f },
                    new GraphPoint { X = 19.00f, Y =  4.82f },
                    new GraphPoint { X = 20.00f, Y =  5.60f },
                    new GraphPoint { X = 21.00f, Y =  4.76f },
                    new GraphPoint { X = 22.00f, Y =  6.72f },
                    new GraphPoint { X = 23.00f, Y =  2.04f },
                    new GraphPoint { X = 24.00f, Y =  4.72f },
                    new GraphPoint { X = 25.00f, Y =  6.00f },
                    new GraphPoint { X = 26.00f, Y = 11.56f },
                    new GraphPoint { X = 27.00f, Y = 12.88f },
                    new GraphPoint { X = 28.00f, Y =  3.00f },
                    new GraphPoint { X = 29.00f, Y =  8.08f },
                    new GraphPoint { X = 30.00f, Y = 12.40f },
                    new GraphPoint { X = 31.00f, Y = 10.24f },
                    new GraphPoint { X = 32.00f, Y =  5.72f },
                    new GraphPoint { X = 33.00f, Y =  0.58f },
                    new GraphPoint { X = 34.00f, Y =  5.52f },
                    new GraphPoint { X = 35.00f, Y =  2.40f },
                    new GraphPoint { X = 36.00f, Y =  8.00f },
                    new GraphPoint { X = 37.00f, Y =  2.62f },
                    new GraphPoint { X = 38.00f, Y = 10.00f },
                    new GraphPoint { X = 39.00f, Y =  8.86f },
                    new GraphPoint { X = 40.00f, Y =  8.80f },
                    new GraphPoint { X = 41.00f, Y =  9.34f },
                    new GraphPoint { X = 42.00f, Y = 11.00f },
                    new GraphPoint { X = 43.00f, Y =  0.00f },
                    new GraphPoint { X = 44.00f, Y =  0.00f },
                    new GraphPoint { X = 45.00f, Y =  4.20f },
                    new GraphPoint { X = 46.00f, Y =  5.04f },
                    new GraphPoint { X = 47.00f, Y =  5.10f },
                    new GraphPoint { X = 48.00f, Y =  5.00f },
                    new GraphPoint { X = 49.00f, Y =  6.96f },
                    new GraphPoint { X = 50.00f, Y =  8.00f },
                    new GraphPoint { X = 51.00f, Y =  5.78f },
                    new GraphPoint { X = 52.00f, Y =  6.00f },
                    new GraphPoint { X = 53.00f, Y =  0.44f },
                    new GraphPoint { X = 54.00f, Y =  0.00f },
                    new GraphPoint { X = 55.00f, Y =  2.60f },
                    new GraphPoint { X = 56.00f, Y =  3.00f },
                    new GraphPoint { X = 57.00f, Y =  5.18f },
                    new GraphPoint { X = 58.00f, Y = 10.12f },
                    new GraphPoint { X = 59.00f, Y =  3.00f },
                    new GraphPoint { X = 60.00f, Y =  0.00f },
                    new GraphPoint { X = 61.00f, Y =  0.86f },
                    new GraphPoint { X = 62.00f, Y =  3.88f },
                    new GraphPoint { X = 63.00f, Y =  3.38f },
                    new GraphPoint { X = 64.00f, Y =  3.00f },
                    new GraphPoint { X = 65.00f, Y =  3.10f },
                    new GraphPoint { X = 66.00f, Y = 10.36f },
                    new GraphPoint { X = 67.00f, Y = 10.42f },
                    new GraphPoint { X = 68.00f, Y = 11.04f },
                    new GraphPoint { X = 69.00f, Y =  7.12f },
                    new GraphPoint { X = 70.00f, Y =  8.00f },
                    new GraphPoint { X = 71.00f, Y =  9.92f },
                    new GraphPoint { X = 72.00f, Y =  1.84f },
                    new GraphPoint { X = 73.00f, Y =  1.02f },
                    new GraphPoint { X = 74.00f, Y =  5.44f },
                    new GraphPoint { X = 75.00f, Y =  6.00f },
                    new GraphPoint { X = 76.00f, Y =  6.00f },
                    new GraphPoint { X = 77.00f, Y =  5.10f },
                    new GraphPoint { X = 78.00f, Y =  8.72f },
                    new GraphPoint { X = 79.00f, Y =  8.92f },
                    new GraphPoint { X = 80.00f, Y =  7.40f },
                    new GraphPoint { X = 81.00f, Y = 12.64f },
                    new GraphPoint { X = 82.00f, Y =  9.04f },
                    new GraphPoint { X = 83.00f, Y =  7.90f },
                    new GraphPoint { X = 84.00f, Y = 13.36f },
                    new GraphPoint { X = 85.00f, Y = 13.00f },
                    new GraphPoint { X = 86.00f, Y =  8.32f },
                    new GraphPoint { X = 87.00f, Y =  0.00f },
                    new GraphPoint { X = 88.00f, Y =  0.00f },
                    new GraphPoint { X = 89.00f, Y = 13.00f },
                    new GraphPoint { X = 90.00f, Y = 13.60f },
                    new GraphPoint { X = 91.00f, Y = 12.34f },
                    new GraphPoint { X = 92.00f, Y = 12.92f },
                    new GraphPoint { X = 93.00f, Y = 13.00f },
                    new GraphPoint { X = 94.00f, Y = 11.80f },
                    new GraphPoint { X = 95.00f, Y = 11.90f },
                    new GraphPoint { X = 96.00f, Y = 13.00f },
                    new GraphPoint { X = 97.00f, Y = 13.00f },
                    new GraphPoint { X = 98.00f, Y =  4.68f },
                    new GraphPoint { X = 99.00f, Y =  0.00f }
                }
            };

            return m_SampleGraph;
        }


        /*
                                     if (l_Notes.Count > 0)
                            {
                                var maximum_tolerance           = .06 + 1e-9;// # Magic number based on maximum tolerated swing speed
                                var maximum_window_tolerance    = .07 + 1e-9; //# For windowed sliders

                                NoteData l_LastRed = null;
                                NoteData l_LastBlue = null;

                                Dictionary<int, int> l_SPSR = new Dictionary<int, int>();
                                Dictionary<int, int> l_SPSB = new Dictionary<int, int>();
                                for (int l_I = 0; l_I < (l_SongDuration / l_PreviewBeatmapLevel.beatsPerMinute * 60f) + 1; ++l_I)
                                {
                                    l_SPSR.Add(l_I, 0);
                                    l_SPSB.Add(l_I, 0);
                                }

                                foreach (var l_Note in l_Notes)
                                {
                                    var real_time = l_Note.time / l_PreviewBeatmapLevel.beatsPerMinute * 60f;

                                    if (l_Note.colorType == ColorType.ColorA)
                                    {
                                        if (l_LastRed != null)
                                        {
                                            bool l_IsWindow = Mathf.Max(Mathf.Abs(l_Note.lineIndex - l_LastRed.lineIndex), Mathf.Abs(l_Note.noteLineLayer - l_LastRed.noteLineLayer)) >= 2;
                                            if (l_IsWindow && (((l_Note.time - l_LastRed.time) / l_PreviewBeatmapLevel.beatsPerMinute * 60f) > maximum_window_tolerance)
                                                || (((l_Note.time - l_LastRed.time) / l_PreviewBeatmapLevel.beatsPerMinute * 60f) > maximum_tolerance)
                                                )
                                                l_SPSR[(int)Mathf.Floor(real_time)]++;
                                        }
                                        else
                                            l_SPSR[(int)Mathf.Floor(real_time)]++;

                                        l_LastRed = l_Note;
                                    }
                                    else if (l_Note.colorType == ColorType.ColorB)
                                    {

                                        if (l_LastBlue != null)
                                        {
                                            bool l_IsWindow = Mathf.Max(Mathf.Abs(l_Note.lineIndex - l_LastBlue.lineIndex), Mathf.Abs(l_Note.noteLineLayer - l_LastBlue.noteLineLayer)) >= 2;
                                            if (l_IsWindow && (((l_Note.time - l_LastBlue.time) / l_PreviewBeatmapLevel.beatsPerMinute * 60f) > maximum_window_tolerance)
                                                || (((l_Note.time - l_LastBlue.time) / l_PreviewBeatmapLevel.beatsPerMinute * 60f) > maximum_tolerance)
                                                )
                                                l_SPSB[(int)Mathf.Floor(real_time)]++;
                                        }
                                        else
                                            l_SPSB[(int)Mathf.Floor(real_time)]++;

                                        l_LastBlue = l_Note;
                                    }
                                }

                                var swing_count_list = new Dictionary<int, int>(l_SPSR);
                                foreach (var l_KVP in l_SPSB)
                                {
                                    if (!swing_count_list.ContainsKey(l_KVP.Key))
                                        swing_count_list.Add(l_KVP.Key, l_KVP.Value);
                                    else
                                        swing_count_list[l_KVP.Key] += l_KVP.Value;
                                }

                                for (int l_I = 0; l_I < swing_count_list.Count; ++l_I)
                                {
                                    float l_StartTime = (l_I / 60f) * l_PreviewBeatmapLevel.beatsPerMinute;
                                    float l_SectionEndTime = (((l_I == swing_count_list.Count - 1) ? (l_SongDuration / l_PreviewBeatmapLevel.beatsPerMinute * 60f) : l_I + 1) / 60f) * l_PreviewBeatmapLevel.beatsPerMinute;
                                    l_GraphData.Add((swing_count_list[l_I], l_StartTime, l_SectionEndTime));
                                }


                                // float l_PrevSPS         = 0f;
                                // float l_Threshold       = 0.1f;
                                // float l_SectionStart    = 0f;
                                // for (int l_I = 0; l_I < swing_count_list.Max(x => x.Key); ++l_I)
                                // {
                                //     float l_CurrentSPS = ((float)swing_count_list[l_I]) / ((float)l_I - l_SectionStart);
                                //
                                //     if (l_I == (swing_count_list.Count - 1)
                                //         || (l_GraphData.Count == 0 && l_CurrentSPS != 0f)
                                //         || (((float)l_I) - l_SectionStart > 1f && Mathf.Abs(l_CurrentSPS - l_PrevSPS) > (l_PrevSPS * l_Threshold)))
                                //     {
                                //         var l_SectionSPS = swing_count_list.Where(x => x.Key >= l_SectionStart && x.Key < (l_I + 1)).Sum(x => x.Value) / (l_I - l_SectionStart);
                                //         l_PrevSPS = l_SectionSPS;
                                //
                                //         l_GraphData.Add((l_SectionSPS, (l_SectionStart / 60f) * l_DifficultyBeatmap.level.beatsPerMinute, (((float)l_I) / 60f) * l_DifficultyBeatmap.level.beatsPerMinute));
                                //         l_SectionStart = (float)l_I;
                                //     }
                                // }

                                swing_count_list.ToList().ForEach(x => Logger.Instance.Debug(string.Format("{0} - {1}", x.Key, x.Value)));

        */

    }
}
