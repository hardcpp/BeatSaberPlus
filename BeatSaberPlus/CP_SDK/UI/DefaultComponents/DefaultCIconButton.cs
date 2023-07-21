using CP_SDK.Unity.Extensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CIconButton component
    /// </summary>
    public class DefaultCIconButton : Components.CIconButton, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform   m_RTransform;
        private LayoutElement   m_LElement;
        private Image           m_IconImage;
        private Button          m_Button;

        private event Action    m_OnClick;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform  => m_RTransform;
        public override LayoutElement       LElement    => m_LElement;
        public override Button              ButtonC     => m_Button;
        public override Image               IconImageC  => m_IconImage;

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

            m_LElement = gameObject.AddComponent<LayoutElement>();

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
            m_IconImage.sprite                  = UISystem.GetUIDownArrowSprite();
            m_IconImage.preserveAspect          = true;
            m_IconImage.raycastTarget           = false;

            m_Button = gameObject.AddComponent<Button>();
            m_Button.targetGraphic  = m_IconImage;
            m_Button.transition     = Selectable.Transition.ColorTint;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(Button_OnClick);

            var l_FakeBg = gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
            l_FakeBg.material                   = UISystem.Override_GetUIMaterial();
            l_FakeBg.type                       = Image.Type.Simple;
            l_FakeBg.pixelsPerUnitMultiplier    = 1;
            l_FakeBg.sprite                     = UISystem.GetUIRectBGSprite();
            l_FakeBg.color                      = ColorU.WithAlpha(Color.black, 0.01f);

            var l_Colors = m_Button.colors;
            l_Colors.normalColor        = new Color32(255, 255, 255, 150);
            l_Colors.highlightedColor   = new Color32(255, 255, 255, 255);
            l_Colors.pressedColor       = new Color32(255, 255, 255, 255);
            l_Colors.selectedColor      = l_Colors.normalColor;
            l_Colors.disabledColor      = new Color32(127, 127, 127, 150);
            l_Colors.fadeDuration       = 0.05f;
            m_Button.colors = l_Colors;

            SetWidth(3.6f);
            SetHeight(2.3f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click unity callback
        /// </summary>
        private void Button_OnClick()
        {
            try { m_OnClick?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCIconButton.Button_OnClick] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            UISystem.Override_OnClick?.Invoke(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CIconButton OnClick(Action p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnClick += p_Functor;
            else        m_OnClick -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On pointer enter
        /// </summary>
        /// <param name="p_EventData"></param>
        public void OnPointerEnter(PointerEventData p_EventData)
        {
            if (!m_Button.interactable)
                return;

            StopAllCoroutines();
            StartCoroutine(Coroutine_AnimateScale(1.25f * Vector3.one, 0.075f));
        }
        /// <summary>
        /// On pointer exit
        /// </summary>
        /// <param name="p_EventData"></param>
        public void OnPointerExit(PointerEventData p_EventData)
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_AnimateScale(Vector3.one, 0.075f));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Coroutine animate scale
        /// </summary>
        /// <param name="p_Target">Target scale</param>
        /// <param name="p_Time">Animation time</param>
        /// <returns></returns>
        private IEnumerator Coroutine_AnimateScale(Vector3 p_Target, float p_Time)
        {
            var l_Waiter = new WaitForEndOfFrame();
            var l_DeltaT = 0.0f;

            while (l_DeltaT < p_Time)
            {
                l_DeltaT += Time.deltaTime;
                m_IconImage.rectTransform.localScale = Vector3.Lerp(m_IconImage.rectTransform.localScale, p_Target, l_DeltaT);

                yield return l_Waiter;
            }

            m_IconImage.rectTransform.localScale = p_Target;
        }
    }
}
