using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_SongChartVisualizer.UI
{
    /// <summary>
    /// Floating panel view
    /// </summary>
    internal sealed class ChartFloatingPanelView : CP_SDK.UI.ViewController<ChartFloatingPanelView>
    {
        private const float GraphAreaWidth  = 970f;
        private const float GraphAreaHeight = 500f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private RectTransform                   m_GraphCanvas       = null;
        private Image                           m_Point             = null;
        private RectTransform                   m_PointRTransform   = null;
        private CP_SDK.Pool.ObjectPool<Image>   m_LinePool          = null;
        private List<TMPro.TextMeshProUGUI>     m_VLegendTexts      = new List<TMPro.TextMeshProUGUI>();
        private List<Image>                     m_VLegendLines      = new List<Image>();
        private List<Image>                     m_Lines             = new List<Image>(SongChartVisualizer.MaxPoints);
        private List<Vector2>                   m_Points            = new List<Vector2>(SongChartVisualizer.MaxPoints);
        private Data.Graph                      m_Graph             = null;
        private Transform                       m_RotationTarget    = null;
        private Transform                       m_RotationFollow    = null;
        private Func<float>                     m_GetSongTime       = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_GraphFrame = new GameObject("GraphFrame", typeof(RectTransform)).GetComponent<RectTransform>();
            l_GraphFrame.gameObject.layer = CP_SDK.UI.UISystem.UILayer;
            l_GraphFrame.SetParent(transform, false);
            l_GraphFrame.anchorMin          = new Vector2(0.5f, 0.5f);
            l_GraphFrame.anchorMax          = new Vector2(0.5f, 0.5f);
            l_GraphFrame.anchoredPosition   = new Vector2(0.0f, 0.0f);
            l_GraphFrame.sizeDelta          = new Vector2(GraphAreaWidth + 78.0f, GraphAreaHeight + 59.0f);
            l_GraphFrame.pivot              = new Vector2(0.5f, 0.5f);

            m_GraphCanvas = new GameObject("GraphCanvas", typeof(RectTransform)).GetComponent<RectTransform>();
            m_GraphCanvas.gameObject.layer = CP_SDK.UI.UISystem.UILayer;
            m_GraphCanvas.transform.SetParent(l_GraphFrame.transform, false);
            m_GraphCanvas.anchorMin         = new Vector2(  0.0f,   0.0f);
            m_GraphCanvas.anchorMax         = new Vector2(  0.0f,   0.0f);
            m_GraphCanvas.anchoredPosition  = new Vector2(524.0f, 280.0f);
            m_GraphCanvas.sizeDelta         = new Vector2(GraphAreaWidth, GraphAreaHeight);
            m_GraphCanvas.pivot             = new Vector2(  0.5f,   0.5f);
            m_GraphCanvas.localPosition     = new Vector3(  2.0f,   0.0f, 0.0f);
            m_GraphCanvas.localScale        = new Vector3(  0.1f,   0.1f, 0.1f);

            var l_Font          = CP_SDK.UI.UISystem.Override_GetUIFont();
            var l_LabelCount    = 10;
            for (var l_I = 0; l_I < l_LabelCount; ++l_I)
            {
                m_VLegendTexts.Add(CreateVLegendText(l_I * 1f / l_LabelCount, l_Font));
                m_VLegendLines.Add(CreateVLegendLine(l_I * 1f / l_LabelCount));
            }

            m_LinePool = new CP_SDK.Pool.ObjectPool<Image>(
                createFunc: () =>
                {
                    var l_Line = new GameObject("", typeof(RectTransform), typeof(Image));
                    l_Line.transform.SetParent(m_GraphCanvas, false);
                    l_Line.SetActive(false);

                    var l_RectTransform = l_Line.GetComponent<RectTransform>();
                    l_RectTransform.anchorMin = Vector2.zero;
                    l_RectTransform.anchorMax = Vector2.zero;

                    var l_Image = l_Line.GetComponent<Image>();
                    l_Image.raycastTarget   = false;
                    l_Image.maskable        = false;
                    l_Image.color           = SCVConfig.Instance.LineColor;

                    return l_Image;
                },
                actionOnGet: (x) =>
                {
                    x.color = SCVConfig.Instance.LineColor;
                    x.gameObject.SetActive(true);
                },
                actionOnRelease: (x) =>
                {
                    x.gameObject.SetActive(false);
                },
                actionOnDestroy: (x) =>
                {
                    GameObject.Destroy(x.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: SongChartVisualizer.MaxPoints,
                maxSize:         SongChartVisualizer.MaxPoints
            );

            m_Point = new GameObject("Point").AddComponent<Image>();
            m_Point.gameObject.layer = CP_SDK.UI.UISystem.UILayer;
            m_Point.transform.SetParent(m_GraphCanvas, false);
            m_Point.raycastTarget   = false;
            m_Point.maskable        = false;
            m_Point.useSpriteMesh   = true;
            m_Point.color           = SCVConfig.Instance.CursorColor;

            m_PointRTransform = m_Point.rectTransform;
            m_PointRTransform.sizeDelta         = new Vector2(10.0f, 10.0f);
            m_PointRTransform.anchorMin         = new Vector2( 0.0f,  0.0f);
            m_PointRTransform.anchorMax         = new Vector2( 0.0f,  0.0f);
            m_PointRTransform.anchoredPosition  = new Vector2( 0.0f,  0.0f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set graph
        /// </summary>
        /// <param name="p_Graph">New graph</param>
        internal void SetGraph(Data.Graph p_Graph)
        {
            var l_MinValue = p_Graph.MinY;
            var l_MaxValue = p_Graph.MaxY + 2;

            var l_YDelta = l_MaxValue - l_MinValue;
            if (l_YDelta <= 0)
                l_YDelta = 2.5f;

            l_MinValue = Mathf.Max(0f, l_MinValue - (l_YDelta * 0.2f));
            l_MaxValue = l_MaxValue + (l_YDelta * 0.1f);

            for (var l_I = 0; l_I < m_Lines.Count; ++l_I)
                m_LinePool.Release(m_Lines[l_I]);
            m_Lines.Clear();

            if (p_Graph.Points != null)
            {
                m_Points.Clear();

                var l_LastPoint  = default(Vector2);
                var l_PointCount = p_Graph.Points.Length;
                for (var l_I = 0; l_I < l_PointCount; ++l_I)
                {
                    var l_PointX        = (float)l_I * (GraphAreaWidth / l_PointCount);
                    var l_PointY        = ((p_Graph.Points[l_I].Y - l_MinValue) / (l_MaxValue - l_MinValue)) * GraphAreaHeight;
                    var l_CurrentPoint  = new Vector2(l_PointX, l_PointY);
                    var l_Line          = m_LinePool.Get();
                    var l_Direction     = (l_CurrentPoint - l_LastPoint).normalized;
                    var l_Distance      = Vector2.Distance(l_LastPoint, l_CurrentPoint);

                    l_Line.rectTransform.sizeDelta          = new Vector2(l_Distance, 2f);
                    l_Line.rectTransform.anchoredPosition   = l_LastPoint + l_Direction * l_Distance * 0.5f;
                    l_Line.rectTransform.localEulerAngles   = new Vector3(0, 0, Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg);

                    m_Lines.Add(l_Line);
                    m_Points.Add(l_CurrentPoint);

                    l_LastPoint = l_CurrentPoint;
                }

                if (l_LastPoint != default)
                {
                    var l_Line          = m_LinePool.Get();
                    var l_Direction     = (new Vector2(GraphAreaWidth, 0.0f) - l_LastPoint).normalized;
                    var l_Distance      = Vector2.Distance(l_LastPoint, new Vector2(GraphAreaWidth, 0.0f));

                    l_Line.rectTransform.sizeDelta          = new Vector2(l_Distance, 2f);
                    l_Line.rectTransform.anchoredPosition   = l_LastPoint + l_Direction * l_Distance * 0.5f;
                    l_Line.rectTransform.localEulerAngles   = new Vector3(0, 0, Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg);

                    m_Lines.Add(l_Line);
                    m_Points.Add(new Vector2(GraphAreaWidth, 0.0f));
                }
            }

            var l_VLegendTextsCount = m_VLegendTexts.Count;
            for (var l_I = 0; l_I < l_VLegendTextsCount; ++l_I)
            {
                var l_NormalizedValue = (l_I * 1f / l_VLegendTextsCount);
                m_VLegendTexts[l_I].text    = System.Math.Round(l_MinValue + (l_NormalizedValue * (l_MaxValue - l_MinValue))).ToString();
                m_VLegendTexts[l_I].enabled = System.Math.Round(l_MinValue + (l_NormalizedValue * (l_MaxValue - l_MinValue)), 2) >= 0f;
            }

            m_Graph = p_Graph;
        }
        /// <summary>
        /// Set rotation follow settings
        /// </summary>
        /// <param name="p_Target">Target of rotation</param>
        /// <param name="p_Follow">Rotation to follow</param>
        internal void SetRotationFollow(Transform p_Target, Transform p_Follow)
        {
            m_RotationTarget = p_Target;
            m_RotationFollow = p_Follow;
        }
        /// <summary>
        /// Set get song time function
        /// </summary>
        /// <param name="p_GetSongTimeFunction">New function</param>
        internal void SetGetSongTimeFunction(Func<float> p_GetSongTimeFunction)
            => m_GetSongTime = p_GetSongTimeFunction;
        /// <summary>
        /// Update visual style
        /// </summary>
        internal void UpdateStyle()
        {
            var l_VLegendTextsCount = m_VLegendTexts.Count;
            for (var l_I = 0; l_I < l_VLegendTextsCount; ++l_I)
            {
                m_VLegendTexts[l_I].color = SCVConfig.Instance.LegendColor;
                m_VLegendTexts[l_I].gameObject.SetActive(SCVConfig.Instance.ShowNPSLegend);
            }

            var l_VLegendLineCount = m_VLegendLines.Count;
            for (var l_I = 0; l_I < l_VLegendLineCount; ++l_I)
            {
                m_VLegendLines[l_I].color = SCVConfig.Instance.DashLineColor;
                m_VLegendLines[l_I].gameObject.SetActive(SCVConfig.Instance.ShowNPSLegend);
            }

            var l_LineCount = m_Lines.Count;
            for (var l_I = 0; l_I < l_LineCount; ++l_I)
                m_Lines[l_I].color = SCVConfig.Instance.LineColor;

            m_Point.color = SCVConfig.Instance.CursorColor;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On physic frame callback
        /// </summary>
        private void FixedUpdate()
        {
            if (m_Graph == null || m_GetSongTime == null || CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Playing)
                return;

            if (m_RotationFollow)
                m_RotationTarget.localRotation = m_RotationFollow.rotation;

            var l_Progress  = m_Graph.SongTimeToProgress(m_GetSongTime());
            var l_Low       = m_Points[m_Graph.ProgressToIndexLow(l_Progress)];
            var l_HI        = m_Points[m_Graph.ProgressToIndexHi(l_Progress)];
            var l_PointPos  = new Vector2(
                l_Progress * GraphAreaWidth,
                Mathf.Lerp(l_Low.y, l_HI.y, m_Graph.ProgressToIndexRelativeVirtual(l_Progress))
            );

            m_PointRTransform.anchoredPosition = l_PointPos;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create VLegend text
        /// </summary>
        /// <param name="p_PositionY">Position Y</param>
        /// <param name="p_Font">Font asset</param>
        /// <returns></returns>
        private TMPro.TextMeshProUGUI CreateVLegendText(float p_PositionY, TMPro.TMP_FontAsset p_Font)
        {
            var l_LegendLabel = new GameObject("", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            l_LegendLabel.layer = CP_SDK.UI.UISystem.UILayer;
            l_LegendLabel.transform.SetParent(m_GraphCanvas, false);

            var l_RectTransform = l_LegendLabel.GetComponent<RectTransform>();
            l_RectTransform.anchorMin           = Vector2.zero;
            l_RectTransform.anchorMax           = Vector2.zero;
            l_RectTransform.sizeDelta           = new Vector2( 160.0f, 30.0f);
            l_RectTransform.pivot               = new Vector2(   0.5f,  0.5f);
            l_RectTransform.anchoredPosition    = new Vector2( -10.0f, p_PositionY * m_GraphCanvas.sizeDelta.y);
            l_RectTransform.localPosition       = new Vector3(-585.0f, l_RectTransform.localPosition.y, l_RectTransform.localPosition.z);

            var l_Text = l_LegendLabel.GetComponent<TMPro.TextMeshProUGUI>();
            l_Text.font             = p_Font;
            l_Text.alignment        = TMPro.TextAlignmentOptions.MidlineRight;
            l_Text.color            = SCVConfig.Instance.LegendColor;
            l_Text.fontSize         = 25;
            l_Text.text             = "0";
            l_Text.raycastTarget    = false;
            l_Text.maskable         = false;

            l_LegendLabel.gameObject.SetActive(SCVConfig.Instance.ShowNPSLegend);

            return l_Text;
        }
        /// <summary>
        /// Create VLegend line
        /// </summary>
        /// <param name="p_PositionY">Position Y</param>
        /// <returns></returns>
        private Image CreateVLegendLine( float p_PositionY)
        {
            var l_LegendLine = new GameObject("", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            l_LegendLine.layer = CP_SDK.UI.UISystem.UILayer;
            l_LegendLine.transform.SetParent(m_GraphCanvas, false);

            var l_RectTransform = l_LegendLine.GetComponent<RectTransform>();
            l_RectTransform.anchorMin           = new Vector2( 0.5f, 0.0f);
            l_RectTransform.anchorMax           = new Vector2( 0.5f, 0.0f);
            l_RectTransform.sizeDelta           = new Vector2(m_GraphCanvas.sizeDelta.x, 3.0f);
            l_RectTransform.pivot               = new Vector2( 0.5f, 0.5f);
            l_RectTransform.anchoredPosition    = new Vector2(-4.0f, p_PositionY * m_GraphCanvas.sizeDelta.y);

            var l_Image = l_LegendLine.GetComponent<Image>();
            l_Image.sprite          = CP_SDK.Unity.SpriteU.CreateFromTexture(Texture2D.whiteTexture);
            l_Image.type            = Image.Type.Simple;
            l_Image.color           = SCVConfig.Instance.DashLineColor;
            l_Image.raycastTarget   = false;
            l_Image.maskable        = false;

            l_LegendLine.gameObject.SetActive(SCVConfig.Instance.ShowNPSLegend);

            return l_Image;
        }
    }
}
