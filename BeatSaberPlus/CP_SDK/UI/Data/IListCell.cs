using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.UI.Data
{
    /// <summary>
    /// Abstract List Cell component
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class IListCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform       m_RTransform;
        private Image               m_Image;
        private Button              m_Button;
        private Components.CVXList  m_OwnerList;
        private int                 m_Index;
        private IListItem           m_ListItem;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public RectTransform        RTransform  => m_RTransform;
        public Components.CVXList   OwnerList   => m_OwnerList;
        public int                  Index       => m_Index;
        public IListItem            ListItem    => m_ListItem;
        public string               Tooltip;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create cell instance
        /// </summary>
        /// <param name="p_Parent">Parent</param>
        /// <returns></returns>
        public IListCell Create(RectTransform p_Parent)
        {
            var l_NewCell = AddSelfComponent(new GameObject("ListCell", typeof(RectTransform), UISystem.Override_UnityComponent_Image, typeof(Button)));
            l_NewCell.transform.SetParent(p_Parent, false);
            l_NewCell.gameObject.SetActive(false);
            l_NewCell.Build();

            return l_NewCell;
        }
        /// <summary>
        /// Bind to list
        /// </summary>
        /// <param name="p_OwnerList">Owner list</param>
        /// <param name="p_Index">List item index</param>
        /// <param name="p_ListItem">List item instance</param>
        public void Bind(Components.CVXList p_OwnerList, int p_Index, IListItem p_ListItem)
        {
            m_OwnerList = p_OwnerList;
            m_Index     = p_Index;
            m_ListItem  = p_ListItem;

            SetState(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        public virtual void Build()
        {
            if (m_RTransform)
                return;

            m_RTransform = GetComponent<RectTransform>();

            m_Image = gameObject.GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_Image.material                = UISystem.Override_GetUIMaterial();
            m_Image.type                    = Image.Type.Sliced;
            m_Image.pixelsPerUnitMultiplier = 1;
            m_Image.sprite                  = UISystem.GetUIRoundBGSprite();
            m_Image.preserveAspect          = false;

            m_Button = gameObject.GetComponent<Button>();
            m_Button.targetGraphic  = m_Image;
            m_Button.transition     = Selectable.Transition.ColorTint;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(Button_OnClick);

            SetState(false);
        }
        /// <summary>
        /// Set list cell state
        /// </summary>
        /// <param name="p_State">New state</param>
        public void SetState(bool p_State)
        {
            if (!m_RTransform)
                return;

            var l_IsOdd  = ((Index & 1) != 0) ? true : false;
            var l_Colors = m_Button.colors;
            l_Colors.normalColor        = new Color32(255, 255, 255, p_State ? (byte)100 : (l_IsOdd ? (byte)15 : (byte)0));
            l_Colors.highlightedColor   = new Color32(255, 255, 255, p_State ? (byte)100 : (byte)75);
            l_Colors.pressedColor       = new Color32(255, 255, 255, p_State ? (byte)100 : (byte)75);
            l_Colors.selectedColor      = l_Colors.normalColor;
            l_Colors.disabledColor      = new Color32(127, 127, 127, p_State ? (byte)100 : (byte)75);
            l_Colors.fadeDuration       = 0.05f;
            m_Button.colors = l_Colors;
        }
        /// <summary>
        /// Set is selectable
        /// </summary>
        /// <param name="p_Selectable">New state</param>
        public void SetSelectable(bool p_Selectable)
        {
            if (!m_Button || !m_Image)
                return;

            m_Image.enabled = p_Selectable;
            m_Button.enabled = p_Selectable;
        }
        /// <summary>
        /// Get cell height
        /// </summary>
        /// <returns></returns>
        public abstract float GetCellHeight();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add self component
        /// </summary>
        /// <param name="p_Target">Target gameobject</param>
        /// <returns></returns>
        protected abstract IListCell AddSelfComponent(GameObject p_Target);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cell click
        /// </summary>
        private void Button_OnClick()
        {
            if (m_OwnerList)
                m_OwnerList.OnListCellClicked(this);

            UISystem.Override_OnClick(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On pointer enter
        /// </summary>
        /// <param name="p_EventData"></param>
        public void OnPointerEnter(PointerEventData p_EventData)
        {
            if (string.IsNullOrEmpty(Tooltip))
                return;

            var l_ViewController = GetComponentInParent<IViewController>();
            if (!l_ViewController)
                return;

            var l_Rect  = RTransform.rect;
            var l_RPos  = new Vector2(l_Rect.x + l_Rect.width / 2f, l_Rect.y + l_Rect.height);
            var l_Pos   = RTransform.TransformPoint(l_RPos);
            l_ViewController.ShowTooltip(l_Pos, Tooltip);
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
