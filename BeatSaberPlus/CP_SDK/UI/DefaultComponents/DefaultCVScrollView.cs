using CP_SDK.Unity.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CVScrollView component
    /// </summary>
    public class DefaultCVScrollView : Components.CVScrollView
    {
        private RectTransform               m_RTransform;
        private LayoutElement               m_LElement;
        private Subs.SubVScrollIndicator    m_VScrollIndicator;
        private Components.CIconButton      m_UpButton;
        private Components.CIconButton      m_DownButton;
        private Image                       m_Handle;
        private RectTransform               m_ViewPort;
        private RectTransform               m_VScrollViewContent;
        private VerticalLayoutGroup         m_VLayoutGroup;
        private RectTransform               m_Container;

        private float m_Smooth = 8f;
        private float m_DestinationPos;
        private float m_ScrollBarWidth = 6f;

        public event Action<float> ScrollPositionChangedEvent;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform      => m_RTransform;
        public override LayoutElement   LElement        => m_LElement;
        public override RectTransform   Container       => m_Container;
        public override float           Position        => m_VScrollViewContent.anchoredPosition.y;
        public override float           ViewPortWidth   => m_ViewPort.rect.width;
        public override float           ScrollableSize  => Mathf.Max(ContentSize - ScrollPageSize, 0f);
        public override float           ScrollPageSize  => m_ViewPort.rect.height;
        public override float           ContentSize     => m_Container.rect.height;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            gameObject.layer = UISystem.UILayer;

            m_LElement = gameObject.AddComponent<LayoutElement>();

            m_RTransform = transform as RectTransform;
            m_RTransform.anchorMin      = new Vector2(0f, 1f);
            m_RTransform.anchorMax      = new Vector2(1f, 1f);
            m_RTransform.sizeDelta      = Vector2.zero;
            m_RTransform.localPosition  = Vector3.zero;

            ////////////////////////////////////////////////////////////////////////////

            var l_ScrollBar = new GameObject("ScrollBar", typeof(RectTransform)).transform as RectTransform;
            l_ScrollBar.gameObject.layer = UISystem.UILayer;
            l_ScrollBar.SetParent(transform, false);
            l_ScrollBar.anchorMin           = new Vector2(                      1f, 0f);
            l_ScrollBar.anchorMax           = new Vector2(                      1f, 1f);
            l_ScrollBar.sizeDelta           = new Vector2(        m_ScrollBarWidth, 0f);
            l_ScrollBar.anchoredPosition    = new Vector2(-(m_ScrollBarWidth / 2f), 0f);

            var l_ScrollBarBG = l_ScrollBar.gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
            l_ScrollBarBG.material                  = UISystem.Override_GetUIMaterial();
            l_ScrollBarBG.color                     = ColorU.WithAlpha("#202020", 0.7f);
            l_ScrollBarBG.pixelsPerUnitMultiplier   = 1;
            l_ScrollBarBG.type                      = Image.Type.Sliced;
            l_ScrollBarBG.raycastTarget             = false;
            l_ScrollBarBG.sprite                    = UISystem.GetUIRoundBGSprite();

            m_UpButton = UISystem.IconButtonFactory.Create("Up", l_ScrollBar);
            m_UpButton.LElement.enabled                         = false;
            m_UpButton.RTransform.pivot                         = new Vector2(0.5f, 0.5f);
            m_UpButton.RTransform.anchorMin                     = new Vector2(0.0f, 0.5f);
            m_UpButton.RTransform.anchorMax                     = new Vector2(1.0f, 1.0f);
            m_UpButton.RTransform.sizeDelta                     = Vector2.zero;
            m_UpButton.RTransform.anchoredPosition              = Vector2.zero;
            m_UpButton.IconImageC.rectTransform.localEulerAngles    = new Vector3( 0.0f,  0.0f, 180.0f);
            m_UpButton.IconImageC.rectTransform.pivot               = new Vector2( 0.5f,  0.0f);
            m_UpButton.IconImageC.rectTransform.anchorMin           = new Vector2( 0.5f,  1.0f);
            m_UpButton.IconImageC.rectTransform.anchorMax           = new Vector2( 0.5f,  1.0f);
            m_UpButton.IconImageC.rectTransform.anchoredPosition    = new Vector2( 0.0f, -2.0f);
            m_UpButton.IconImageC.rectTransform.sizeDelta           = new Vector2( 4.0f,  2.0f);
            m_UpButton.SetSprite(UISystem.GetUIDownArrowSprite()).OnClick(OnUpButton);

            m_DownButton = UISystem.IconButtonFactory.Create("Down", l_ScrollBar);
            m_DownButton.LElement.enabled                       = false;
            m_DownButton.RTransform.pivot                       = new Vector2(0.5f, 0.5f);
            m_DownButton.RTransform.anchorMin                   = new Vector2(0.0f, 0.0f);
            m_DownButton.RTransform.anchorMax                   = new Vector2(1.0f, 0.5f);
            m_DownButton.RTransform.sizeDelta                   = Vector2.zero;
            m_DownButton.RTransform.anchoredPosition            = Vector2.zero;
            m_DownButton.IconImageC.rectTransform.pivot             = new Vector2( 0.5f,  0.0f);
            m_DownButton.IconImageC.rectTransform.anchorMin         = new Vector2( 0.5f,  0.0f);
            m_DownButton.IconImageC.rectTransform.anchorMax         = new Vector2( 0.5f,  0.0f);
            m_DownButton.IconImageC.rectTransform.anchoredPosition  = new Vector2( 0.0f,  2.0f);
            m_DownButton.IconImageC.rectTransform.sizeDelta         = new Vector2( 4.0f,  2.0f);
            m_DownButton.SetSprite(UISystem.GetUIDownArrowSprite()).OnClick(OnDownButton);

            var l_ScrollIndicator = new GameObject("ScrollIndicator", typeof(RectTransform)).GetComponent<RectTransform>();
            l_ScrollIndicator.gameObject.layer = UISystem.UILayer;
            l_ScrollIndicator.SetParent(l_ScrollBar.transform, false);
            l_ScrollIndicator.anchorMin = new Vector2(0.5f,   0.0f);
            l_ScrollIndicator.anchorMax = new Vector2(0.5f,   1.0f);
            l_ScrollIndicator.sizeDelta = new Vector2(1.6f, -12.0f);

            var l_ScrollIndicatorImage = l_ScrollIndicator.gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
            l_ScrollIndicatorImage.sprite           = UISystem.GetUIRoundBGSprite();
            l_ScrollIndicatorImage.color            = new Color(0f, 0f, 0f, 0.5f);
            l_ScrollIndicatorImage.type             = Image.Type.Sliced;
            l_ScrollIndicatorImage.material         = UISystem.Override_GetUIMaterial();
            l_ScrollIndicatorImage.raycastTarget    = false;

            m_Handle = new GameObject("Handle", typeof(RectTransform), UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_Handle.gameObject.layer = UISystem.UILayer;
            m_Handle.rectTransform.SetParent(l_ScrollIndicator.transform, false);
            m_Handle.rectTransform.pivot        = new Vector2(0.5f, 1.0f);
            m_Handle.rectTransform.anchorMin    = new Vector2(0.0f, 1.0f);
            m_Handle.rectTransform.anchorMax    = new Vector2(1.0f, 1.0f);
            m_Handle.rectTransform.sizeDelta    = Vector2.zero;
            m_Handle.sprite         = UISystem.GetUIRoundBGSprite();
            m_Handle.color          = new Color(1f, 1f, 1f, 0.5f);
            m_Handle.type           = Image.Type.Sliced;
            m_Handle.material       = UISystem.Override_GetUIMaterial();
            m_Handle.raycastTarget  = false;

            ////////////////////////////////////////////////////////////////////////////

            m_ViewPort = new GameObject("ViewPort", typeof(RectTransform), typeof(RectMask2D)).transform as RectTransform;
            m_ViewPort.gameObject.layer = UISystem.UILayer;
            m_ViewPort.SetParent(transform, false);
            m_ViewPort.anchorMin        = new Vector2(                      0f, 0f);
            m_ViewPort.anchorMax        = new Vector2(                      1f, 1f);
            m_ViewPort.sizeDelta        = new Vector2(       -m_ScrollBarWidth, 0f);
            m_ViewPort.localPosition    = new Vector3(-(m_ScrollBarWidth / 2f), 0f, 0f);
            m_ViewPort.GetComponent<RectMask2D>().padding = new Vector4(0.25f, 0.25f, 0.25f, 0.25f);

            m_VScrollViewContent = new GameObject("ScrollViewContent", typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            m_VScrollViewContent.gameObject.layer = UISystem.UILayer;
            m_VScrollViewContent.SetParent(m_ViewPort, false);
            m_VScrollViewContent.anchorMin  = new Vector2(0f, 1f);
            m_VScrollViewContent.anchorMax  = new Vector2(1f, 1f);
            m_VScrollViewContent.sizeDelta  = Vector2.zero;
            m_VScrollViewContent.pivot      = new Vector2(0.5f, 1f);

            var l_VScrollViewContent_ContentSizeFitter = m_VScrollViewContent.GetComponent<ContentSizeFitter>();
            l_VScrollViewContent_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            l_VScrollViewContent_ContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            var l_VScrollViewContent_VerticalLayoutGroup = m_VScrollViewContent.GetComponent<VerticalLayoutGroup>();
            l_VScrollViewContent_VerticalLayoutGroup.childForceExpandWidth   = false;
            l_VScrollViewContent_VerticalLayoutGroup.childForceExpandHeight  = false;
            l_VScrollViewContent_VerticalLayoutGroup.childControlWidth       = true;
            l_VScrollViewContent_VerticalLayoutGroup.childControlHeight      = true;
            l_VScrollViewContent_VerticalLayoutGroup.childScaleWidth         = false;
            l_VScrollViewContent_VerticalLayoutGroup.childScaleHeight        = false;
            l_VScrollViewContent_VerticalLayoutGroup.childAlignment          = TextAnchor.UpperCenter;

            ////////////////////////////////////////////////////////////////////////////

            m_Container = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).GetComponent<RectTransform>();
            m_Container.gameObject.layer = UISystem.UILayer;
            m_Container.SetParent(m_VScrollViewContent, false);
            m_Container.anchorMin = new Vector2(0f, 1f);
            m_Container.anchorMax = new Vector2(0f, 1f);

            m_VLayoutGroup = m_Container.GetComponent<VerticalLayoutGroup>();
            m_VLayoutGroup.childForceExpandWidth    = true;
            m_VLayoutGroup.childForceExpandHeight   = false;
            m_VLayoutGroup.childControlWidth        = true;
            m_VLayoutGroup.childControlHeight       = true;
            m_VLayoutGroup.childScaleWidth          = false;
            m_VLayoutGroup.childScaleHeight         = false;
            m_VLayoutGroup.childAlignment           = TextAnchor.LowerCenter;
            m_VLayoutGroup.padding                  = new RectOffset(2, 2, 2, 2);
            m_VLayoutGroup.spacing                  = 0;

            m_Container.sizeDelta = new Vector2(0f, -1f);

            ////////////////////////////////////////////////////////////////////////////

            enabled = false;

            m_VScrollIndicator = l_ScrollIndicator.gameObject.AddComponent<Subs.SubVScrollIndicator>();

            m_VScrollViewContent.gameObject.AddComponent<Subs.SubVScrollViewContent>().VScrollView = this;
            m_VScrollIndicator.Handle = m_Handle.rectTransform;

            UpdateContentSize();
            RefreshScrollButtons();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            var l_AnchoredPosition  = m_VScrollViewContent.anchoredPosition;
            var l_VPos              = Mathf.Lerp(l_AnchoredPosition.y, m_DestinationPos, Time.deltaTime * m_Smooth);

            if (Mathf.Abs(l_AnchoredPosition.y - m_DestinationPos) < 0.01f)
            {
                l_VPos = m_DestinationPos;
                enabled = false;
            }

            m_VScrollViewContent.anchoredPosition = new Vector2(0f, l_VPos);
            ScrollPositionChangedEvent?.Invoke(l_VPos);

            UpdateVScrollIndicator(Mathf.Abs(l_VPos));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On scroll changed
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CVScrollView OnScrollChanged(Action<float> p_Functor, bool p_Add = true)
        {
            if (p_Add)  ScrollPositionChangedEvent += p_Functor;
            else        ScrollPositionChangedEvent -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update content size
        /// </summary>
        public override Components.CVScrollView UpdateContentSize()
        {
            SetContentSize(m_VScrollViewContent.rect.height);
            ScrollTo(0f, p_Animated: false);
            return this;
        }
        /// <summary>
        /// Set content size
        /// </summary>
        /// <param name="p_ContentSize">New content size</param>
        public override Components.CVScrollView SetContentSize(float p_ContentSize)
        {
            m_VScrollViewContent.sizeDelta = new Vector2(m_VScrollViewContent.sizeDelta.x, p_ContentSize);
            m_VScrollIndicator.NormalizedPageHeight = m_ViewPort.rect.height / p_ContentSize;

            SetDestinationScrollPos(m_DestinationPos);
            RefreshScrollButtons();
            enabled = true;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Scroll to position
        /// </summary>
        /// <param name="p_TargetPosition">New target position</param>
        /// <param name="p_Animated">Is animated?</param>
        public override Components.CVScrollView ScrollTo(float p_TargetPosition, bool p_Animated)
        {
            SetDestinationScrollPos(p_TargetPosition);

            if (!p_Animated)
            {
                m_VScrollViewContent.anchoredPosition = new Vector2(0f, m_DestinationPos);
                ScrollPositionChangedEvent?.Invoke(m_DestinationPos);
            }

            RefreshScrollButtons();
            enabled = true;
            return this;
        }
        /// <summary>
        /// Scroll to end
        /// </summary>
        /// <param name="p_Animated">Is animated?</param>
        public override Components.CVScrollView ScrollToEnd(bool p_Animated)
            => ScrollTo(ContentSize - ScrollPageSize, p_Animated);
        /// <summary>
        /// Refresh scroll buttons
        /// </summary>
        public override Components.CVScrollView RefreshScrollButtons()
        {
            m_UpButton.SetInteractable(m_DestinationPos > 0.001f);
            m_DownButton.SetInteractable(m_DestinationPos < ContentSize - ScrollPageSize - 0.001f);
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set destination scroll position
        /// </summary>
        /// <param name="p_Value">New destination</param>
        private void SetDestinationScrollPos(float p_Value)
        {
            float l_Target = ContentSize - ScrollPageSize;
            if (l_Target < 0f)  m_DestinationPos = 0f;
            else                m_DestinationPos = Mathf.Clamp(p_Value, 0f, l_Target);
        }
        /// <summary>
        /// Update the vertical scroll indicator
        /// </summary>
        /// <param name="p_Position">Current position</param>
        private void UpdateVScrollIndicator(float p_Position) => m_VScrollIndicator.Progress = p_Position / (ContentSize - ScrollPageSize);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On up button pressed
        /// </summary>
        private void OnUpButton()
        {
            var l_Target = m_DestinationPos;
            switch (ScrollType)
            {
                case EScrollType.FixedCellSize:
                    l_Target -= FixedCellSize * (float)(Mathf.RoundToInt(ScrollPageSize / FixedCellSize) - 1);
                    l_Target  = Mathf.FloorToInt(l_Target / FixedCellSize) * FixedCellSize;
                    break;

                case EScrollType.PageSize:
                    l_Target -= PageStepNormalizedSize * ScrollPageSize;
                    break;
            }

            SetDestinationScrollPos(l_Target);
            RefreshScrollButtons();

            enabled = true;
        }
        /// <summary>
        /// On down button pressed
        /// </summary>
        private void OnDownButton()
        {
            var l_Target = m_DestinationPos;
            switch (ScrollType)
            {
                case EScrollType.FixedCellSize:
                    l_Target += FixedCellSize * (float)(Mathf.RoundToInt(ScrollPageSize / FixedCellSize) - 1);
                    l_Target  = Mathf.CeilToInt(l_Target / FixedCellSize) * FixedCellSize;
                    break;

                case EScrollType.PageSize:
                    l_Target += PageStepNormalizedSize * ScrollPageSize;
                    break;
            }

            SetDestinationScrollPos(l_Target);
            RefreshScrollButtons();

            enabled = true;
        }
    }
}
