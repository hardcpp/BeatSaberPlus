using CP_SDK.Unity.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI
{
    /// <summary>
    /// Tooltip widget
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        private RectTransform           m_RTransform;
        private HorizontalLayoutGroup   m_HorizontalLayoutGroup;
        private ContentSizeFitter       m_ContentSizeFitter;
        private Image                   m_Image;
        private Components.CImage       m_Border;
        private Components.CText        m_Text;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a tooltip
        /// </summary>
        /// <param name="p_Parent">Parent container</param>
        /// <returns></returns>
        public static Tooltip Create(RectTransform p_Parent)
        {
            var l_Tooltip = new GameObject("Tooltip").AddComponent<Tooltip>();
            l_Tooltip.m_RTransform = l_Tooltip.gameObject.AddComponent<RectTransform>();
            l_Tooltip.m_RTransform.SetParent(p_Parent, false);
            l_Tooltip.m_RTransform.localRotation    = Quaternion.identity;
            l_Tooltip.m_RTransform.localScale       = Vector3.one;
            l_Tooltip.m_RTransform.anchorMin        = new Vector2(0.5f, 0.0f);
            l_Tooltip.m_RTransform.anchorMax        = new Vector2(0.5f, 0.0f);
            l_Tooltip.m_RTransform.pivot            = new Vector2(0.5f, 0.0f);

            l_Tooltip.m_HorizontalLayoutGroup = l_Tooltip.gameObject.AddComponent<HorizontalLayoutGroup>();
            l_Tooltip.m_HorizontalLayoutGroup.padding = new RectOffset(2, 2, 2, 2);

            l_Tooltip.m_ContentSizeFitter = l_Tooltip.gameObject.AddComponent<ContentSizeFitter>();
            l_Tooltip.m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            l_Tooltip.m_ContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            l_Tooltip.m_Image = l_Tooltip.gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
            l_Tooltip.m_Image.material                  = UISystem.Override_GetUIMaterial();
            l_Tooltip.m_Image.type                      = Image.Type.Sliced;
            l_Tooltip.m_Image.pixelsPerUnitMultiplier   = 1;
            l_Tooltip.m_Image.sprite                    = UISystem.GetUIRoundBGSprite();
            l_Tooltip.m_Image.raycastTarget             = false;
            l_Tooltip.m_Image.color                     = UISystem.TooltipBGColor;
            l_Tooltip.m_Image.maskable                  = false;

            l_Tooltip.m_Border = UISystem.ImageFactory.Create("Text", l_Tooltip.m_RTransform);
            l_Tooltip.m_Border.SetSprite(UISystem.GetUIRoundSmoothFrameSprite());
            l_Tooltip.m_Border.SetColor(ColorU.WithAlpha(Color.white, 0.80f));
            l_Tooltip.m_Border.SetType(Image.Type.Sliced);
            l_Tooltip.m_Border.LElement.ignoreLayout = true;
            l_Tooltip.m_Border.RTransform.anchorMin         = Vector2.zero;
            l_Tooltip.m_Border.RTransform.anchorMax         = Vector2.one;
            l_Tooltip.m_Border.RTransform.anchoredPosition  = Vector2.zero;
            l_Tooltip.m_Border.RTransform.sizeDelta         = Vector2.zero;

            l_Tooltip.m_Text = UISystem.TextFactory.Create("Text", l_Tooltip.m_RTransform);
            l_Tooltip.m_Text.SetText("Tooltip");
            l_Tooltip.m_Text.SetFontSize(3.8f);
            l_Tooltip.m_Text.SetColor(Color.white);

            l_Tooltip.gameObject.SetActive(false);

            return l_Tooltip;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the tooltip
        /// </summary>
        /// <param name="p_Position">World position</param>
        /// <param name="p_Text">Tooltip text</param>
        public void Show(Vector3 p_Position, string p_Text)
        {
            m_Text.SetText(p_Text);
            m_RTransform.position = p_Position;

            gameObject.SetActive(true);
        }
        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
