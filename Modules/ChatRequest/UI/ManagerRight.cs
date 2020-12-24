using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatRequest.UI
{
    /// <summary>
    /// Manager view detail
    /// </summary>
    internal class ManagerRight : SDK.UI.ResourceViewController<ManagerRight>
    {
        /// <summary>
        /// Month list
        /// </summary>
        private static string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("DetailBackground")]
        private GameObject m_DetailBackground = null;
        [UIObject("SubDetailBackground")]
        private GameObject m_SubDetailBackground = null;
        [UIComponent("DetailText")]
        private HMUI.TextPageScrollView m_DetailText = null;
        [UIObject("SubDetailText")]
        private GameObject m_SubDetailText = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pending detail
        /// </summary>
        private BeatSaverSharp.Beatmap m_PendingDetail = null;
        /// <summary>
        /// Last detail
        /// </summary>
        private BeatSaverSharp.Beatmap m_LastDetail = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            SDK.UI.Backgroundable.SetOpacity(m_DetailBackground, 0.5f);
            SDK.UI.Backgroundable.SetOpacity(m_SubDetailBackground, 0.5f);

            m_SubDetailText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            if (m_PendingDetail != null)
                SetDetail(m_PendingDetail);
            else
                SetVisible(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set detail view visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        internal void SetVisible(bool p_Visible)
        {
            if (!CanBeUpdated || transform.childCount == 0)
                return;

            transform.GetChild(0).gameObject.SetActive(p_Visible);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set details
        /// </summary>
        /// <param name="p_Detail">p_Detail</param>
        internal void SetDetail(BeatSaverSharp.Beatmap p_Detail, bool p_SecondTime = false)
        {
            if (!CanBeUpdated)
            {
                m_PendingDetail = p_Detail;
                return;
            }

            string l_Description = "<line-height=125%>" + System.Net.WebUtility.HtmlEncode(p_Detail.Description.Replace("\r\n", "\n"));

            if (l_Description.Trim().Length == "<line-height=125%>".Length)
                l_Description = "<align=\"center\"><alpha=#AA><i>No description...</i></align>";

            l_Description += "\n\n\n\n\n\n\n\n\n\n ";

            m_DetailText.SetText(l_Description);
            m_DetailText.ScrollTo(0, true);

            float   l_Vote      = (float)Math.Round((double)p_Detail.Stats.Rating * 100f, 0);
            string  l_SubText   = $"Votes +{p_Detail.Stats.UpVotes}/-{p_Detail.Stats.DownVotes} ({l_Vote}%) {p_Detail.Stats.Downloads} downloads\n";
            l_SubText += "Uploaded on " + s_Months[p_Detail.Uploaded.Month - 1] + " " + p_Detail.Uploaded.Day + " of " + p_Detail.Uploaded.Year;

            m_SubDetailText.GetComponent<TextMeshProUGUI>().text = l_SubText;

            m_LastDetail    = p_Detail;
            m_PendingDetail = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On link pressed
        /// </summary>
        [UIAction("click-btn-link")]
        private void OnLinkPressed()
        {
            ShowMessageModal("Song sent to the chat.");

            var l_Plugin = ChatRequest.Instance;
            l_Plugin.SendChatMessage($"\"{m_LastDetail.Name}\" by {m_LastDetail.Metadata.LevelAuthorName} => https://beatsaver.com/beatmap/{m_LastDetail.Key}");
        }
        /// <summary>
        /// On beat saver pressed
        /// </summary>
        [UIAction("click-btn-beatsaver")]
        private void OnBeatsaverPressed()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://beatsaver.com/beatmap/" + m_LastDetail.Key);
        }
    }
}
