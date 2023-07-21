using CP_SDK.UI.Components;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CPrimaryButton component
    /// </summary>
    public class DefaultCPrimaryButton : Components.CPrimaryButton, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform               m_RTransform;
        private ContentSizeFitter           m_CSizeFitter;
        private LayoutElement               m_LElement;
        private Button                      m_Button;
        private Image                       m_BackgroundImage;
        private Image                       m_IconImage;
        private CText                       m_Label;
        private Subs.SubStackLayoutGroup    m_StackLayoutGroup;

        private string                      m_Tooltip;
        private event Action                m_OnClickEvent;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform          => m_RTransform;
        public override ContentSizeFitter   CSizeFitter         => m_CSizeFitter;
        public override LayoutGroup         LayoutGroupC        => m_StackLayoutGroup;
        public override LayoutElement       LElement            => m_LElement;
        public override Button              ButtonC             => m_Button;
        public override Image               BackgroundImageC    => m_BackgroundImage;
        public override Image               IconImageC          => m_IconImage;
        public override CText               TextC               => m_Label;

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

            m_RTransform = transform as RectTransform;

            m_CSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            m_CSizeFitter.horizontalFit   = ContentSizeFitter.FitMode.PreferredSize;
            m_CSizeFitter.verticalFit     = ContentSizeFitter.FitMode.PreferredSize;

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.minHeight = 5f;

            m_BackgroundImage = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_BackgroundImage.gameObject.layer = UISystem.UILayer;
            m_BackgroundImage.rectTransform.SetParent(transform, false);
            m_BackgroundImage.rectTransform.anchorMin     = Vector2.zero;
            m_BackgroundImage.rectTransform.anchorMax     = Vector2.one;
            m_BackgroundImage.rectTransform.sizeDelta     = Vector2.zero;
            m_BackgroundImage.rectTransform.localPosition = Vector2.zero;
            m_BackgroundImage.material                = UISystem.Override_GetUIMaterial();
            m_BackgroundImage.color                   = UISystem.PrimaryColor;
            m_BackgroundImage.type                    = Image.Type.Sliced;
            m_BackgroundImage.pixelsPerUnitMultiplier = 1;
            m_BackgroundImage.sprite                  = UISystem.GetUIButtonSprite();

            m_Label = UISystem.TextFactory.Create("Label", transform);
            m_Label.SetMargins(2.0f, 0.0f, 2.0f, 0.0f);
            m_Label.SetAlign(TMPro.TextAlignmentOptions.Capline);
            m_Label.SetStyle(FontStyles.Bold);

            m_IconImage = new GameObject("Icon", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_IconImage.gameObject.layer = UISystem.UILayer;
            m_IconImage.rectTransform.SetParent(transform, false);
            m_IconImage.rectTransform.anchorMin     = Vector2.zero;
            m_IconImage.rectTransform.anchorMax     = Vector2.one;
            m_IconImage.rectTransform.sizeDelta     = Vector2.zero;
            m_IconImage.rectTransform.localPosition = Vector2.zero;
            m_IconImage.material                = UISystem.Override_GetUIMaterial();
            m_IconImage.type                    = Image.Type.Simple;
            m_IconImage.pixelsPerUnitMultiplier = 1;
            m_IconImage.preserveAspect          = true;
            m_IconImage.gameObject.SetActive(false);

            m_Button = gameObject.AddComponent<Button>();
            m_Button.targetGraphic  = m_BackgroundImage;
            m_Button.transition     = Selectable.Transition.ColorTint;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(Button_OnClick);

            var l_Colors = m_Button.colors;
            l_Colors.normalColor        = new Color32(255, 255, 255, 180);
            l_Colors.highlightedColor   = new Color32(255, 255, 255, 255);
            l_Colors.pressedColor       = new Color32(200, 200, 200, 255);
            l_Colors.selectedColor      = l_Colors.normalColor;
            l_Colors.disabledColor      = new Color32(255, 255, 255,  48);
            l_Colors.fadeDuration       = 0.05f;
            m_Button.colors = l_Colors;

            m_StackLayoutGroup = gameObject.AddComponent<Subs.SubStackLayoutGroup>();
            m_StackLayoutGroup.ChildForceExpandWidth    = true;
            m_StackLayoutGroup.ChildForceExpandHeight   = true;
            m_StackLayoutGroup.childAlignment           = TextAnchor.MiddleCenter;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CPOrSButton OnClick(Action p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnClickEvent += p_Functor;
            else        m_OnClickEvent -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set tooltip
        /// </summary>
        /// <param name="p_Tooltip">New tooltip</param>
        /// <returns></returns>
        public override Components.CPOrSButton SetTooltip(string p_Tooltip)
        {
            m_Tooltip = p_Tooltip;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click unity callback
        /// </summary>
        private void Button_OnClick()
        {
            try { m_OnClickEvent?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCPrimaryButton.Button_OnClick] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            UISystem.Override_OnClick?.Invoke(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On pointer enter
        /// </summary>
        /// <param name="p_EventData"></param>
        public void OnPointerEnter(PointerEventData p_EventData)
        {
            if (string.IsNullOrEmpty(m_Tooltip))
                return;

            var l_ViewController = GetComponentInParent<IViewController>();
            if (!l_ViewController)
                return;

            var l_Rect = RTransform.rect;
            var l_RPos = new Vector2(l_Rect.x + l_Rect.width / 2.0f, l_Rect.y + l_Rect.height);
            var l_Pos = RTransform.TransformPoint(l_RPos);
            l_ViewController.ShowTooltip(l_Pos, m_Tooltip);
        }
        /// <summary>
        /// On pointer exit
        /// </summary>
        /// <param name="p_EventData"></param>
        public void OnPointerExit(PointerEventData p_EventData)
        {
            var l_ViewController = GetComponentInParent<IViewController>();
            if (!l_ViewController)
                return;

            l_ViewController.HideTooltip();
        }
    }
}
