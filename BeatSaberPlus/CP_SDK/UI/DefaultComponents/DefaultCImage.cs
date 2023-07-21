using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CImage component
    /// </summary>
    public class DefaultCImage : Components.CImage
    {
        private RectTransform                   m_RTransform;
        private LayoutElement                   m_LElement;
        private Image                           m_ImageC;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform  => m_RTransform;
        public override LayoutElement   LElement    => m_LElement;
        public override Image           ImageC      => m_ImageC;

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
            m_RTransform.sizeDelta = new Vector2(5f, 5f);

            m_LElement = gameObject.AddComponent<LayoutElement>();

            m_ImageC = gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_ImageC.material                = UISystem.Override_GetUIMaterial();
            m_ImageC.type                    = Image.Type.Simple;
            m_ImageC.pixelsPerUnitMultiplier = 1;
            m_ImageC.sprite                  = UISystem.GetUIRectBGSprite();
            m_ImageC.preserveAspect          = true;
            m_ImageC.raycastTarget           = false;
        }
    }
}
