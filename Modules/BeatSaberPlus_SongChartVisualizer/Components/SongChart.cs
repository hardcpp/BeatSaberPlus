using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_SongChartVisualizer.Components
{
    /// <summary>
    /// Chart creator component
    /// </summary>
    internal class SongChart : MonoBehaviour
    {
        /// <summary>
        /// Audio time sync controller
        /// </summary>
        private AudioTimeSyncController m_AudioTimeSyncController;
        /// <summary>
        /// FlyingGameHUDRotation instance
        /// </summary>
        private GameObject m_FlyingGameHUDRotation = null;
        /// <summary>
        /// NPS sections
        /// </summary>
        private (float, float, float)[] m_GraphData = new (float, float, float)[] { };
        /// <summary>
        /// Current section index
        /// </summary>
        private int m_CurrentGraphDataIndex;
        /// <summary>
        /// Graph canvas
        /// </summary>
        private RectTransform m_GraphCanvas = null;
        /// <summary>
        /// Graph points
        /// </summary>
        private List<Vector2> m_Points = new List<Vector2>();
        /// <summary>
        /// Pointer cursor
        /// </summary>
        private GameObject m_Pointer = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component first frame
        /// </summary>
        private void Start()
        {
            m_AudioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();

            bool l_NoUI = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.noTextsAndHuds ?? false;

            if (l_NoUI)
            {
                SongChartVisualizer.Instance.DestroyChart();
                return;
            }

            var l_TransformedBeatmapData    = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.transformedBeatmapData;
            var l_PreviewBeatmapLevel       = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.previewBeatmapLevel;
            var l_DifficultyBeatmap         = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.difficultyBeatmap;

            if ((l_TransformedBeatmapData != null && l_PreviewBeatmapLevel != null && l_DifficultyBeatmap != null) || BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
            {
                var l_SongDuration = -1f;

                /// Demo data
                if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                    l_SongDuration = 201f;
                else
                    l_SongDuration = l_DifficultyBeatmap?.level?.beatmapLevelData?.audioClip?.length ?? -1f;

                if (l_SongDuration >= 0)
                {
                    if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                        FillGraphDataWithDemoValues();
                    else if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                    {
                        m_FlyingGameHUDRotation = l_TransformedBeatmapData.spawnRotationEventsCount > 0 ? Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault()?.gameObject : null;

                        var l_Notes = l_TransformedBeatmapData.allBeatmapDataItems
                                    .Where(x => x.type == BeatmapDataItem.BeatmapDataItemType.BeatmapObject && x is NoteData)
                                    .Select(x => x as NoteData)
                                    .Where(x => x.colorType != ColorType.None)
                                    .OrderBy(x => x.time)
                                    .ToList();
                        var l_GraphData = new List<(float, float, float)>();

                        if (true)
                        {
                            if (l_Notes.Count > 0)
                            {
                                List<float> l_NPSS = new List<float>();
                                for (int l_I = 0; l_I < (l_SongDuration + 1f); ++l_I)
                                {
                                    var l_NoteCount = l_Notes.Count(x => x.time >= l_I && x.time < (l_I + 1f));
                                    l_NPSS.Add(l_NoteCount == 0 ? 0f : l_NoteCount);
                                }

                                float l_PrevNPS      = 0f;
                                float l_Threshold    = 0.1f;
                                float l_SectionStart = 0f;
                                for (int l_I = 0; l_I < l_NPSS.Count; ++l_I)
                                {
                                    float l_CurrentNPS = l_NPSS[l_I];

                                    if (l_I == (l_NPSS.Count - 1)
                                        || (l_GraphData.Count == 0 && l_CurrentNPS != 0f)
                                        || (((float)l_I) - l_SectionStart > 1f && Mathf.Abs(l_CurrentNPS - l_PrevNPS) > (l_PrevNPS * l_Threshold)))
                                    {
                                        var l_NoteCount  = l_Notes.Count(x => x.time >= l_SectionStart && x.time < (float)l_I);
                                        var l_SectionNPS = l_NoteCount == 0 ? 0f : l_NoteCount / (((float)l_I) - l_SectionStart);
                                        l_PrevNPS = l_SectionNPS;

                                        l_GraphData.Add((l_SectionNPS, l_SectionStart, (float)l_I));
                                        l_SectionStart = (float)l_I;
                                    }
                                }
                            }
                        }
                        else
                        {
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
                            }



                        }

                        m_GraphData = l_GraphData.ToArray();
                    }

                    ///for (var l_I = 0; l_I < m_GraphData.Length; l_I++)
                    ///{
                    ///   var l_NPSInfos = m_GraphData[l_I];
                    ///    Logger.Instance.Debug($"l_GraphData.Add(({l_NPSInfos.Item1.ToString().Replace(",", ".")}, {l_NPSInfos.Item2.ToString().Replace(",", ".")}, {l_NPSInfos.Item3.ToString().Replace(",", ".")} ));");
                    ///}

                    if (m_GraphData.Length > 0)
                    {
                        var l_GraphFrame = new GameObject("", typeof(RectTransform));
                        l_GraphFrame.transform.SetParent(transform, false);
                        (l_GraphFrame.transform as RectTransform).anchorMin         = Vector2.one * 0.5f;
                        (l_GraphFrame.transform as RectTransform).anchorMax         = Vector2.one * 0.5f;
                        (l_GraphFrame.transform as RectTransform).anchoredPosition  = Vector2.zero;
                        (l_GraphFrame.transform as RectTransform).sizeDelta         = new Vector2(1048, 559);
                        (l_GraphFrame.transform as RectTransform).pivot             = Vector2.one * 0.5f;
                        (l_GraphFrame.transform as RectTransform).offsetMin         = ((l_GraphFrame.transform as RectTransform).sizeDelta / 2) * -1f;
                        (l_GraphFrame.transform as RectTransform).offsetMax         = ((l_GraphFrame.transform as RectTransform).sizeDelta / 2) * 1f;

                        m_GraphCanvas = (new GameObject("", typeof(RectTransform)).transform as RectTransform);
                        m_GraphCanvas.gameObject.transform.SetParent(l_GraphFrame.transform, false);
                        m_GraphCanvas.anchorMin        = Vector2.zero;
                        m_GraphCanvas.anchorMax        = Vector2.zero;
                        m_GraphCanvas.anchoredPosition = new Vector2(524, 280);
                        m_GraphCanvas.sizeDelta        = new Vector2(970, 500);
                        m_GraphCanvas.pivot            = Vector2.one * 0.5f;
                        m_GraphCanvas.offsetMin        = new Vector2(39, 30);
                        m_GraphCanvas.offsetMax        = new Vector2(1009, 530);
                        m_GraphCanvas.gameObject.transform.localPosition    = new Vector3(2, 0, 0);
                        m_GraphCanvas.gameObject.transform.localScale       = Vector3.one * 0.1f;

                        BuildGraph();

                        m_Pointer = new GameObject("");
                        m_Pointer.transform.SetParent(m_GraphCanvas, false);

                        var l_Image = m_Pointer.AddComponent<Image>();
                        l_Image.useSpriteMesh   = true;
                        l_Image.color           = SCVConfig.Instance.CursorColor;

                        m_CurrentGraphDataIndex = BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu ? (m_GraphData.Length / 2) : 0;

                        var l_RectTransform = m_Pointer.GetComponent<RectTransform>();
                        l_RectTransform.sizeDelta = Vector2.one * 10f;
                        (m_Pointer.transform as RectTransform).anchorMin         = Vector2.zero;
                        (m_Pointer.transform as RectTransform).anchorMax         = Vector2.zero;
                        (m_Pointer.transform as RectTransform).anchoredPosition  = m_Points.Count > 0 ? m_Points[m_CurrentGraphDataIndex] : Vector2.zero;

                        BeatSaberPlus.SDK.Unity.GameObject.ChangerLayerRecursive(gameObject, LayerMask.NameToLayer("UI"));
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frames
        /// </summary>
        private void Update()
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                return;

            if (m_FlyingGameHUDRotation != null && m_FlyingGameHUDRotation && SCVConfig.Instance.FollowEnvironementRotation)
                transform.parent.rotation = m_FlyingGameHUDRotation.transform.rotation;

            if (m_GraphCanvas == null || m_Pointer == null || m_AudioTimeSyncController == null || !m_AudioTimeSyncController || m_AudioTimeSyncController .state == AudioTimeSyncController.State.Stopped)
                return;

            float l_CurrentSongPosition = m_AudioTimeSyncController.songTime;
            float l_PositionX           = (m_GraphCanvas.sizeDelta.x / m_AudioTimeSyncController.songLength) * l_CurrentSongPosition;
            float l_PositionY           = 0f;

            if (m_AudioTimeSyncController != null && m_GraphData.Length > 1)
            {
                if (m_GraphData[m_CurrentGraphDataIndex].Item3 < l_CurrentSongPosition && m_CurrentGraphDataIndex != (m_GraphData.Length - 1))
                    ++m_CurrentGraphDataIndex;

                float l_LerpFactor = (l_CurrentSongPosition - m_GraphData[m_CurrentGraphDataIndex].Item2) / (m_GraphData[m_CurrentGraphDataIndex].Item3 - m_GraphData[m_CurrentGraphDataIndex].Item2);
                l_PositionY = Mathf.Lerp(m_CurrentGraphDataIndex > 0 ? m_Points[m_CurrentGraphDataIndex - 1].y : m_Points[m_CurrentGraphDataIndex].y, m_Points[m_CurrentGraphDataIndex].y, l_LerpFactor);
            }

            (m_Pointer.transform as RectTransform).anchoredPosition = new Vector2(l_PositionX, l_PositionY);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Graph builder
        /// </summary>
        public void BuildGraph()
        {
            var l_MaxValue  = m_GraphData.Max(x => x.Item1);
            var l_MinValue  = m_GraphData.Min(x => x.Item1);

            var l_YDelta = l_MaxValue - l_MinValue;
            if (l_YDelta <= 0)
                l_YDelta = 2.5f;

            l_MaxValue = l_MaxValue + (l_YDelta * 0.1f);
            l_MinValue = Mathf.Max(0f, l_MinValue - (l_YDelta * 0.2f));

            Vector2 l_LastPoint = default;
            foreach (var l_Point in m_GraphData)
            {
                var l_PointX        = m_GraphCanvas.sizeDelta.x * ((l_Point.Item3 - l_Point.Item2) / m_GraphData.Last().Item3) + (m_Points.Count != 0 ? m_Points.Last().x : 0);
                var l_CurrentPoint  = new Vector2(l_PointX, (l_Point.Item1 - l_MinValue) / (l_MaxValue - l_MinValue) * m_GraphCanvas.sizeDelta.y);

                m_Points.Add(l_CurrentPoint);

                if (l_LastPoint != null)
                {
                    var l_Line = new GameObject("", typeof(Image));
                    l_Line.transform.SetParent(m_GraphCanvas, false);
                    l_Line.GetComponent<Image>().color = SCVConfig.Instance.LineColor;

                    var l_Direction = (l_CurrentPoint - l_LastPoint).normalized;
                    var l_Distance  = Vector2.Distance(l_LastPoint, l_CurrentPoint);

                    var l_RectTransform = l_Line.GetComponent<RectTransform>();
                    l_RectTransform.anchorMin           = new Vector2(0, 0);
                    l_RectTransform.anchorMax           = new Vector2(0, 0);
                    l_RectTransform.sizeDelta           = new Vector2(l_Distance, 2f);
                    l_RectTransform.anchoredPosition    = l_LastPoint + l_Direction * l_Distance * .5f;
                    l_RectTransform.localEulerAngles    = new Vector3(0, 0, Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg);
                }

                l_LastPoint = l_CurrentPoint;
            }

            if (l_LastPoint != default)
            {
                var l_Line = new GameObject("", typeof(Image));
                l_Line.transform.SetParent(m_GraphCanvas, false);
                l_Line.GetComponent<Image>().color = SCVConfig.Instance.LineColor;

                var l_Direction = (new Vector2(m_GraphCanvas.sizeDelta.x, 0) - l_LastPoint).normalized;
                var l_Distance  = Vector2.Distance(l_LastPoint, new Vector2(m_GraphCanvas.sizeDelta.x, 0));

                var l_RectTransform = l_Line.GetComponent<RectTransform>();
                l_RectTransform.anchorMin           = new Vector2(0, 0);
                l_RectTransform.anchorMax           = new Vector2(0, 0);
                l_RectTransform.sizeDelta           = new Vector2(l_Distance, 2f);
                l_RectTransform.anchoredPosition    = l_LastPoint + l_Direction * l_Distance * .5f;
                l_RectTransform.localEulerAngles    = new Vector3(0, 0, Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg);
            }

            if (SCVConfig.Instance.ShowNPSLegend)
            {
                var l_Font          = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().FirstOrDefault(x => x.name == "Teko-Medium SDF");
                var l_LabelCount    = 10;
                for (var l_I = 0; l_I <= l_LabelCount; l_I++)
                {
                    var l_NormalizedValue = l_I * 1f / l_LabelCount;

                    var l_LegendLabel = new GameObject("", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
                    l_LegendLabel.transform.SetParent(m_GraphCanvas.transform, false);
                    (l_LegendLabel.transform as RectTransform).anchorMin            = Vector2.zero;
                    (l_LegendLabel.transform as RectTransform).anchorMax            = Vector2.zero;
                    (l_LegendLabel.transform as RectTransform).sizeDelta            = new Vector2(160, 30);
                    (l_LegendLabel.transform as RectTransform).pivot                = Vector2.one * 0.5f;
                    (l_LegendLabel.transform as RectTransform).offsetMin            = new Vector2(-168.2f, 30.6f);
                    (l_LegendLabel.transform as RectTransform).offsetMax            = new Vector2(-8.2f, 60.6f);
                    (l_LegendLabel.transform as RectTransform).anchoredPosition     = new Vector2(-88.2f, 45.6f);
                    (l_LegendLabel.transform as RectTransform).anchoredPosition     = new Vector2(-10f, l_NormalizedValue * m_GraphCanvas.sizeDelta.y);
                    (l_LegendLabel.transform as RectTransform).localPosition        = new Vector3(-585.00f, (l_LegendLabel.transform as RectTransform).localPosition.y, (l_LegendLabel.transform as RectTransform).localPosition.z);
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().font        = l_Font;
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().alignment   = TMPro.TextAlignmentOptions.MidlineRight;
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().color       = SCVConfig.Instance.LegendColor;
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().text        = Mathf.Round(l_MinValue + (l_NormalizedValue * (l_MaxValue - l_MinValue))).ToString();
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().fontSize    = 25;
                    l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>().enabled     = System.Math.Round(l_MinValue + (l_NormalizedValue * (l_MaxValue - l_MinValue)), 2) >= 0f;

                    var l_LegendLine = new GameObject("", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    l_LegendLine.transform.SetParent(m_GraphCanvas, false);
                    (l_LegendLine.transform as RectTransform).anchorMin          = new Vector2(0.5f, 0);
                    (l_LegendLine.transform as RectTransform).anchorMax          = new Vector2(0.5f, 0);
                    (l_LegendLine.transform as RectTransform).sizeDelta          = new Vector2(970.0f, 3.0f);
                    (l_LegendLine.transform as RectTransform).pivot              = Vector2.one * 0.5f;
                    (l_LegendLine.transform as RectTransform).offsetMin          = new Vector2(-448.1f, 247.6f);
                    (l_LegendLine.transform as RectTransform).offsetMax          = new Vector2(521.9f, 250.6f);
                    (l_LegendLine.transform as RectTransform).anchoredPosition   = new Vector2(-4f, l_NormalizedValue * m_GraphCanvas.sizeDelta.y);
                    l_LegendLine.GetComponent<Image>().sprite                    = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
                    l_LegendLine.GetComponent<Image>().type                      = Image.Type.Simple;
                    l_LegendLine.GetComponent<Image>().color                     = SCVConfig.Instance.DashLineColor;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fill graph data with demo values
        /// </summary>
        private void FillGraphDataWithDemoValues()
        {
            var l_GraphData = new List<(float, float, float)>();
            l_GraphData.Add((0f, 0f, 2f));
            l_GraphData.Add((7.5f, 2f, 4f));
            l_GraphData.Add((12f, 4f, 6f));
            l_GraphData.Add((12.5f, 6f, 8f));
            l_GraphData.Add((11f, 8f, 12f));
            l_GraphData.Add((10.5f, 12f, 14f));
            l_GraphData.Add((17f, 14f, 16f));
            l_GraphData.Add((18f, 16f, 18f));
            l_GraphData.Add((14.5f, 18f, 20f));
            l_GraphData.Add((11f, 20f, 22f));
            l_GraphData.Add((15f, 22f, 24f));
            l_GraphData.Add((4f, 24f, 26f));
            l_GraphData.Add((2.5f, 26f, 28f));
            l_GraphData.Add((8.5f, 28f, 30f));
            l_GraphData.Add((9.333333f, 30f, 33f));
            l_GraphData.Add((7.5f, 33f, 35f));
            l_GraphData.Add((10.5f, 35f, 37f));
            l_GraphData.Add((9.5f, 37f, 39f));
            l_GraphData.Add((3f, 39f, 41f));
            l_GraphData.Add((5f, 41f, 43f));
            l_GraphData.Add((8f, 43f, 45f));
            l_GraphData.Add((13.5f, 45f, 47f));
            l_GraphData.Add((12.5f, 47f, 49f));
            l_GraphData.Add((9.5f, 49f, 51f));
            l_GraphData.Add((10f, 51f, 53f));
            l_GraphData.Add((13.5f, 53f, 55f));
            l_GraphData.Add((12.5f, 55f, 57f));
            l_GraphData.Add((8.5f, 57f, 59f));
            l_GraphData.Add((10f, 59f, 61f));
            l_GraphData.Add((9.666667f, 61f, 64f));
            l_GraphData.Add((12f, 64f, 66f));
            l_GraphData.Add((8f, 66f, 68f));
            l_GraphData.Add((7f, 68f, 70f));
            l_GraphData.Add((9.5f, 70f, 72f));
            l_GraphData.Add((8.5f, 72f, 74f));
            l_GraphData.Add((8f, 74f, 77f));
            l_GraphData.Add((8f, 77f, 79f));
            l_GraphData.Add((7f, 79f, 81f));
            l_GraphData.Add((8f, 81f, 83f));
            l_GraphData.Add((9f, 83f, 85f));
            l_GraphData.Add((9f, 85f, 87f));
            l_GraphData.Add((8.5f, 87f, 89f));
            l_GraphData.Add((9.666667f, 89f, 92f));
            l_GraphData.Add((9.333333f, 92f, 95f));
            l_GraphData.Add((7.5f, 95f, 97f));
            l_GraphData.Add((3f, 97f, 99f));
            l_GraphData.Add((4.5f, 99f, 101f));
            l_GraphData.Add((12f, 101f, 103f));
            l_GraphData.Add((14f, 103f, 106f));
            l_GraphData.Add((14.5f, 106f, 108f));
            l_GraphData.Add((30.5f, 108f, 110f));
            l_GraphData.Add((43f, 110f, 112f));
            l_GraphData.Add((15f, 112f, 114f));
            l_GraphData.Add((0.5f, 114f, 116f));
            l_GraphData.Add((0f, 116f, 118f));
            l_GraphData.Add((6f, 118f, 120f));
            l_GraphData.Add((11f, 120f, 122f));
            l_GraphData.Add((7f, 122f, 124f));
            l_GraphData.Add((15.5f, 124f, 126f));
            l_GraphData.Add((12f, 126f, 128f));
            l_GraphData.Add((3f, 128f, 130f));
            l_GraphData.Add((8.5f, 130f, 132f));
            l_GraphData.Add((4.5f, 132f, 134f));
            l_GraphData.Add((7.5f, 134f, 136f));
            l_GraphData.Add((10f, 136f, 138f));
            l_GraphData.Add((9.5f, 138f, 140f));
            l_GraphData.Add((5.5f, 140f, 142f));
            l_GraphData.Add((6.666667f, 142f, 145f));
            l_GraphData.Add((7.4f, 145f, 150f));
            l_GraphData.Add((7.5f, 150f, 152f));
            l_GraphData.Add((6f, 152f, 154f));
            l_GraphData.Add((7.5f, 154f, 156f));
            l_GraphData.Add((10.5f, 156f, 158f));
            l_GraphData.Add((16.5f, 158f, 160f));
            l_GraphData.Add((10.5f, 160f, 162f));
            l_GraphData.Add((0f, 162f, 164f));
            l_GraphData.Add((0f, 164f, 169f));
            l_GraphData.Add((9f, 169f, 171f));
            l_GraphData.Add((40f, 171f, 173f));
            l_GraphData.Add((45f, 173f, 175f));
            l_GraphData.Add((36.5f, 175f, 177f));
            l_GraphData.Add((11.5f, 177f, 179f));
            l_GraphData.Add((11f, 179f, 181f));
            l_GraphData.Add((15.5f, 181f, 183f));
            l_GraphData.Add((36.5f, 183f, 185f));
            l_GraphData.Add((40.33333f, 185f, 188f));
            l_GraphData.Add((18f, 188f, 190f));
            l_GraphData.Add((15.33333f, 190f, 193f));
            l_GraphData.Add((1.5f, 193f, 195f));
            l_GraphData.Add((0f, 195f, 197f));
            l_GraphData.Add((0f, 197f, 201f));

            m_GraphData = l_GraphData.ToArray();
        }
    }
}
