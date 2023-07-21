using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CTabControl component
    /// </summary>
    public class DefaultCTabControl : Components.CTabControl
    {
        private RectTransform                       m_RTransform;
        private LayoutElement                       m_LElement;
        private Components.CTextSegmentedControl    m_TextSegmentedControl;
        private Components.CVLayout                 m_Content;

        private (string, RectTransform)[] m_Tabs = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform  => m_RTransform;
        public override LayoutElement       LElement    => m_LElement;

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
            m_LElement.minHeight        = 15f;
            m_LElement.flexibleWidth    = 1000f;
            m_LElement.flexibleHeight   = 1000f;

            m_TextSegmentedControl = UISystem.TextSegmentedControlFactory.Create("TextSegmentedControl", transform);
            m_TextSegmentedControl.OnActiveChanged(TextSegmentedControl_OnActiveChanged);
            m_TextSegmentedControl.RTransform.anchorMin         = new Vector2(0.0f, 1.0f);
            m_TextSegmentedControl.RTransform.anchorMax         = new Vector2(1.0f, 1.0f);
            m_TextSegmentedControl.RTransform.pivot             = new Vector2(0.5f, 1.0f);
            m_TextSegmentedControl.RTransform.sizeDelta         = Vector2.zero;
            m_TextSegmentedControl.RTransform.anchoredPosition  = Vector3.zero;

            m_Content = UISystem.VLayoutFactory.Create("Content", transform);
            m_Content.RTransform.anchorMin                  = Vector2.zero;
            m_Content.RTransform.anchorMax                  = Vector2.one;
            m_Content.RTransform.pivot                      = new Vector2( 0.5f,  0.5f);
            m_Content.RTransform.sizeDelta                  = new Vector2( 0.0f, -7.0f);
            m_Content.RTransform.localPosition              = new Vector3( 0.0f, -3.5f, 0f);
            m_Content.CSizeFitter.horizontalFit             = ContentSizeFitter.FitMode.Unconstrained;
            m_Content.CSizeFitter.verticalFit               = ContentSizeFitter.FitMode.Unconstrained;
            m_Content.VLayoutGroup.childForceExpandHeight   = false;
            m_Content.SetBackground(true);
            m_Content.SetPadding(0);
            m_Content.SetSpacing(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active tab changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CTabControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true)
        {
            m_TextSegmentedControl.OnActiveChanged(p_Functor, p_Add);
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get active tab
        /// </summary>
        /// <returns></returns>
        public override int GetActiveTab()
            => m_TextSegmentedControl.GetActiveText();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set active tab
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public override Components.CTabControl SetActiveTab(int p_Index, bool p_Notify = true)
        {
            m_TextSegmentedControl.SetActiveText(p_Index, p_Notify);
            return this;
        }
        /// <summary>
        /// Set tabs
        /// </summary>
        /// <param name="p_Tabs">Tabs</param>
        /// <returns></returns>
        public override Components.CTabControl SetTabs(params (string, RectTransform)[] p_Tabs)
        {
            if (m_Tabs != null)
                throw new System.Exception("Tabs already set!");

            for (var l_I = 0; l_I < p_Tabs.Length; ++l_I)
            {
                p_Tabs[l_I].Item2.gameObject.SetActive(false);
                p_Tabs[l_I].Item2.SetParent(m_Content.RTransform, false);
                p_Tabs[l_I].Item2.localPosition     = Vector3.zero;
                p_Tabs[l_I].Item2.localScale        = Vector3.one;
                p_Tabs[l_I].Item2.localEulerAngles  = Vector3.zero;
                p_Tabs[l_I].Item2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
                p_Tabs[l_I].Item2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top,    0, 0);
                p_Tabs[l_I].Item2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,   0, 0);
                p_Tabs[l_I].Item2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right,  0, 0);
                p_Tabs[l_I].Item2.anchorMin         = Vector2.zero;
                p_Tabs[l_I].Item2.anchorMax         = Vector2.one;
            }

            m_Tabs = p_Tabs;

            m_TextSegmentedControl.SetTexts(p_Tabs.Select(x => x.Item1).ToArray());

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active text changed
        /// </summary>
        /// <param name="p_Index"></param>
        private void TextSegmentedControl_OnActiveChanged(int p_Index)
        {
            for (var l_I = 0; l_I < m_Tabs.Length; ++l_I)
                m_Tabs[l_I].Item2.gameObject.SetActive(l_I == p_Index);
        }
    }
}
