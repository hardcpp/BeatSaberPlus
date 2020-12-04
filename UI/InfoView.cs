using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Info view UI controller
    /// </summary>
    internal class InfoView : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("CreditBackground")]
        internal GameObject CreditBackground = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Welcome to BeatSaberPlus by HardCPP#1985</b></u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = "Version 1.2.1";
        [UIValue("Line3")]
        private readonly string m_Line3 = " ";
        [UIValue("Line4")]
        private readonly string m_Line4 = " ";
        [UIValue("Line5")]
        private readonly string m_Line5 = " ";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                /// Update background color
                var l_Color = CreditBackground.GetComponent<ImageView>().color;
                l_Color.a = 0.5f;

                CreditBackground.GetComponent<ImageView>().color = l_Color;
            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemDisabling">Is screen system disabling</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            m_ParserParams.EmitEvent("CloseAllModals");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to discord
        /// </summary>
        [UIAction("click-btn-discord")]
        private void OnDiscordPressed()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://discord.gg/K4X94Ea");
        }
        /// <summary>
        /// Go to patreon
        /// </summary>
        [UIAction("click-btn-patreon")]
        private void OnPatreonPressed()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://www.patreon.com/BeatSaberPlus");
        }
    }
}
