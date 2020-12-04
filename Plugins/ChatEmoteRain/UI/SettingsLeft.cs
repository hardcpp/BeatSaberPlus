using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatEmoteRain.UI
{
    /// <summary>
    /// Emote rain settings credits view
    /// </summary>
    internal class SettingsLeft : BSMLResourceViewController
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

#pragma warning disable CS0649
#pragma warning disable CS0414
        [UIObject("CreditBackground")]
        private GameObject m_CreditBackground;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>EmoteRain</b> - Let em' rain!</u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = " - Made by <b>Cr4</b> and <b>Uialeth</b>";
        [UIValue("Line3")]
        private readonly string m_Line3 = " - Check out the README at https://github.com/SetCr4/EmoteRain";
        [UIValue("Line4")]
        private readonly string m_Line4 = " ";
        [UIValue("Line5")]
        private readonly string m_Line5 = " ";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0414
#pragma warning restore CS0649

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
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if (p_FirstActivation)
            {
                /// Update background color
                Color l_Color = this.m_CreditBackground.GetComponent<ImageView>().color;
                l_Color.a = 0.5f;

                m_CreditBackground.GetComponent<ImageView>().color = l_Color;
            }
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
        /// Open ReadMe button pressed
        /// </summary>
        [UIAction("click-open-readme-btn-pressed")]
        private void OnOpenReadMePressed()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://github.com/SetCr4/EmoteRain/blob/master/README.md");
        }
    }
}
