using System;

namespace CP_SDK.UI
{
    /// <summary>
    /// Mod button
    /// </summary>
    public class ModButton
    {
        private string  m_Text;
        private string  m_Tooltip;
        private bool    m_Interactable;
        private Action  m_OnClick;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string Text
        {
            get => m_Text;
            set  { m_Text = value; ModMenu.FireOnModButtonChanged(this); }
        }
        public string Tooltip
        {
            get => m_Tooltip;
            set  { m_Tooltip = value; ModMenu.FireOnModButtonChanged(this); }
        }
        public bool Interactable
        {
            get => m_Interactable;
            set  { m_Interactable = value; ModMenu.FireOnModButtonChanged(this); }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Text">Button text</param>
        /// <param name="p_OnClick">Action when clicked</param>
        /// <param name="p_Tooltip">Tooltip text</param>
        /// <param name="p_Interactable">Is interactable?</param>
        public ModButton(string p_Text, Action p_OnClick, string p_Tooltip = null, bool p_Interactable = true)
        {
            m_Text          = p_Text;
            m_Tooltip       = p_Tooltip;
            m_Interactable  = p_Interactable;

            m_OnClick = p_OnClick;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fire on click event
        /// </summary>
        internal void FireOnClick()
        {
            try { m_OnClick?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ModButton.FireOnClick] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
