using CP_SDK.XUI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Manager view detail
    /// </summary>
    internal sealed class ManagerRightView : CP_SDK.UI.ViewController<ManagerRightView>
    {
        private XUIVLayout      m_Root;
        private XUIVScrollView  m_ScrollView;
        private XUIText         m_Description;
        private XUIText         m_Details;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK_BS.Game.BeatMaps.MapDetail m_PendingDetail   = null;
        private CP_SDK_BS.Game.BeatMaps.MapDetail m_LastDetail      = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information"),

                XUIHLayout.Make(
                    XUIVScrollView.Make(
                        XUIText.Make($"<align=\"center\"><alpha=#AA><i>No description...</i></align>")
                            .SetAlign(TMPro.TextAlignmentOptions.Left)
                            .Bind(ref m_Description)
                    )
                    .Bind(ref m_ScrollView)
                )
                .SetHeight(45)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                XUIVLayout.Make(
                    XUIText.Make("No map\nSelected")
                        .SetFontSize(3f)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetStyle(TMPro.FontStyles.Bold)
                        .Bind(ref m_Details)
                )
                .SetBackground(true)
                .SetWidth(85f),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Link song to chat",       OnLinkPressed)
                        .SetHeight(8f).SetWidth(40f),
                    XUIPrimaryButton.Make("Open in beatsaver.com",   OnBeatsaverPressed)
                        .SetHeight(8f).SetWidth(40f)
                )
                .SetBackground(true)
                .SetWidth(84f)
            )
            .Bind(ref m_Root)
            .SetBackground(true, null, true)
            .BuildUI(transform);
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
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected sealed override void OnViewDeactivation()
            => CloseAllModals();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set detail view visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        internal void SetVisible(bool p_Visible)
        {
            m_Root?.Element?.gameObject?.SetActive(p_Visible);

            if (CanBeUpdated)
                CloseAllModals();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set details
        /// </summary>
        /// <param name="p_Detail">p_Detail</param>
        internal void SetDetail(CP_SDK_BS.Game.BeatMaps.MapDetail p_Detail)
        {
            if (!CanBeUpdated)
            {
                m_PendingDetail = p_Detail;
                return;
            }

            string l_Description = System.Net.WebUtility.HtmlDecode(p_Detail.description.Replace("\r\n", "\n").Replace("<", "<\u200B").Replace(">", "\u200B>"));
            if (l_Description.Trim().Length == 0)
                l_Description = "<align=\"center\"><alpha=#AA><i>No description...</i></align>";
            else
                l_Description = "<alpha=#AA>" + l_Description;

            m_Description.SetText(l_Description);
            m_ScrollView.Element.ScrollTo(0, true);

            var l_Date = p_Detail.GetUploadTime();

            float   l_Vote      = (float)Math.Round((double)p_Detail.stats.score * 100f, 0);
            string  l_SubText   = $"Votes +{p_Detail.stats.upvotes}/-{p_Detail.stats.downvotes} ({l_Vote}%)\n";
            l_SubText += "Uploaded on " + CP_SDK.Misc.Time.MonthNames[l_Date.Month - 1] + " " + l_Date.Day + " of " + l_Date.Year;

            m_Details.SetText(l_SubText);

            m_LastDetail    = p_Detail;
            m_PendingDetail = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On link pressed
        /// </summary>
        private void OnLinkPressed()
        {
            ShowMessageModal("Song sent to the chat.");

            var l_Plugin = ChatRequest.Instance;
            l_Plugin.SendChatMessage($"\"{m_LastDetail.name.Replace(".", " . ")}\" by {m_LastDetail.metadata.levelAuthorName.Replace(".", " . ")} => https://beatsaver.com/maps/{m_LastDetail.id}", null, null);
        }
        /// <summary>
        /// On beat saver pressed
        /// </summary>
        private void OnBeatsaverPressed()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL("https://beatsaver.com/maps/" + m_LastDetail.id);
        }
    }
}
